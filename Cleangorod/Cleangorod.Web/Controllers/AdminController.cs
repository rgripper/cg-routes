using AutoMapper;
using Cleangorod.Data;
using Cleangorod.Data.Models;
using Cleangorod.Web.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Web.Http;

namespace Cleangorod.Web.Controllers
{
    [Authorize(Roles="Admin")]
    [RoutePrefix("api/Admin")]
    public class AdminController : IdentityAwareApiController
    {
        private ApplicationDbContext _Db = new ApplicationDbContext();

        public class PostDateSelectionbindingModel
        {
            public DateTime Date { get; set; }
            public bool Selected { get; set; }
        }

        public AdminController(ApplicationUserManager userManager, ApplicationRoleManager roleManager,
            ISecureDataFormat<AuthenticationTicket> accessTokenFormat)
            : base(userManager, roleManager, accessTokenFormat)
        {
            
        }

        public AdminController()
            : base()
        {

        }

        [Route("PostDateSelection")]
        public async Task<IHttpActionResult> PostDateSelection(PostDateSelectionbindingModel model)
        {
            if (model.Selected)
            {
                var ranges = _Db.CollectionDateTimeRanges.Where(x => x.Date == model.Date).ToList();
                if (ranges.Count == 0)
                {
                    foreach (var item in CreateRanges(model.Date))
                    {
                        _Db.CollectionDateTimeRanges.Add(item);
                    }
                }
                else
                {
                    foreach (var item in ranges)
                    {
                        item.Active = true;
                    }
                }
            }
            else
            {
                var ranges = _Db.CollectionDateTimeRanges
                    .Include(x => x.ClientSchedules)
                    .Where(x => x.Date == model.Date)
                    .ToList();
                foreach (var item in ranges)
                {
                    item.Active = false;
                }
            }

            await _Db.SaveChangesAsync();

            return Ok();
        }

        [HttpGet]
        [Route("DownloadSchedules")]
        public IHttpActionResult DownloadSchedules()
        {
            return DownloadSchedules(GetLastMonday(DateTime.Today));
        }

        private IHttpActionResult DownloadSchedules(DateTime weekStart)
        {
            var schedules = _Db.CollectionSchedules
                .Include(x => x.Client)
                .Include(x => x.Client.ClientAddress)
                .Include(x => x.Ranges)
                .Where(x => x.WeekStart == weekStart).ToList();

            var inputStream = new MemoryStream();
            var excelPackage = new ExcelPackage(inputStream);
            var worksheet = excelPackage.Workbook.Worksheets.Add("График вывозов");

            var valuesGroups = new List<List<object>>();
            valuesGroups.Add(new List<object> { "Имя", "Телефон", "Адрес", 
                weekStart.AddDays(5).ToString("dd MMM") + "(сб)", 
                weekStart.AddDays(6).ToString("dd MMM") + "(вс)", 
                "Пометки", "Email" });
            for (int i = 0; i < schedules.Count; i++)
            {
                var x = schedules[i];
                valuesGroups.Add(new List<object> 
                {
                    x.Client.Name + " " + x.Client.Surname,
                    x.Client.PhoneNumber,
                    x.Client.ClientAddress.Address,
                    String.Join(", ", x.Ranges
                        .Where(r => r.Date.DayOfWeek == DayOfWeek.Saturday)
                        .OrderBy(r => r.StartHour)
                        .Select(r => r.StartHour + "-" + r.EndHour)),
                    String.Join(", ", x.Ranges
                        .Where(r => r.Date.DayOfWeek == DayOfWeek.Sunday)
                        .OrderBy(r => r.StartHour)
                        .Select(r => r.StartHour + "-" + r.EndHour)),
                    x.Note,
                    x.Client.Email,
                });

            }

            for (int i = 0; i < valuesGroups.Count; i++)
            {
                var valueGroup = valuesGroups[i];
                for (int j = 0; j < valueGroup.Count; j++)
                {
                    worksheet.Cells[i + 1, j + 1].Value = valueGroup[j];
                }
            }

            worksheet.Cells.AutoFitColumns();

            excelPackage.Save();
            inputStream.Position = 0;
            return new FileActionResult(
                inputStream, 
                String.Format("Выезды за {0:M}-{1:M}.xlsx", weekStart.AddDays(5), weekStart.AddDays(6)),
                "application/vnd.ms-excel");
        }

        private static IEnumerable<CollectionDateTimeRange> CollapseRanges(IEnumerable<CollectionDateTimeRange> ranges)
        {
            CollectionDateTimeRange range = null;
            foreach (var item in ranges.OrderBy(x => x.StartHour))
            {
                if (range == null)
                {
                    range = item;
                    continue;
                }

                if (range.EndHour == item.StartHour)
                {
                    range.EndHour = item.EndHour;
                }
                else
                {
                    yield return range;
                    range = item;
                }
            }

            if (range != null)
            {
                yield return range;
            }
        }

        [Route("GetSelectedDates")]
        public IEnumerable<DateTime> GetSelectedDates(DateTime startDate, DateTime endDate)
        {
            startDate = startDate.ToUniversalTime();
            endDate = endDate.ToUniversalTime();
            return _Db.CollectionDateTimeRanges.Where(x => x.Date >= startDate && x.Date <= endDate).Select(x => x.Date).Distinct();
        }

        [Route("GetRouteClientsForDate")]
        public IEnumerable<RouteClientViewModel> GetRouteClientsForDate(DateTime date)
        {
            var result =
                _Db.CollectionSchedules
                    .Include(x => x.Ranges)
                    .Include(x => x.Client)
                    .Include(x => x.Client.ClientAddress)
                    .Where(x => x.Ranges.Any(r => r.Date == date))
                    .ToList();

            return result.Where(x => x.Client.ClientAddress.Latitude != null).Select(x => new RouteClientViewModel
            {
                Address = x.Client.ClientAddress.Address,
                Latitude = x.Client.ClientAddress.Latitude,
                Longitude = x.Client.ClientAddress.Longitude,
                Ranges = Mapper.Map<CollectionDateTimeRangeViewModel[]>(x.Ranges)
            });
        }

        private static DateTime GetLastMonday(DateTime date)
        {
            return date.AddDays(date.DayOfWeek == DayOfWeek.Sunday ? -6 : (DayOfWeek.Monday - date.DayOfWeek));
        }

        private static IEnumerable<CollectionDateTimeRange> CreateRanges(DateTime date)
        {
            var weekStart = GetLastMonday(date);
            return Enumerable.Range(8, 21 - 8).Where(x => x % 2 == 0).Select(x =>
                new CollectionDateTimeRange { Date = date, StartHour = x, EndHour = x + 2, WeekStart = weekStart, Active = true });
        }

    }
}
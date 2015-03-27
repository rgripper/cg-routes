using Cleangorod.Data.Models;
using Cleangorod.Web.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using AutoMapper;

namespace Cleangorod.Web.Controllers
{
    [Authorize]
    [RoutePrefix("api/Client")]
    public class ClientController : ApiController
    {
        private ApplicationDbContext _Db = new ApplicationDbContext();

        // POST api/Client/ChangeClientAddress
        [Route("ChangeClientAddress")]
        public async Task<IHttpActionResult> ChangeClientAddress(ChangeClientAddressViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            ApplicationDbContext context = new ApplicationDbContext();
            context.Set<ClientAddress>()
                .Add(new ClientAddress { Address = model.Address, Latitude = model.Latitude, Longitude = model.Longitude });


            var user = context.Set<ApplicationUser>().First(x => x.UserName == this.User.Identity.Name);

            if (user.ClientAddress == null)
            {
                user.ClientAddress = new ClientAddress();
            }

            user.ClientAddress.Latitude = model.Latitude;
            user.ClientAddress.Longitude = model.Longitude;
            user.ClientAddress.Address = model.Address;

            await context.SaveChangesAsync();

            return Ok();
        }

        public class PostScheduleBindingModel
        {
            public int? ScheduleId { get; set; }
            public DateTime WeekStart { get; set; }
            public string Note { get; set; }
            public IEnumerable<int> RangeIds { get; set; }
        }

        [Route("PostSchedule")]
        public async Task<IHttpActionResult> PostSchedule(PostScheduleBindingModel model)
        {
            // todo: isEmpty + note
            var isEmpty = !model.RangeIds.Any();
            if (isEmpty)
            {
                if (model.ScheduleId.HasValue)
                {
                    var schedule = new ClientSchedule { Id = model.ScheduleId.Value };

                    _Db.CollectionSchedules.Attach(schedule);
                    _Db.CollectionSchedules.Remove(schedule);
                    await _Db.SaveChangesAsync();
                }
                return Ok();
            }
            else 
            {
                ClientSchedule schedule = null;
                if (model.ScheduleId.HasValue)
                {
                    schedule = _Db.CollectionSchedules.Find(model.ScheduleId.Value);
                }
                else
                {
                    var userId = User.Identity.GetUserId();
                    schedule = _Db.CollectionSchedules.SingleOrDefault(x => x.Client.Id == userId && x.WeekStart == model.WeekStart);
                    if (schedule == null)
                    {
                        var client = _Db.Users.SingleOrDefault(x => x.Id == userId); // REVIEW
                        if (client == null)
                        {
                            throw new Exception("Could not find user");
                        }
                        schedule = new ClientSchedule { WeekStart = model.WeekStart, Client = client };
                        _Db.CollectionSchedules.Add(schedule);
                    }
                }

                schedule.Note = model.Note;

                schedule.Ranges.Clear();
                var ranges = _Db.CollectionDateTimeRanges.Where(x => model.RangeIds.Contains(x.Id)).ToList();
                foreach (var range in ranges)
	            {
                    schedule.Ranges.Add(range);
	            }
                
                await _Db.SaveChangesAsync();

                return Ok();
            }
        }

        [Route("GetSchedule")]
        public ClientScheduleViewModel GetSchedule()
        {
            var userId = User.Identity.GetUserId();
            var client = _Db.Users.SingleOrDefault(x => x.Id == userId);

            var now = DateTime.UtcNow;
            var availableRanges = _Db.GetDateTimeRangesToEndOfWeek(now);

            var startOfThisWeek = _Db.GetStartOfNextWeek(now).AddDays(-7);

            var schedule = _Db.GetScheduleForWeek(userId, now);
            var scheduleViewModel = schedule == null 
                ? new ClientScheduleViewModel { WeekStart = startOfThisWeek } 
                : Mapper.Map<ClientScheduleViewModel>(schedule);
            foreach (var item in scheduleViewModel.Ranges)
            {
                item.Selected = true;
            }

            var selectedRangesIds = new HashSet<int>(scheduleViewModel.Ranges.Select(x => x.Id));
            var unselectedRanges = availableRanges.Where(x => !selectedRangesIds.Contains(x.Id));
            var unselectedRangesViewModels = Mapper.Map<CollectionDateTimeRangeViewModel[]>(unselectedRanges);
            scheduleViewModel.Ranges = scheduleViewModel.Ranges.Concat(unselectedRangesViewModels).Where(x => x.Active);

            return scheduleViewModel;
            //var data = new
            //{
            //    Schedule = new CollectionScheduleViewModel
            //    {
            //        Note = "moo",
            //        Ranges = new List<CollectionDateTimeRangeViewModel> 
            //        { 
            //            new CollectionDateTimeRangeViewModel 
            //            { 
            //                Start = DateTime.Today.AddHours(8), 
            //                End = DateTime.Today.AddHours(12),
            //                Selected = false,
            //            },
            //            new CollectionDateTimeRangeViewModel 
            //            { 
            //                Start = DateTime.Today.AddHours(12), 
            //                End = DateTime.Today.AddHours(15),
            //                Selected = true,
            //            },
            //            new CollectionDateTimeRangeViewModel 
            //            { 
            //                Start = DateTime.Today.AddHours(15), 
            //                End = DateTime.Today.AddHours(18),
            //                Selected = false,
            //            } 
            //        }
            //    }
            //};
            //return Ok(data);
        }

    }
}

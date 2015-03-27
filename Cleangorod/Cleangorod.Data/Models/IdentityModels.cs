using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Globalization;

namespace Cleangorod.Data.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }

        public string Surname { get; set; }

        public string Note { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager, string authenticationType)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, authenticationType);
            // Add custom user claims here
            return userIdentity;
        }

        public ClientAddress ClientAddress { get; set; }
    }

    public class ClientAddress
    {
        public int Id { get; set; }

        public ApplicationUser User { get; set; }

        public string Address { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public virtual IDbSet<ClientSchedule> CollectionSchedules { get { return Set<ClientSchedule>(); } }

        public virtual IDbSet<CollectionDateTimeRange> CollectionDateTimeRanges { get { return Set<CollectionDateTimeRange>(); } }

        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
            Database.SetInitializer<ApplicationDbContext>(new CreateDatabaseIfNotExists<ApplicationDbContext>());
        }

        public IEnumerable<CollectionDateTimeRange> GetDateTimeRangesToEndOfWeek(DateTime value)
        {
            var startOfNextWeek = GetStartOfNextWeek(value);
            var hour = value.Hour;
            return CollectionDateTimeRanges
                .Where(x => (x.Date > value.Date) || (x.Date == value.Date && x.StartHour > x.EndHour))
                .Where(x => x.Date < startOfNextWeek)
                .ToList();
        }

        public ClientSchedule GetScheduleForWeek(string clientId, DateTime value)
        {
            var startOfThisWeek = GetStartOfThisWeek(value);
            return CollectionSchedules
                .Include(x => x.Ranges)
                .Where(x => x.Client.Id == clientId && x.WeekStart == startOfThisWeek)
                .SingleOrDefault();
        }

        public DateTime GetStartOfThisWeek(DateTime value)
        {
            return GetStartOfNextWeek(value).AddDays(-7);
        }

        public DateTime GetStartOfNextWeek(DateTime value)
        {
            var cultureInfo = CultureInfo.GetCultureInfo("ru-RU");
            var firstDayOfWeek = cultureInfo.DateTimeFormat.FirstDayOfWeek;

            int offset = 7 + firstDayOfWeek - value.DayOfWeek;
            if (offset == 8)
            {
                offset = 1;
            }
            return value.AddDays(offset).Date;
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ApplicationUser>()
                .HasOptional(p => p.ClientAddress)
                .WithOptionalDependent(x => x.User);
        }
    }
}
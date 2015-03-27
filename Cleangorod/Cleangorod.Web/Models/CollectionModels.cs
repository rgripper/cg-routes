using Cleangorod.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Cleangorod.Web.Models
{
    public class ClientScheduleViewModel
    {
        public int? Id { get; set; }

        public DateTime WeekStart { get; set; }

        public IEnumerable<CollectionDateTimeRangeViewModel> Ranges { get; set; }

        public string Note { get; set; }

        public ClientScheduleViewModel()
        {
            Ranges = new List<CollectionDateTimeRangeViewModel>();
        }
    }

    public class CollectionDateTimeRangeViewModel
    {
        public int Id { get; set; }

        public DateTime WeekStart { get; set; }

        public bool Active { get; set; }

        public DateTime Date { get; set; }

        public int StartHour { get; set; }

        public int EndHour { get; set; }

        public bool Selected { get; set; }
    }

}
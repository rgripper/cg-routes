using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cleangorod.Data.Models
{
    public class ClientSchedule
    {
        public int Id { get; set; }

        public ApplicationUser Client { get; set; }

        public virtual ICollection<CollectionDateTimeRange> Ranges { get; set; }

        public string Note { get; set; }

        public DateTime WeekStart { get; set; }

        public ClientSchedule()
        {
            Ranges = new List<CollectionDateTimeRange>();
        }
    }


    public class CollectionDateTimeRange
    {
        public int Id { get; set; }

        public DateTime WeekStart { get; set; }

        public DateTime Date { get; set; }

        public int StartHour { get; set; }

        public int EndHour { get; set; }

        public bool Active { get; set; }

        public virtual ICollection<ClientSchedule> ClientSchedules { get; set; }

        public CollectionDateTimeRange()
        {
            ClientSchedules = new List<ClientSchedule>();
        }
    }
}

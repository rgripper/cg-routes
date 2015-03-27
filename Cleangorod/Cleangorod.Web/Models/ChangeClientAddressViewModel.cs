using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Cleangorod.Web.Models
{
    public class ChangeClientAddressViewModel
    {
        public string Address { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }
    }
}
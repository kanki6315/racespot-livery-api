using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RaceSpotLiveryAPI.Models
{
    public class IracingDriverModel
    {
        public IracingDriverModel()
        {
        }

        public string DriverId { get; set; }
        public string DriverName { get; set; }
        public string DriverIrating { get; set; }

        public string LicenseLevel { get; set; }
        public string SafetyRating { get; set; }

    }
}

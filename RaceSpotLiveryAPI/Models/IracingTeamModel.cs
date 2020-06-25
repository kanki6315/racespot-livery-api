using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RaceSpotLiveryAPI.Models
{
    public class IracingTeamModel
    {
        public List<IracingDriverModel> Drivers { get; set; }

        public string TeamName { get; set; }
        public string TeamOwner { get; set; }
        public string TeamOwnerId { get; set; }
        public string TeamId { get; set; }
        public string NumDrivers { get; set; }

        public IracingTeamModel()
        {
        }
    }

}

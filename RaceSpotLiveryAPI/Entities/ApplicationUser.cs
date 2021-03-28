using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RaceSpotLiveryAPI.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsLeagueAdmin { get; set; }
        public string IracingId { get; set; }
        public bool IsAgreedToEmails { get; set; }
        public DateTime LastUpdated { get; set; }

        public virtual UserInvite Invite { get; set; }
        public ICollection<Livery> Liveries { get; set; }
        public ICollection<SeriesAdmin> Series { get; set; }
    }
}

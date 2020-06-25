using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RaceSpotLiveryAPI.Entities
{
    public class UserInvite
    {
        public UserInvite()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public InviteStatus Status { get; set; }
        public DateTime LastUpdated { get; set; }
        public string IracingId { get; set; }

        public virtual ApplicationUser User { get; set; }
        public string UserId { get; set; }
    }

    public enum InviteStatus
    {
        SENT,
        DONE
    }
}

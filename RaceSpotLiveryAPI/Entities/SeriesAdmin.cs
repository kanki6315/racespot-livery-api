using System;

namespace RaceSpotLiveryAPI.Entities
{
    public class SeriesAdmin
    {
        public Guid SeriesId { get; set; }
        public virtual Series Series { get; set; }

        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}
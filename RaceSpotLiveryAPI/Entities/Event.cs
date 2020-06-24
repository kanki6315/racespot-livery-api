using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RaceSpotLiveryAPI.Entities
{
    public class Event
    {
        public Event()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public virtual Series Series { get; set; }
        public Guid SeriesId { get; set; }
        public DateTime RaceTime { get; set; }
        public string BroadcastLink { get; set; }
        public EventState EventState { get; set; }
    }

    public enum EventState
    {
        UPCOMING,
        DONE
    }
}

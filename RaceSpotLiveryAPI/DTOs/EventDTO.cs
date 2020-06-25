using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RaceSpotLiveryAPI.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RaceSpotLiveryAPI.DTOs
{
    public class EventDTO
    {
        public EventDTO()
        {

        }

        public EventDTO(Event eventObj)
        {
            this.Id = eventObj.Id;
            this.SeriesId = eventObj.SeriesId;
            this.RaceTime = eventObj.RaceTime;
            this.BroadcastLink = eventObj.BroadcastLink;
            this.EventState = eventObj.EventState;
            this.Order = eventObj.Order;
        }

        public Guid Id { get; set; }
        public Guid SeriesId { get; set; }
        public DateTime RaceTime { get; set; }
        public string BroadcastLink { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public EventState EventState { get; set; }
        public int Order { get; set; }
    }
}

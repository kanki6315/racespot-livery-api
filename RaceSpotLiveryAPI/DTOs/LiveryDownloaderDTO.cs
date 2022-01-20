using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RaceSpotLiveryAPI.Entities;

namespace RaceSpotLiveryAPI.DTOs
{
    public class LiveryDownloaderDTO
    {
        
        public LiveryDownloaderDTO(Livery livery)
        {
            this.SeriesId = livery.SeriesId;
            this.LiveryType = livery.LiveryType;
            this.ITeamId = livery.ITeamId;
            this.ITeamName = livery.ITeamName;
            this.IsCustomNumber = livery.IsCustomNumber;
            this.IsRejected = livery.IsRejected;
            
            if (livery.Car != null)
            {
                CarPath = livery.Car.Path;
            }

            if (livery.User != null)
            {
                IracingId = livery.User.IracingId;
            }

            if (livery.IsRejected && livery.Rejections != null)
            {
                this.RejectionStatus = livery.Rejections.First(l => l.Status != RejectionStatus.Resolved).Status;
            }
        }
        
        [JsonConverter(typeof(StringEnumConverter))]
        public LiveryType LiveryType { get; set; }
        public Guid SeriesId { get; set; }
        public string ITeamId { get; set; }
        public string ITeamName { get; set; }
        public string CarPath { get; set; }
        public bool IsCustomNumber { get; set; }
        public bool IsRejected { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public RejectionStatus RejectionStatus { get; set; }
        public string IracingId { get; set; }
    }
}
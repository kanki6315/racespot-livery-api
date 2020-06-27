using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RaceSpotLiveryAPI.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RaceSpotLiveryAPI.DTOs
{
    public class LiveryDTO
    {
        public LiveryDTO()
        {

        }

        public LiveryDTO(Livery livery)
        {
            this.Id = livery.Id;
            this.SeriesId = livery.SeriesId;
            this.LiveryType = livery.LiveryType;
            this.ITeamId = livery.ITeamId;
            this.ITeamName = livery.ITeamName;
            if(livery.Car != null)
            {
                CarName = livery.Car.Name;
            }
        }

        public LiveryDTO(Livery livery, string PreviewUrl) : this(livery)
        {
            this.PreviewUrl = PreviewUrl;
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public LiveryType LiveryType { get; set; }
        public Guid SeriesId { get; set; }
        public string ITeamId { get; set; }
        public string ITeamName { get; set; }
        public Guid Id { get; set; }

        public string UploadUrl { get; set; }
        public string PreviewUrl { get; set; }
        public string CarName { get; set; }
        public Guid CarId { get; set; }
    }
}

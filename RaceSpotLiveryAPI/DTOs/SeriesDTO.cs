using RaceSpotLiveryAPI.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RaceSpotLiveryAPI.DTOs
{
    public class SeriesDTO
    {
        public SeriesDTO()
        {
        }

        public SeriesDTO(Series series)
        {
            this.Id = series.Id;
            this.Name = series.Name;
            this.IsTeam = series.IsTeam;
            this.IsArchived = series.IsArchived;
            this.LastUpdated = series.LastUpdated;
            this.LogoImgUrl = series.LogoImgUrl;
            this.Description = series.Description;

            if(series.SeriesCars != null)
            {
                Cars = series.SeriesCars.Select(s => new CarDTO(s.Car)).ToList();
            }
            if(series.Events != null)
            {
                Events = series.Events.Select(e => new EventDTO(e)).ToList();
            }
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool IsTeam { get; set; }
        public bool IsArchived { get; set; }
        public DateTime LastUpdated { get; set; }
        public string LogoImgUrl { get; set; }
        public string Description { get; set; }

        public List<Guid> CarIds { get; set; }
        public List<CarDTO> Cars { get; set; }

        public List<EventDTO> Events { get; set; }
    }
}

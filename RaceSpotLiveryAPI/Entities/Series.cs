using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RaceSpotLiveryAPI.Entities
{
    public class Series
    {
        public Series()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool IsTeam { get; set; }
        public bool IsArchived { get; set; }
        public DateTime LastUpdated { get; set; }
        public string LogoImgUrl { get; set; }
        public string Description { get; set; }


        public ICollection<Event> Events { get; set; }
        public ICollection<Livery> Liveries { get; set; }
        public ICollection<SeriesCar> SeriesCars { get; set; }
    }
}

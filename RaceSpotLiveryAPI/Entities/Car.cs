using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RaceSpotLiveryAPI.Entities
{
    public class Car
    {
        public Car()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string LogoImgUrl { get; set; }

        public ICollection<SeriesCar> SeriesCars { get; set; }
        public ICollection<Livery> Liveries { get; set; }
    }
}

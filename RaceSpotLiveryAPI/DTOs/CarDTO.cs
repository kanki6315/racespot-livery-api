using RaceSpotLiveryAPI.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RaceSpotLiveryAPI.DTOs
{
    public class CarDTO
    {
        public CarDTO()
        {

        }

        public CarDTO(Car car)
        {
            this.Id = car.Id;
            this.Name = car.Name;
            this.Path = car.Path;
            this.LogoImgUrl = car.LogoImgUrl;
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string LogoImgUrl { get; set; }
    }
}

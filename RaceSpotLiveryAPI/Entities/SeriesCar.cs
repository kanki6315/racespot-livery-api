using RaceSpotLiveryAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RaceSpotLiveryAPI.Entities
{
    public class SeriesCar
    {
        public Guid SeriesId { get; set; }
        public virtual Series Series { get; set; }

        public Guid CarId { get; set; }
        public virtual Car Car { get; set; }
    }
}

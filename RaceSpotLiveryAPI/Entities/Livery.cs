using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace RaceSpotLiveryAPI.Entities
{
    public class Livery
    {
        public Livery()
        {
            Id = Guid.NewGuid();
        }

        public LiveryType LiveryType { get; set; }
        public virtual Series Series { get; set; }
        public Guid SeriesId { get; set; }
        public string ITeamId { get; set; }

        public Guid Id { get; set; }
    }

    public enum LiveryType
    {
        Car,
        Suit,
        Helmet,
        [EnumMember(Value = "Spec Map")]
        SpecMap
    }

}

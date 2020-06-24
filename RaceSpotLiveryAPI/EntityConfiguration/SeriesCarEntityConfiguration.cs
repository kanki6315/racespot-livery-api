using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaceSpotLiveryAPI.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RaceSpotLiveryAPI.EntityConfiguration
{
    public class SeriesCarEntityConfiguration : IEntityTypeConfiguration<SeriesCar>
    {
        public void Configure(EntityTypeBuilder<SeriesCar> builder)
        {
            builder.HasKey(p => new { p.SeriesId, p.CarId });

            builder.HasOne<Car>(p => p.Car)
                .WithMany(c => c.SeriesCars)
                .HasForeignKey(p => p.CarId);

            builder.HasOne<Series>(p => p.Series)
                .WithMany(c => c.SeriesCars)
                .HasForeignKey(p => p.SeriesId);
        }
    }
}

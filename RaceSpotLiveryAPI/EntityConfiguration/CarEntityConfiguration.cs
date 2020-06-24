using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaceSpotLiveryAPI.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RaceSpotLiveryAPI.EntityConfiguration
{
    public class CarEntityConfiguration : IEntityTypeConfiguration<Car>
    {
        public void Configure(EntityTypeBuilder<Car> builder)
        {
            builder.Property(p => p.Id).IsRequired().ValueGeneratedNever();
            builder.Property(p => p.Name).IsRequired().ValueGeneratedNever();
            builder.Property(p => p.Path).IsRequired().ValueGeneratedNever();
            builder.Property(p => p.LogoImgUrl).IsRequired(false).ValueGeneratedNever();

            builder.HasKey(p => p.Id);
        }
    }
}

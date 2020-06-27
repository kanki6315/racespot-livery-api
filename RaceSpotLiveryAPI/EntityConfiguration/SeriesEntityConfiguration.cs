using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaceSpotLiveryAPI.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RaceSpotLiveryAPI.EntityConfiguration
{
    public class SeriesEntityConfiguration : IEntityTypeConfiguration<Series>
    {
        public void Configure(EntityTypeBuilder<Series> builder)
        {
            builder.Property(p => p.Id).IsRequired().ValueGeneratedNever();
            builder.Property(p => p.Name).IsRequired().ValueGeneratedNever();
            builder.Property(p => p.IsTeam).IsRequired().ValueGeneratedNever();
            builder.Property(p => p.IsArchived).IsRequired().ValueGeneratedNever();
            builder.Property(p => p.LastUpdated).IsRequired().ValueGeneratedNever();
            builder.Property(p => p.LogoImgUrl).IsRequired().ValueGeneratedNever();
            builder.Property(p => p.Description).IsRequired(false).ValueGeneratedNever();

            builder.HasMany(p => p.Events)
                .WithOne(e => e.Series)
                .IsRequired();

            builder.HasMany(p => p.Liveries)
                .WithOne(l => l.Series)
                .IsRequired();

            builder.HasKey(p => p.Id);
        }
    }

}

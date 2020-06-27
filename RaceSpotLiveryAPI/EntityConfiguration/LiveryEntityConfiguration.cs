using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaceSpotLiveryAPI.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RaceSpotLiveryAPI.EntityConfiguration
{
    public class LiveryEntityConfiguration : IEntityTypeConfiguration<Livery>
    {
        public void Configure(EntityTypeBuilder<Livery> builder)
        {
            builder.Property(p => p.Id).IsRequired().ValueGeneratedNever();
            builder.Property(p => p.LiveryType).IsRequired().ValueGeneratedNever();
            builder.Property(p => p.ITeamId).IsRequired(false).ValueGeneratedNever();
            builder.Property(p => p.ITeamName).IsRequired(false).ValueGeneratedNever();

            builder.HasOne(p => p.User)
                .WithMany(u => u.Liveries)
                .IsRequired();
            
            builder.HasKey(p => p.Id);
        }
    }
}

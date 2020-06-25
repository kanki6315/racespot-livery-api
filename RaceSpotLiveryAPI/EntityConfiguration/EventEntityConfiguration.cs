using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaceSpotLiveryAPI.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RaceSpotLiveryAPI.EntityConfiguration
{
    public class EventEntityConfiguration : IEntityTypeConfiguration<Event>
    {
        public void Configure(EntityTypeBuilder<Event> builder)
        {
            builder.Property(p => p.Id).IsRequired().ValueGeneratedNever();
            builder.Property(p => p.RaceTime).IsRequired().ValueGeneratedNever();
            builder.Property(p => p.BroadcastLink).IsRequired(false).ValueGeneratedNever();
            builder.Property(p => p.EventState).IsRequired().ValueGeneratedNever();
            builder.Property(p => p.Order).IsRequired().ValueGeneratedNever();

            builder.HasKey(p => p.Id);
        }
    }
}

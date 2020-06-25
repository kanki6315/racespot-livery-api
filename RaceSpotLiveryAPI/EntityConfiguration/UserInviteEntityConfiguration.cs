using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaceSpotLiveryAPI.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RaceSpotLiveryAPI.EntityConfiguration
{
    public class UserInviteEntityConfiguration : IEntityTypeConfiguration<UserInvite>
    {
        public void Configure(EntityTypeBuilder<UserInvite> builder)
        {
            builder.Property(p => p.Id).IsRequired().ValueGeneratedNever();
            builder.Property(p => p.Status).IsRequired().ValueGeneratedNever();
            builder.Property(p => p.LastUpdated).IsRequired().ValueGeneratedNever();
            builder.Property(p => p.IracingId).IsRequired().ValueGeneratedNever();

            builder.HasOne(p => p.User)
                .WithOne(e => e.Invite)
                .HasForeignKey<UserInvite>(i => i.UserId);

            builder.HasKey(p => p.Id);
        }
    }
}

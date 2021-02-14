using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaceSpotLiveryAPI.Entities;

namespace RaceSpotLiveryAPI.EntityConfiguration
{
    public class RejectionNoticeEntityConfiguration : IEntityTypeConfiguration<RejectionNotice>
    {
        public void Configure(EntityTypeBuilder<RejectionNotice> builder)
        {
            builder.Property(p => p.Id).IsRequired().ValueGeneratedNever();
            builder.Property(p => p.Message).IsRequired().ValueGeneratedNever();
            builder.Property(p => p.Status).IsRequired().ValueGeneratedNever();

            builder.HasOne(p => p.Livery)
                .WithMany(u => u.Rejections)
                .IsRequired();
            
            builder.HasKey(p => p.Id);
        }
    }
}
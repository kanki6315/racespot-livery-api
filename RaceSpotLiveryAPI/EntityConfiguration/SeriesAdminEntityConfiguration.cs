using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaceSpotLiveryAPI.Entities;

namespace RaceSpotLiveryAPI.EntityConfiguration
{
    public class SeriesAdminEntityConfiguration: IEntityTypeConfiguration<SeriesAdmin>
    {
        public void Configure(EntityTypeBuilder<SeriesAdmin> builder)
        {
            builder.HasKey(p => new { p.SeriesId, p.UserId });

            builder.HasOne<ApplicationUser>(p => p.User)
                .WithMany(c => c.Series)
                .HasForeignKey(p => p.UserId);

            builder.HasOne<Series>(p => p.Series)
                .WithMany(c => c.SeriesAdmins)
                .HasForeignKey(p => p.SeriesId);
        }
    }
}
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RaceSpotLiveryAPI.Entities;
using RaceSpotLiveryAPI.EntityConfiguration;
using RaceSpotLiveryAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RaceSpotLiveryAPI.Contexts
{
    public class RaceSpotDBContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Series> Series { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Livery> Liveries { get; set; }
        public DbSet<Car> Cars { get; set; }
        public DbSet<SeriesCar> SeriesCars { get; set; }
        public DbSet<UserInvite> UserInvites { get; set; }

        public RaceSpotDBContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfiguration(new SeriesEntityConfiguration());
            builder.ApplyConfiguration(new CarEntityConfiguration());
            builder.ApplyConfiguration(new EventEntityConfiguration());
            builder.ApplyConfiguration(new LiveryEntityConfiguration());
            builder.ApplyConfiguration(new SeriesCarEntityConfiguration());
            builder.ApplyConfiguration(new UserInviteEntityConfiguration());

            base.OnModelCreating(builder);
        }
    }
}

using EDR.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace EDR.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {

        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<IdentityUser>().ToTable("Users");
            modelBuilder.Entity<ApplicationUser>().ToTable("Users");
            modelBuilder.Entity<IdentityRole>().ToTable("Roles");
            modelBuilder.Entity<IdentityUserClaim>().ToTable("UserClaims");
            modelBuilder.Entity<IdentityUserLogin>().ToTable("UserLogins");
            modelBuilder.Entity<IdentityUserRole>().ToTable("UserRoles");
        }

        public DbSet<Class> Classes { get; set; }
        public DbSet<DanceStyle> DanceStyles { get; set; }
        public DbSet<Theater> Theaters { get; set; }
        public DbSet<Hotel> Hotels { get; set; }
        public DbSet<Restaurant> Restaurants { get; set; }
        public DbSet<Nightclub> Nightclubs { get; set; }
        public DbSet<ConferenceCenter> ConferenceCenters { get; set; }
        public DbSet<Studio> Studios { get; set; }
        public DbSet<Conference> Conferences { get; set; }
        public DbSet<Rehearsal> Rehearsals { get; set; }
        public DbSet<Workshop> Workshops { get; set; }
        public DbSet<OpenHouse> OpenHouses { get; set; }
        public DbSet<Concert> Concerts { get; set; }
        public DbSet<Social> Socials { get; set; }
        public DbSet<Party> Parties { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<School> Schools { get; set; }
    }
}
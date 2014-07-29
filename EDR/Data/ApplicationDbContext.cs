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

            // Identity
            modelBuilder.Entity<IdentityUser>().ToTable("Users");
            modelBuilder.Entity<ApplicationUser>().ToTable("Users");
            modelBuilder.Entity<IdentityRole>().ToTable("Roles");
            modelBuilder.Entity<IdentityUserClaim>().ToTable("UserClaims");
            modelBuilder.Entity<IdentityUserLogin>().ToTable("UserLogins");
            modelBuilder.Entity<IdentityUserRole>().ToTable("UserRoles");

            // Places
            modelBuilder.Entity<Theater>().ToTable("Theaters");
            modelBuilder.Entity<Hotel>().ToTable("Hotels");
            modelBuilder.Entity<Restaurant>().ToTable("Restaurants");
            modelBuilder.Entity<Nightclub>().ToTable("Nightclubs");
            modelBuilder.Entity<ConferenceCenter>().ToTable("ConferenceCenters");
            modelBuilder.Entity<Studio>().ToTable("Studios");

            // Events
            modelBuilder.Entity<Class>().ToTable("Classes");
            modelBuilder.Entity<Conference>().ToTable("Conferences");
            modelBuilder.Entity<Rehearsal>().ToTable("Rehearsals");
            modelBuilder.Entity<Workshop>().ToTable("Workshops");
            modelBuilder.Entity<OpenHouse>().ToTable("OpenHouses");
            modelBuilder.Entity<Concert>().ToTable("Concerts");
            modelBuilder.Entity<Social>().ToTable("Socials");
            modelBuilder.Entity<Party>().ToTable("Parties");

            // Groups
            modelBuilder.Entity<Team>().ToTable("Teams");
            modelBuilder.Entity<School>().ToTable("Schools");
        }

        public DbSet<Place> Places { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<DanceStyle> DanceStyles { get; set; }
        public DbSet<Class> Classes { get; set; }
    }
}
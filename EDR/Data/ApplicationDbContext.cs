using EDR.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Reflection;
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

            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();

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
            modelBuilder.Entity<OtherPlace>().ToTable("OtherPlaces");

            // Event Series
            modelBuilder.Entity<ClassSeries>().ToTable("ClassSeries");

            // Events
            modelBuilder.Entity<Class>().ToTable("Classes");
            modelBuilder.Entity<Conference>().ToTable("Conferences");
            modelBuilder.Entity<Rehearsal>().ToTable("Rehearsals");
            modelBuilder.Entity<Workshop>().ToTable("Workshops");
            modelBuilder.Entity<OpenHouse>().ToTable("OpenHouses");
            modelBuilder.Entity<Concert>().ToTable("Concerts");
            modelBuilder.Entity<Social>().ToTable("Socials");
            modelBuilder.Entity<Party>().ToTable("Parties");
            modelBuilder.Entity<LinkedFacebookObject>().ToTable("LinkedFacebookObjects");
            modelBuilder.Entity<EventAttendee>().ToTable("EventAttendees");
            modelBuilder.Entity<Feed>()
                        .ToTable("EventFeeds")
                        .HasRequired(f => f.Event)
                        .WithMany(f => f.Feeds)
                        .WillCascadeOnDelete(true);
            modelBuilder.Entity<EventRegistration>().ToTable("EventRegistrations");
            //modelBuilder.Entity<ObjectFeed>()
            //            .ToTable("ObjectFeeds")
            //            .HasRequired(f => f.Object)
            //            .WithMany(f => f.Feeds)
            //            .WillCascadeOnDelete(true);

            //// Groups
            //modelBuilder.Entity<Team>().ToTable("Teams");
            //modelBuilder.Entity<School>().ToTable("Schools");
            modelBuilder.Entity<EventInstance>().ToTable("EventInstances");

            // Reviews
            modelBuilder.Entity<Review>().ToTable("Reviews");

            //Photos
            modelBuilder.Entity<Picture>().ToTable("Pictures");
            modelBuilder.Entity<UserPicture>().ToTable("UserPictures");
            modelBuilder.Entity<EventPicture>().ToTable("EventPictures");
            modelBuilder.Entity<Album>().ToTable("Albums");
            modelBuilder.Entity<EventAlbum>().ToTable("EventAlbums");

            //Videos
            modelBuilder.Entity<Video>().ToTable("Videos");
            modelBuilder.Entity<EventVideo>().ToTable("EventVideos");
            modelBuilder.Entity<EventPlaylist>().ToTable("EventPlaylists");
            modelBuilder.Entity<DanceStyleVideo>().ToTable("DanceStyleVideos");

            //Students
            modelBuilder.Entity<Student>().ToTable("Students");

            //EventMembers
            modelBuilder.Entity<EventMember>().ToTable("EventMembers");

            //Invitations
            modelBuilder.Entity<ClassTeacherInvitation>().ToTable("ClassTeacherInvitations");
            modelBuilder.Entity<SocialPromoterInvitation>().ToTable("SocialPromoterInvitations");
            modelBuilder.Entity<PlaceOwnerInvitation>().ToTable("PlaceOwnerInvitations");

            //Store
            modelBuilder.Entity<DancePack>().ToTable("DancePacks");
            modelBuilder.Entity<Cart>().ToTable("Carts")
                        .HasRequired(f => f.DancePack)
                        .WithMany(f => f.Carts)
                        .WillCascadeOnDelete(true);
            modelBuilder.Entity<Order>().ToTable("Orders");
            modelBuilder.Entity<OrderDetail>().ToTable("OrderDetails");
            modelBuilder.Entity<OrderDetail>()
                        .HasRequired(f => f.Order)
                        .WithMany(f => f.OrderDetails)
                        .WillCascadeOnDelete(true);
            modelBuilder.Entity<OrderDetail>()
                        .HasRequired(f => f.DancePack)
                        .WithMany(f => f.OrderDetails)
                        .WillCascadeOnDelete(true);
            modelBuilder.Entity<Ticket>().ToTable("Tickets");
            //modelBuilder.Entity<EventTicket>().ToTable("EventTickets");
            modelBuilder.Entity<UserTicket>().ToTable("UserTickets");

            //Organizations
            modelBuilder.Entity<Organization>().ToTable("Organizations");
            modelBuilder.Entity<OrganizationMember>().ToTable("OrganizationMembers");
        }

        public DbSet<Place> Places { get; set; }

        //  Events
        public DbSet<Event> Events { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<Social> Socials { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        //public DbSet<EventTicket> EventTickets { get; set; }
        public DbSet<UserTicket> UserTickets { get; set; }
        public DbSet<EventInstance> EventInstances { get; set; }
        public DbSet<EventRegistration> EventRegistrations { get; set; }

        public DbSet<Series> Series { get; set; }
        //  public DbSet<Group> Groups { get; set; }
        public DbSet<DanceStyle> DanceStyles { get; set; }
        public DbSet<Review> Reviews { get; set; }

        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Promoter> Promoters { get; set; }
        public DbSet<Owner> Owners { get; set; }
        public DbSet<IdentityUser> IdentityUsers { get; set; }

        public DbSet<Picture> Pictures { get; set; }
        public DbSet<Album> Albums { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Video> Videos { get; set; }
        public DbSet<Playlist> Playlists { get; set; }
        public DbSet<EventMember> EventMembers { get; set; }
        public DbSet<EventAttendee> EventAttendees { get; set; }
        public DbSet<Feed> EventFeeds { get; set; }
        //public DbSet<ObjectFeed> ObjectFeeds { get; set; }

        public DbSet<ClassTeacherInvitation> ClassTeacherInvitations { get; set; }
        public DbSet<SocialPromoterInvitation> SocialPromoterInvitations { get; set; }
        public DbSet<PlaceOwnerInvitation> PlaceOwnerInvitations { get; set; }
        public DbSet<LinkedFacebookObject> LinkedFacebookObjects { get; set; }

        //Store
        public DbSet<Cart> Carts { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<DancePack> DancePacks { get; set; }

        //Organization
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<OrganizationMember> OrganizationMembers { get; set; }
        public DbSet<School> Schools { get; set; }

        //// TODO: REMOVE THESE DBSETS
        //public DbSet<Class> Classes { get; set; }
        //public DbSet<ClassSeries> ClassSeries { get; set; }
    }
}
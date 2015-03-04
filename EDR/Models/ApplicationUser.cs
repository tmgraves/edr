using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System;

namespace EDR.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);

            // Add custom user claims here
            return userIdentity;
        }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ZipCode { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int Experience
        {
            get
            {
                return StartDate != null ? (int)((DateTime.Today - (DateTime)StartDate).Days / 365) : 0;
            }
        }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Started Dancing")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? StartDate { get; set; }

        public string FacebookUsername { get; set; }
        public string FacebookToken { get; set; }
        [Display(Name="YouTube Username")]
        public string YouTubeUsername { get; set; }
        public int? InstagramId { get; set; }
        [Display(Name = "Instagram Username")]
        public string InstagramUsername { get; set; }
        public string InstagramToken { get; set; }

        //  Spotify Variables
        [Display(Name = "Spotify Uri")]
        public string SpotifyUri { get; set; }
        public string SpotifyId { get; set; }
        [Display(Name = "Spotify Username")]
        public string SpotifyUsername { get; set; }
        public string SpotifyToken { get; set; }
        public string SpotifyRefreshToken { get; set; }

        [Display(Name = "Full Name")]
        public string FullName
        { 
            get
            {
                return FirstName + " " + LastName;
            }
        }

        public virtual ICollection<Event> Events { get; set; }
        public virtual ICollection<Teacher> Teachers { get; set; }
        public virtual ICollection<Owner> Owners { get; set; }
        public virtual ICollection<Promoter> Promoters { get; set; }
        public virtual ICollection<DanceStyle> DanceStyles { get; set; }
        public virtual ICollection<Party> Parties { get; set; }
        public virtual ICollection<UserPicture> UserPictures { get; set; }
        public virtual ICollection<Student> Students { get; set; }
        public ICollection<EventMember> EventMembers { get; set; }
    }
}
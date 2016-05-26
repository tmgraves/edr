using EDR.Enums;
using Foolproof;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EDR.Models
{
    public abstract class Organization : Entity
    {
        [Required(ErrorMessage ="Please enter a Name")]
        public string Name { get; set; }
        public string Description { get; set; }
        public string FacebookLink { get; set; }
        public string PhotoUrl { get; set; }
        public bool Public { get; set; }
        [DataType(DataType.Date)]
        [Display(Name = "Date Started")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime DateStarted { get; set; }
        public string Address { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Country { get; set; }
        [ScaffoldColumn(false)]
        public double Latitude { get; set; }
        [ScaffoldColumn(false)]
        public double Longitude { get; set; }
        public ICollection<OrganizationMember> Members { get; set; }
        public ICollection<DanceStyle> DanceStyles { get; set; }
        public ICollection<Review> Reviews { get; set; }
        public virtual ICollection<Feed> Feeds { get; set; }
    }

    public class School : Organization
    {
        public ICollection<Teacher> Teachers { get; set; }
        public ICollection<Studio> Studios { get; set; }
        public ICollection<Class> Classes { get; set; }
        public ICollection<Ticket> Tickets { get; set; }
        public ICollection<Owner> Owners { get; set; }
        public ICollection<Team> Teams { get; set; }

        public School()
        {
            Teachers = new List<Teacher>();
            Studios = new List<Studio>();
            Classes = new List<Class>();
            Tickets = new List<Ticket>();
            Owners = new List<Owner>();
            Members = new List<OrganizationMember>();
        }
    }

    public class Team : Organization
    {
        [Range(1, 100)]
        public int SkillLevel { get; set; }
        public int? SchoolId { get; set; }
        [ForeignKey("SchoolId")]
        public School School { get; set; }
        public ICollection<Teacher> Teachers { get; set; }
        public ICollection<Rehearsal> Rehearsals { get; set; }
        public ICollection<Performance> Performances { get; set; }
        public ICollection<Audition> Auditions { get; set; }
    }

    public class OrganizationMember : Entity
    {
        [Required]
        public int OrganizationId { get; set; }
        [ForeignKey("OrganizationId")]
        public Organization Organization { get; set; }
        [Required]
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
        public bool Admin { get; set; }
    }

    public class Audition : Entity
    {
        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Audition Date")]
        [DisplayFormat(DataFormatString = "{0:MM-dd-yyyy}")]
        public DateTime StartDate { get; set; }
        [Required]
        [DataType(DataType.Time)]
        [Display(Name = "Start Time")]
        [DisplayFormat(DataFormatString = "{0:hh:mm tt}", ApplyFormatInEditMode = false)]
        public DateTime StartTime { get; set; }
        [Required]
        [DataType(DataType.Time)]
        [Display(Name = "End Time")]
        [DisplayFormat(DataFormatString = "{0:hh:mm tt}", ApplyFormatInEditMode = false)]
        [GreaterThan("StartTime")]
        public DateTime EndTime { get; set; }
        [Required]
        public int TeamId { get; set; }
        [ForeignKey("TeamId")]
        public Team Team { get; set; }
        [Required]
        public int PlaceId { get; set; }
        [ForeignKey("PlaceId")]
        public Place Place { get; set; }
    }

    public class Performance : Entity
    {
        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Performance Date")]
        [DisplayFormat(DataFormatString = "{0:MM-dd-yyyy}")]
        public DateTime StartDate { get; set; }
        [Required]
        [DataType(DataType.Time)]
        [Display(Name = "Start Time")]
        [DisplayFormat(DataFormatString = "{0:hh:mm tt}", ApplyFormatInEditMode = false)]
        public DateTime StartTime { get; set; }
        [Required]
        [Display(Name = "Place")]
        public int PlaceId { get; set; }
        [ForeignKey("PlaceId")]
        public Place Place { get; set; }
        public int? TeamId { get; set; }
        [ForeignKey("TeamId")]
        public Team Team { get; set; }
        [Display(Name = "Event")]
        public int? EventId { get; set; }
        [ForeignKey("EventId")]
        public Event Event { get; set; }
    }
}
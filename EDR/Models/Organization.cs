using EDR.Enums;
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
        public string Name { get; set; }
        public string Description { get; set; }
        public string FacebookLink { get; set; }
        public bool Public { get; set; }
        [DataType(DataType.Date)]
        [Display(Name = "Date Started")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime DateStarted { get; set; }
        public string Address { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        [Range(1, 50000, ErrorMessage = "Select a State")]
        public State State { get; set; }
        public string Zip { get; set; }
        public string Country { get; set; }
        [ScaffoldColumn(false)]
        public double Latitude { get; set; }
        [ScaffoldColumn(false)]
        public double Longitude { get; set; }
        public ICollection<OrganizationMember> Members { get; set; }
    }

    public class School : Organization
    {
        public ICollection<Teacher> Teachers { get; set; }
        public ICollection<Studio> Studios { get; set; }
        public ICollection<Class> Classes { get; set; }
        public ICollection<Ticket> Tickets { get; set; }
        public ICollection<Owner> Owners { get; set; }

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
        public int SkillLevel { get; set; }
        public int? SchoolId { get; set; }
        [ForeignKey("SchoolId")]
        public School School { get; set; }
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
}
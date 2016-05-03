using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Foolproof;

namespace EDR.Models.ViewModels
{
    public class LearnViewModel
    {
        //  Search Fields
        [Display(Name = "Teacher:")]
        public string TeacherId { get; set; }
        public string Teacher { get; set; }
        [Display(Name = "Dance Style:")]
        public int? DanceStyleId { get; set; }
        public string Style { get; set; }
        public Address SearchAddress { get; set; }
        public string Location { get; set; }
        [Display(Name = "Skill Level:")]
        public int? SkillLevel { get; set; }
        public List<DayOfWeek> Days { get; set; }
        public List<DayOfWeek> DaysOfWeek { get; set; }

        //  Map Settings
        public double? NELat { get; set; }
        public double? NELng { get; set; }
        public double? SWLat { get; set; }
        public double? SWLng { get; set; }
        public double? CenterLat { get; set; }
        public double? CenterLng { get; set; }
        public int? Zoom { get; set; }

        //  Results
        public IEnumerable<Class> Classes { get; set; }
    }

    public class SocialViewModel
    {
        [Display(Name = "Dance Style:")]
        public IEnumerable<DanceStyle> DanceStyles { get; set; }
        public int? DanceStyleId { get; set; }
        [Display(Name = "Place:")]
        public IEnumerable<Place> Places { get; set; }
        public int? PlaceId { get; set; }
        [Display(Name = "Your Location:", Prompt = "Enter your location here")]
        public string Location { get; set; }
        public Address SearchAddress { get; set; }
        public int Zoom { get; set; }
        public List<DayOfWeek> Days { get; set; }
        public List<DayOfWeek> DaysOfWeek { get; set; }
        public IEnumerable<Social> Socials { get; set; }
        public ApplicationUser User { get; set; }
    }

    public class Test2
    {
        [Required]
        public string Title { get; set; }
        [RequiredIf("Title", Operator.GreaterThan, "")]
        public string Desc { get; set; }
        public List<OrganizationMember> Members { get; set; }
    }
}
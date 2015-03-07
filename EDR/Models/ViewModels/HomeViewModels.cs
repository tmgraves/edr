using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EDR.Models.ViewModels
{
    public class LearnViewModel
    {
        [Display(Name = "Dance Style:")]
        public IEnumerable<DanceStyle> DanceStyles { get; set; }
        public int? DanceStyleId { get; set; }
        [Display(Name = "Place:")]
        public IEnumerable<Place> Places { get; set; }
        public int? PlaceId { get; set; }
        [Display(Name = "Teacher:")]
        public IEnumerable<Teacher> Teachers { get; set; }
        public int? TeacherId { get; set; }
        [Display(Name = "Skill Level:")]
        public int? SkillLevel { get; set; }
        [Display(Name = "Your Location:", Prompt = "Enter your location here")]
        public string Location { get; set; }
        public Address SearchAddress { get; set; }
        public int Zoom { get; set; }
        public List<DayOfWeek> Days { get; set; }
        public List<DayOfWeek> DaysOfWeek { get; set; }
        public IEnumerable<ClassSeries> ClassSeries { get; set; }
        public IEnumerable<Class> Classes { get; set; }
        public IEnumerable<Workshop> Workshops { get; set; }
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
    }
}
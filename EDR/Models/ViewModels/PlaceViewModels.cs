using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EDR.Enums;
using System.ComponentModel.DataAnnotations;

namespace EDR.Models.ViewModels
{
    public class PlaceViewModel
    {
        public Place Place { get; set; }
        public IEnumerable<Class> Classes { get; set; }
        public IEnumerable<Workshop> Workshops { get; set; }
        public IEnumerable<Social> Socials { get; set; }
        public IEnumerable<Concert> Concerts { get; set; }
        public IEnumerable<Conference> Conferences { get; set; }
        public IEnumerable<OpenHouse> OpenHouses { get; set; }
        public IEnumerable<Party> Parties { get; set; }
        public IEnumerable<Owner> Owners { get; set; }
        public string TeacherId { get; set; }
        public List<SelectListItem> TeacherList { get; set; }
        public int? DanceStyleId { get; set; }
        public List<SelectListItem> DanceStyleList { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Select a skill level")]
        public SkillLevel SkillLevel { get; set; }
    }

    public class PlaceEventSearchViewModel
    {
        public IEnumerable<Event> Events { get; set; }
        public List<Media> MediaUpdates { get; set; }
    }
}
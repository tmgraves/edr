using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EDR.Models.ViewModels
{
    public class EventCreateViewModel
    {
        public Event Event { get; set; }
        public string EventType { get; set; }
        public string Role { get; set; }
        [Display(Name = "Your Skill Level (1-5)")]
        public int SkillLevel { get; set; }
        [Display(Name = "Prerequisite(s)")]
        public string Prerequisite { get; set; }
        public ICollection<Teacher> Teachers { get; set; }
        public List<SelectListItem> PlaceList { get; set; }
        public IEnumerable<DanceStyleListItem> AvailableStyles { get; set; }
        public PostedStyles PostedStyles { get; set; }
    }
}
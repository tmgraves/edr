using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EDR.Enums;

namespace EDR.Models.ViewModels
{
    public class PlaceDetailViewModel
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
        public List<SelectListItem> TeacherList { get; set; }
        public List<SelectListItem> DanceStyleList { get; set; }
        public SkillLevel SkillLevelList { get; set; }
    }
}
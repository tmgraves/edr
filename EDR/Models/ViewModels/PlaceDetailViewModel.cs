using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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
        public List<SelectListItem> TeacherList { get; set; }
        public List<SelectListItem> DanceStyleList { get; set; }
        public List<SelectListItem> SkillLevelList
        {
            get
            {
                return new List<SelectListItem>()
                {
                    new SelectListItem { Value = "1", Text = "Beginner" },
                    new SelectListItem { Value = "2", Text = "Beginner/Intermediate" },
                    new SelectListItem { Value = "3", Text = "Intermediate" },
                    new SelectListItem { Value = "4", Text = "Intermediate/Advanced" },
                    new SelectListItem { Value = "5", Text = "Advanced" },
                };
            }
        }
    }
}
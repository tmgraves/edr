using EDR.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace EDR.Models.ViewModels
{
    public class TeacherViewViewModel
    {
        public Teacher Teacher { get; set; }
        public Address Address { get; set; }
        public EventListViewModel Events { get; set; }
        public ClassNewViewModel NewClassModel { get; set; }
        public EventListViewModel NewClasses { get; set; }
        public IEnumerable<ApplicationUser> NewStudents { get; set; }
        public IEnumerable<EventMedia> MediaUpdates { get; set; }
        public List<RoleName> Roles { get; set; }
        public List<FacebookEvent> FacebookEvents { get; set; }
    }

    public class TeacherListViewModel
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

        //  Map Settings
        public double? NELat { get; set; }
        public double? NELng { get; set; }
        public double? SWLat { get; set; }
        public double? SWLng { get; set; }
        public double? CenterLat { get; set; }
        public double? CenterLng { get; set; }
        public int? Zoom { get; set; }

        //  Results
        public IEnumerable<Teacher> Teachers { get; set; }
    }

    public class TeacherManageViewModel
    {
        public Teacher Teacher { get; set; }
        public int? NewStyleId { get; set; }
    }
}
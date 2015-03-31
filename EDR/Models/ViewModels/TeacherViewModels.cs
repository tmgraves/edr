using EDR.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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
}
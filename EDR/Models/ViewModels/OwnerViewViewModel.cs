using EDR.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EDR.Models.ViewModels
{
    public class OwnerViewModel
    {
        public Owner Owner { get; set; }
    }

    public class OwnerViewViewModel : OwnerViewModel
    {
        // TODO: FILL IN PROPERTIES NEEDED FOR VIEW
        public Address Address { get; set; }
        public List<RoleName> Roles { get; set; }
        public IEnumerable<Class> NewClasses { get; set; }
        public IEnumerable<Social> NewSocials { get; set; }
        public IEnumerable<Teacher> Teachers { get; set; }
        public IEnumerable<ApplicationUser> Dancers { get; set; }
    }
}
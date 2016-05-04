using EDR.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

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

    public class OwnerListViewModel
    {
        //  Search Fields
        [Display(Name = "Owner:")]
        public string OwnerId { get; set; }
        public string OwnerName { get; set; }
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
        public IEnumerable<Owner> Owners { get; set; }
    }
}
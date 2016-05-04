using EDR.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace EDR.Models.ViewModels
{
    public class PromoterViewViewModel
    {
        // TODO: FILL IN PROPERTIES NEEDED FOR VIEW

        public Promoter Promoter { get; set; }
        public Address Address { get; set; }
        public IEnumerable<EventMedia> MediaUpdates { get; set; }
        public EventListViewModel NewSocials { get; set; }
        public IEnumerable<ApplicationUser> NewDancers { get; set; }
        public EventListViewModel Events { get; set; }
        public List<RoleName> Roles { get; set; }
        public List<FacebookEvent> FacebookEvents { get; set; }
    }

    public class PromoterListViewModel
    {
        //  Search Fields
        [Display(Name = "Promoter:")]
        public string PromoterId { get; set; }
        public string PromoterName { get; set; }
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
        public IEnumerable<Promoter> Promoters { get; set; }
    }
}
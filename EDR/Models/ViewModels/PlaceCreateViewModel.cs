using EDR.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EDR.Models.ViewModels
{
    public class PlaceCreateViewModel
    {
        public Place Place { get; set; }
        public PlaceType PlaceType { get; set; }
        public int PlaceId { get; set; }
        [Display(Name = "Name of the Location")]
        public string Name { get; set; }
        public string Address { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public State State { get; set; }
        public string Zip { get; set; }
        public string Country { get; set; }
        public string FacebookLink { get; set; }
        public string Website { get; set; }
    }
}
using EDR.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EDR.Models.ViewModels
{
    public class PlaceListViewModel
    {
        public double? NELat { get; set; }
        public double? NELng { get; set; }
        public double? SWLat { get; set; }
        public double? SWLng { get; set; }
        public double? CenterLat { get; set; }
        public double? CenterLng { get; set; }
        public IEnumerable<Place> Places { get; set; }
        [Display(Name = "Dance Style:")]
        public IEnumerable<DanceStyle> DanceStyles { get; set; }
        public int? DanceStyleId { get; set; }
        [Display(Name = "Your Location:", Prompt = "Enter your location here")]
        public string Location { get; set; }
        public Address SearchAddress { get; set; }
        public int Zoom { get; set; }
        [Display(Name = "Place Type:")]
        public PlaceType? PlaceType { get; set; }
    }
}
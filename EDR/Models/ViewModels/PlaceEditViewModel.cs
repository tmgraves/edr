using EDR.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EDR.Models.ViewModels
{
    public class PlaceEditViewModel
    {
        public Place Place { get; set; }
        public PlaceType PlaceType { get; set; }
        public State State { get; set; }
    }
}
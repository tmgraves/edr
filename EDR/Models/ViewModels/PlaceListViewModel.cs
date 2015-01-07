using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EDR.Models.ViewModels
{
    public class PlaceListViewModel
    {
        public IEnumerable<Place> Places { get; set; }
    }
}
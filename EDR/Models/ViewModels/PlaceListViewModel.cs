using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EDR.Models.ViewModels
{
    public class PlaceListViewModel
    {
        public IEnumerable<Studio> Studios { get; set; }
        public IEnumerable<ConferenceCenter> ConferenceCenters { get; set; }
        public IEnumerable<Hotel> Hotels { get; set; }
        public IEnumerable<Nightclub> Nightclubs { get; set; }
        public IEnumerable<Restaurant> Restaurants { get; set; }
        public IEnumerable<Theater> Theaters { get; set; }
    }
}
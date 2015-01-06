using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EDR.Models.ViewModels
{
    public class OwnerViewViewModel
    {
        // TODO: FILL IN PROPERTIES NEEDED FOR VIEW

        public Owner Owner { get; set; }
        public Address Address { get; set; }
        public List<Studio> Studios { get; set; }
        public List<ConferenceCenter> ConferenceCenters { get; set; }
        public List<Hotel> Hotels { get; set; }
        public List<Nightclub> Nightclubs { get; set; }
        public List<OtherPlace> OtherPlaces { get; set; }
        public List<Restaurant> Restaurants { get; set; }
        public List<Theater> Theaters { get; set; }
    }
}
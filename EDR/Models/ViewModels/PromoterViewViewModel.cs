using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EDR.Models.ViewModels
{
    public class PromoterViewViewModel
    {
        // TODO: FILL IN PROPERTIES NEEDED FOR VIEW

        public Promoter Promoter { get; set; }
        public Address Address { get; set; }

        public EventListViewModel Events { get; set; }
    }
}
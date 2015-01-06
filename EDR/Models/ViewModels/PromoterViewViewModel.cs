﻿using System;
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

        public IEnumerable<Social> Socials { get; set; }
        public IEnumerable<Concert> Concerts { get; set; }
        public IEnumerable<Conference> Conferences { get; set; }
        public IEnumerable<OpenHouse> OpenHouses { get; set; }
        public IEnumerable<Party> Parties { get; set; }
    }
}
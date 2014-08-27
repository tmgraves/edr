using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EDR.Models
{
    public class Owner : ApplicationUser
    {
        public string ContactEmail { get; set; }

        public ICollection<Place> Places { get; set; }
    }
}
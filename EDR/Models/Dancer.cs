using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EDR.Models
{
    public class Dancer : ApplicationUser
    {
        public int Experience { get; set; }

        public ICollection<DanceStyle> DanceStyles { get; set; }
        public ICollection<Event> Events { get; set; }
    }
}
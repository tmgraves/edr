using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EDR.Models
{
    public class DanceStyle : Entity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string YouTubeVideoID { get; set; }

        public ICollection<Event> Events { get; set; }

        public ICollection<Series> Series { get; set; }

        public ICollection<Teacher> Teachers { get; set; }
    }
}
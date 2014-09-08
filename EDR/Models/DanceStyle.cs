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

        public ICollection<ApplicationUser> Dancers { get; set; }
        public ICollection<Event> Events { get; set; }
        public ICollection<Series> Series { get; set; }
        public ICollection<Teacher> Teachers { get; set; }
    }

    public class DanceStyleListItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsSelected { get; set; }
        public object Tags { get; set; }
    }

    public class PostedStyles
    {
        //this array will be used to POST values from the form to the controller
        public string[] DanceStyleIds { get; set; }
    }
}
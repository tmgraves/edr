using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EDR.Models
{
    public class EventPlaylist : Playlist
    {
        public Event Event { get; set; }
    }
}
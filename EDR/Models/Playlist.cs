using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EDR.Models
{
    public class Playlist
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public DateTime PublishDate { get; set; }
        public string CoverPhoto { get; set; }
        public string YouTubeUrl { get; set; }
        public ApplicationUser Author { get; set; }
    }
}
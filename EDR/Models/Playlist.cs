using EDR.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EDR.Models
{
    public class Playlist : Entity
    {
        public string Title { get; set; }
        public DateTime PublishDate { get; set; }
        public string CoverPhoto { get; set; }
        public string YouTubeUrl { get; set; }
        public ApplicationUser Author { get; set; }
        public string YouTubeId { get; set; }
        public MediaSource MediaSource { get; set; }
        public virtual ICollection<Video> Videos { get; set; }
        public DateTime UpdatedDate { get; set; }
        public int VideoCount { get; set; }
    }
}
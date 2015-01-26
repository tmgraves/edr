using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EDR.Models
{
    public class Video : Entity
    {
        public string Title { get; set; }
        public DateTime PublishDate { get; set; }
        public string VideoUrl { get; set; }
        public string PhotoUrl { get; set; }
        public string FacebookId { get; set; }
        public string YoutubeId { get; set; }
        public string YoutubeUrl { get; set; }
        public string YoutubeThumbnail { get; set; }
        public ApplicationUser Author { get; set; }
        public string YouTubePlaylistUrl { get; set; }
        public string YouTubePlaylistTitle { get; set; }
    }

    public class EventVideo : Video
    {
        public Event Event { get; set; }
    }
}
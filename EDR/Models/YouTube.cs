using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EDR.Models
{
    public class YouTube
    {
    }

    public class YouTubeVideo
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public DateTime PubDate { get; set; }
        public Uri YoutubeLink { get; set; }
        public Uri VideoLink { get; set; }
        public Uri Thumbnail { get; set; }
    }

    public class YouTubePlaylist
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public Uri Url { get; set; }
        public DateTime PubDate { get; set; }
        public Uri ThumbnailUrl { get; set; }
        public int VideoCount { get; set; }
    }
}
using EDR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace EDR.Utilities
{
    public class YouTubeHelper
    {
        public static List<YouTubeVideo> GetVideos(string youTubeUsername)
        {
            try
            {
                List<YouTubeVideo> vidList = new List<YouTubeVideo>();
                string url = "http://gdata.youtube.com/feeds/api/users/" + youTubeUsername + "/uploads?orderby=published";

                XDocument ytDoc = XDocument.Load(url);

                var movies = ytDoc.Descendants().Where(p => p.Name.LocalName == "entry").ToList();

                foreach (var movie in movies)
                {
                    vidList.Add(new YouTubeVideo() { Id = movie.Descendants().Where(p => p.Name.LocalName == "id").FirstOrDefault().Value.Replace("http://gdata.youtube.com/feeds/api/videos/", ""), Title = movie.Descendants().Where(p => p.Name.LocalName == "title").FirstOrDefault().Value });
                }

                return vidList;
            }
            catch
            {
                return new List<YouTubeVideo>();
            }
        }
    }
}
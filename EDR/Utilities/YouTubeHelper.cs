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
                    vidList.Add(new YouTubeVideo() { Id = movie.Descendants().Where(p => p.Name.LocalName == "id").FirstOrDefault().Value.Replace("http://gdata.youtube.com/feeds/api/videos/", ""), Title = movie.Descendants().Where(p => p.Name.LocalName == "title").FirstOrDefault().Value, PubDate = Convert.ToDateTime(movie.Descendants().Where(p => p.Name.LocalName == "published").FirstOrDefault().Value) });
                }

                List<YouTubeVideo> vidList1 = new List<YouTubeVideo>();
                string url1 = "https://gdata.youtube.com/feeds/api/playlists/PLNq9iPaF9V7lQzj0zalgZAJR72-JsNVjk?orderby=published";    //string url1 = "https://www.googleapis.com/youtube/v3/users/" + youTubeUsername + "/uploads?orderby=published";
                XDocument ytDoc1 = XDocument.Load(url1);
                var movies1 = ytDoc1.Descendants().Where(p => p.Name.LocalName == "entry").ToList();
                foreach (var movie in movies1)
                {
                    var vidPath = movie.Descendants().Where(p => p.Name.LocalName == "link").FirstOrDefault().Attribute("href").Value;
                    Uri vidUri = new Uri(vidPath);//  new Uri("http://www.example.com?param1=good&param2=bad");
                    string vidId = HttpUtility.ParseQueryString(vidUri.Query).Get("v");
                    vidList1.Add(new YouTubeVideo() { Id = vidId, Title = movie.Descendants().Where(p => p.Name.LocalName == "title").FirstOrDefault().Value, PubDate = Convert.ToDateTime(movie.Descendants().Where(p => p.Name.LocalName == "published").FirstOrDefault().Value) });
                }
                
                return vidList;
            }
            catch(Exception ex)
            {
                return new List<YouTubeVideo>();
            }
        }

        public static List<YouTubePlaylist> GetPlaylists(string youTubeUsername)
        {
            try
            {
                List<YouTubePlaylist> plLists = new List<YouTubePlaylist>();
                string url = "http://gdata.youtube.com/feeds/api/users/" + youTubeUsername + "/playlists";
            
                XDocument ytDoc = XDocument.Load(url);
                var lists = ytDoc.Descendants().Where(p => p.Name.LocalName == "entry").ToList();
                foreach (var list in lists)
                {
                    var listPath = list.Descendants().Where(p => p.Name.LocalName == "link" && p.Attribute("rel").Value == "alternate").FirstOrDefault().Attribute("href").Value;
                    Uri listUri = new Uri(listPath);
                    string listId = HttpUtility.ParseQueryString(listUri.Query).Get("list");
                    //  var thumbUrl = list.Descendants().Where(p => p.Name.LocalName == "media:thumbnail").FirstOrDefault().Attribute("href").Value;
                    plLists.Add(new YouTubePlaylist() { Id = listId, Name = list.Descendants().Where(p => p.Name.LocalName == "title").FirstOrDefault().Value, PubDate = Convert.ToDateTime(list.Descendants().Where(p => p.Name.LocalName == "published").FirstOrDefault().Value), Url = listPath });
                }

                return plLists;
            }
            catch(Exception ex)
            {
                return new List<YouTubePlaylist>();
            }
        }

        public static List<YouTubeVideo> GetPlaylistVideos(string listId)
        {
            try
            {
                List<YouTubeVideo> vidList = new List<YouTubeVideo>();
                string url = "https://gdata.youtube.com/feeds/api/playlists/" + listId + "?orderby=published";    //string url1 = "https://www.googleapis.com/youtube/v3/users/" + youTubeUsername + "/uploads?orderby=published";
                XDocument ytDoc = XDocument.Load(url);
                var movies = ytDoc.Descendants().Where(p => p.Name.LocalName == "entry").ToList();
                foreach (var movie in movies)
                {
                    var vidPath = movie.Descendants().Where(p => p.Name.LocalName == "link").FirstOrDefault().Attribute("href").Value;
                    Uri vidUri = new Uri(vidPath);//  new Uri("http://www.example.com?param1=good&param2=bad");
                    string vidId = HttpUtility.ParseQueryString(vidUri.Query).Get("v");
                    vidList.Add(new YouTubeVideo() { Id = vidId, Title = movie.Descendants().Where(p => p.Name.LocalName == "title").FirstOrDefault().Value, PubDate = Convert.ToDateTime(movie.Descendants().Where(p => p.Name.LocalName == "published").FirstOrDefault().Value), Thumbnail = new Uri("https://img.youtube.com/vi/" + vidId + "/mqdefault.jpg"), VideoLink = vidUri });
                }

                return vidList;
            }
            catch (Exception ex)
            {
                return new List<YouTubeVideo>();
            }
        }

        public static YouTubePlaylist GetPlaylist(string playlistUrl)
        {
            try
            {
                Uri listUri = new Uri(playlistUrl);
                string listId = HttpUtility.ParseQueryString(listUri.Query).Get("list");
                var plList = new YouTubePlaylist() { Id = listId, Name = "No Title", PubDate = DateTime.Now, Url = playlistUrl };

                return plList;
            }
            catch (Exception ex)
            {
                return new YouTubePlaylist();
            }
        }

        public static YouTubeVideo GetVideo(string videoUrl)
        {
            try
            {
                Uri vidUri = new Uri(videoUrl);//  new Uri("http://www.example.com?param1=good&param2=bad");
                string vidId = HttpUtility.ParseQueryString(vidUri.Query).Get("v");
                var ytVideo = new YouTubeVideo() { Id = vidId, Title = "No Title", PubDate = DateTime.Now };
                return ytVideo;
            }
            catch (Exception ex)
            {
                return new YouTubeVideo();
            }
        }
    }
}
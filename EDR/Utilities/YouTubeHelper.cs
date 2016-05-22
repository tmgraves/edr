using EDR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;

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
                    XDocument xlist = XDocument.Load("https://gdata.youtube.com/feeds/api/playlists/" + listId + "?v=2");
                    //foreach (var node in xlist.Descendants())
                    //{
                    //    var x = node;
                    //    var mg = list.Descendants().Where(p => p.Name.LocalName == "group").FirstOrDefault();
                    //    var thb = mg.Descendants().Where(m => m.Name.LocalName == "thumbnail").FirstOrDefault();
                    //}
                    var thumbnail = list.Descendants().Where(p => p.Name.LocalName == "group").FirstOrDefault().Descendants().Where(m => m.Name.LocalName == "thumbnail").FirstOrDefault().Attribute("url").Value;
                    //  var thumbUrl = list.Descendants().Where(p => p.Name.LocalName == "media:thumbnail").FirstOrDefault().Attribute("href").Value;
                    plLists.Add(new YouTubePlaylist() { Id = listId, Name = list.Descendants().Where(p => p.Name.LocalName == "title").FirstOrDefault().Value, PubDate = Convert.ToDateTime(list.Descendants().Where(p => p.Name.LocalName == "published").FirstOrDefault().Value), Url = new Uri(listPath), ThumbnailUrl = new Uri(thumbnail) });
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
                var youtubeService = new YouTubeService(new BaseClientService.Initializer()
                {
                    ApiKey = "AIzaSyCQZYhgRAjXZdBM2qCEYbZ9vO0T9eyyfjc",
                    ApplicationName = "EatDanceRepeat"
                });

                var listVideos = youtubeService.PlaylistItems.List("snippet");
                listVideos.PlaylistId = listId;
                listVideos.MaxResults = 50;

                var response = listVideos.Execute();
                List<YouTubeVideo> vidList = response.Items.Select(i => new YouTubeVideo() { Id = i.Snippet.ResourceId.VideoId, PubDate = Convert.ToDateTime(i.Snippet.PublishedAt), Title = i.Snippet.Title, Thumbnail = new Uri(i.Snippet.Thumbnails.Default.Url), VideoLink = new Uri("https://youtu.be/" + i.Snippet.ResourceId.VideoId) }).ToList();
                
                foreach (var playlistItem in response.Items)
                {
                    // Print information about each video.
                    var title = playlistItem.Snippet.Title;
                    var vidid = playlistItem.Snippet.ResourceId.VideoId;
                    var path = "https://youtu.be/" + playlistItem.Snippet.ResourceId.VideoId;
                }

                //  List<YouTubeVideo> vidList = new List<YouTubeVideo>();

                ////  string url = "https://gdata.youtube.com/feeds/api/playlists/" + listId + "?orderby=published";    //string url1 = "https://www.googleapis.com/youtube/v3/users/" + youTubeUsername + "/uploads?orderby=published";
                //string url = "https://www.googleapis.com/youtube/v3/playlistItems?part=snippet&maxResults=50&playlistId=" + listId + "&key=AIzaSyCQZYhgRAjXZdBM2qCEYbZ9vO0T9eyyfjc";

                //XDocument ytDoc = XDocument.Load(url);
                //var movies = ytDoc.Descendants().Where(p => p.Name.LocalName == "entry").ToList();
                //foreach (var movie in movies)
                //{
                //    var vidPath = movie.Descendants().Where(p => p.Name.LocalName == "link").FirstOrDefault().Attribute("href").Value;
                //    Uri vidUri = new Uri(vidPath);//  new Uri("http://www.example.com?param1=good&param2=bad");
                //    string vidId = HttpUtility.ParseQueryString(vidUri.Query).Get("v");
                //    vidList.Add(new YouTubeVideo() { Id = vidId, Title = movie.Descendants().Where(p => p.Name.LocalName == "title").FirstOrDefault().Value, PubDate = Convert.ToDateTime(movie.Descendants().Where(p => p.Name.LocalName == "published").FirstOrDefault().Value), Thumbnail = new Uri("https://img.youtube.com/vi/" + vidId + "/mqdefault.jpg"), VideoLink = vidUri });
                //}

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

                if (listId == null)
                {
                    listId = listUri.Segments[1];
                }

                var youtubeService = new YouTubeService(new BaseClientService.Initializer()
                {
                    ApiKey = "AIzaSyCQZYhgRAjXZdBM2qCEYbZ9vO0T9eyyfjc",
                    ApplicationName = "EatDanceRepeat"
                });

                var list = youtubeService.Playlists.List("snippet, contentDetails");
                list.Id = listId;
                var response = list.Execute();

                YouTubePlaylist ytList = response.Items.Select(i => new YouTubePlaylist() { Id = i.Id, PubDate = Convert.ToDateTime(i.Snippet.PublishedAt), Name = i.Snippet.Title, ThumbnailUrl = new Uri(i.Snippet.Thumbnails.Medium.Url), Url = listUri, VideoCount = Convert.ToInt32(i.ContentDetails.ItemCount) }).FirstOrDefault();

                return ytList;
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
                Uri videoUri = new Uri(videoUrl);
                string videoId = HttpUtility.ParseQueryString(videoUri.Query).Get("v");

                if (videoId == null)
                {
                    videoId = videoUri.Segments[1];
                }

                var youtubeService = new YouTubeService(new BaseClientService.Initializer()
                {
                    ApiKey = "AIzaSyCQZYhgRAjXZdBM2qCEYbZ9vO0T9eyyfjc",
                    ApplicationName = "EatDanceRepeat"
                });

                var video = youtubeService.Videos.List("snippet, contentDetails");
                video.Id = videoId;
                var response = video.Execute();

                YouTubeVideo ytVideo = response.Items.Select(i => new YouTubeVideo() { Id = i.Id, PubDate = Convert.ToDateTime(i.Snippet.PublishedAt), Title = i.Snippet.Title, Thumbnail = new Uri(i.Snippet.Thumbnails.Medium.Url), VideoLink = videoUri, YoutubeLink = videoUri }).FirstOrDefault();
                return ytVideo;
            }
            catch (Exception ex)
            {
                return new YouTubeVideo();
            }
        }
    }
}
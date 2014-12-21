/**/
using EDR.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using System.Net;
using EDR.Models;
using System.Data.Entity;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

namespace EDR.Controllers
{
  /// <summary>
  /// YouTube Data API v3 sample: retrieve my uploads.
  /// Relies on the Google APIs Client Library for .NET, v1.7.0 or higher.
  /// See https://code.google.com/p/google-api-dotnet-client/wiki/GettingStarted
  /// </summary>
    public class YouTubeController : BaseController
    {
        public ActionResult List()
        {
            List<YouTubeVideo> VidList = GetVideos();
            return View(VidList);
        }

        public struct feedEntry
        {
            public string id;
            public DateTime published;
            public DateTime updated;
        }

        public struct feedResult
        {
           public string id;
           public DateTime updated;
           public string title;
           public Uri logo;
           public List<feedEntry> entrys;
        }

        private List<YouTubeVideo> GetVideos()
        {
            try
            {
                List<YouTubeVideo> vidList = new List<YouTubeVideo>();
                string url = "http://gdata.youtube.com/feeds/api/users/tmgraves1974/uploads?orderby=published";

                //// Write results
                //Response.Write("Video title: " + Title + "<br />" + "Description: " + Description);

                XNamespace ns = "http://www.w3.org/2005/Atom";
                XNamespace openSearch = "http://a9.com/-/spec/opensearchrss/1.0/";
                XNamespace gd = "http://schemas.google.com/g/2005";
                XNamespace media = "http://search.yahoo.com/mrss/";
                XNamespace yt = "http://gdata.youtube.com/schemas/2007";

                string youTubeUrl = url;
                XDocument ytDoc = XDocument.Load(youTubeUrl);

                var y = ytDoc.DescendantNodes();
                var z = ytDoc.Descendants("feed");
                var t = ytDoc.Descendants("entry");

                var movies = ytDoc.Descendants().Where(p => p.Name.LocalName == "entry").ToList();

                string n;
                foreach (var movie in movies)
                {
                    vidList.Add(new YouTubeVideo() { Id = movie.Descendants().Where(p => p.Name.LocalName == "id").FirstOrDefault().Value.Replace("http://gdata.youtube.com/feeds/api/videos/", ""), Title = movie.Descendants().Where(p => p.Name.LocalName == "title").FirstOrDefault().Value });
                }

                //Console.WriteLine("TITLE: " + entry["title"]["$t"]);
                //Console.WriteLine("DESC : " + entry["media$group"]["media$description"]["$t"]);
                //foreach (var thumbnail in entry["media$group"]["media$thumbnail"])
                //{
                //    Console.WriteLine(thumbnail["url"]);
                //}

                //System.Net.WebClient wc = new System.Net.WebClient();
                //string s = wc.DownloadString(url);

                //JObject JObj = (JObject)JsonConvert.DeserializeObject(s);

                //string x = "1";
                //var entry = JObj["entry"];

                //vidList.Add(new YouTubeVideo() { Id =  }); 

                //Console.WriteLine("TITLE: " + entry["title"]["$t"]);
                //Console.WriteLine("DESC : " + entry["media$group"]["media$description"]["$t"]);
                //foreach (var thumbnail in entry["media$group"]["media$thumbnail"])
                //{
                //    Console.WriteLine(thumbnail["url"]);
                //}

                //using (var webClient = new System.Net.WebClient())
                //{
                //    var json = webClient.DownloadString(url);
                //    // Now parse with JSON.Net
                //    JObject results = JObject.Parse(json);
                //}

                //using (var httpClient = new HttpClient())
                //{
                //    var json = httpClient.Getstr(url);

                //    JObject results = JObject.Parse(json);

                //    // Now parse with JSON.Net
                //}

                return vidList;
            }
            catch
            {
                return new List<YouTubeVideo>();
            }
        }
        
        //public async Task<List<YouTube>> GetVideos()
        //{
        //    List<YouTube> vidList = new List<YouTube>();
        //    UserCredential credential;
        //    //using (var stream = new FileStream("c:\\googlekey.json", FileMode.Open, FileAccess.Read))
        //    //{
        //    //    credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
        //    //        GoogleClientSecrets.Load(stream).Secrets,
        //    //        // This OAuth 2.0 access scope allows for read-only access to the authenticated 
        //    //        // user's account, but not other types of account access.
        //    //        new[] { YouTubeService.Scope.YoutubeReadonly },
        //    //        "user",
        //    //        CancellationToken.None,
        //    //        new FileDataStore(this.GetType().ToString())
        //    //    );
        //    //}
        //    credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
        //                new ClientSecrets
        //                {
        //                    ClientId = "59661408772-mbrooq12346ikinrkdkvrquup4br8ue3.apps.googleusercontent.com",
        //                    ClientSecret = "cnbprrNI-MflAEnWhldq7rkA"
        //                },
        //        // This OAuth 2.0 access scope allows for read-only access to the authenticated 
        //        // user's account, but not other types of account access.
        //        new[] { YouTubeService.Scope.YoutubeReadonly },
        //        "user",
        //        CancellationToken.None,
        //        new FileDataStore(this.GetType().ToString())
        //    );

        //    var youtubeService = new YouTubeService(new BaseClientService.Initializer()
        //    {
        //        HttpClientInitializer = credential,
        //        ApplicationName = this.GetType().ToString()
        //    });

        //    var channelsListRequest = youtubeService.Channels.List("contentDetails");
        //    channelsListRequest.Mine = true;

        //    // Retrieve the contentDetails part of the channel resource for the authenticated user's channel.
        //    var channelsListResponse = await channelsListRequest.ExecuteAsync();

        //    foreach (var channel in channelsListResponse.Items)
        //    {
        //        // From the API response, extract the playlist ID that identifies the list
        //        // of videos uploaded to the authenticated user's channel.
        //        var uploadsListId = channel.ContentDetails.RelatedPlaylists.Uploads;

        //        Console.WriteLine("Videos in list {0}", uploadsListId);

        //        var nextPageToken = "";
        //        while (nextPageToken != null)
        //        {
        //            var playlistItemsListRequest = youtubeService.PlaylistItems.List("snippet");
        //            playlistItemsListRequest.PlaylistId = uploadsListId;
        //            playlistItemsListRequest.MaxResults = 50;
        //            playlistItemsListRequest.PageToken = nextPageToken;

        //            // Retrieve the list of videos uploaded to the authenticated user's channel.
        //            var playlistItemsListResponse = await playlistItemsListRequest.ExecuteAsync();

        //            foreach (var playlistItem in playlistItemsListResponse.Items)
        //            {
        //                // Print information about each video.
        //                vidList.Add(new YouTube() { YouTubeMovieID = playlistItem.Snippet.ResourceId.VideoId, YouTubeMovieTitle = playlistItem.Snippet.Title });
        //                //Console.WriteLine("{0} ({1})", playlistItem.Snippet.Title, playlistItem.Snippet.ResourceId.VideoId);
        //            }

        //            nextPageToken = playlistItemsListResponse.NextPageToken;
        //        }
        //    }
        //    return vidList;
        //}
    }
}

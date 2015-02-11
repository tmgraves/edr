using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EDR.Models;
using System.Collections.Specialized;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EDR.Utilities
{
    public class SpotifyHelper
    {
        public static List<SpotifyPlaylist> GetPlaylists(string token, string userid)
        {
            var lstPlaylists = new List<SpotifyPlaylist>();
            //NameValueCollection parameters = new NameValueCollection();
            //parameters.Add("access_token", token);

            WebClient client = new WebClient();
            var result = client.DownloadString("https://api.spotify.com/v1/users/" + userid + "/playlists?access_token=" + token);
            // deserializing nested JSON string to object  
            var jsResult = JsonConvert.DeserializeObject<SpotifyPlaylistsResponse>(result);

            //foreach (var lst in jsResult.items)
            //{
            //    dynamic images = lst.images;
            //    dynamic tracks = lst.tracks;
            //    var image = images[0].url;
            //}
            return jsResult.items.ToList();
        }

        public static SpotifyAccessToken GetAccessToken(string code)
        {
            var client_id = "44d7c94dd6c847ff93d25447c09d37ca";
            var client_secret = "03b18ba62015498bb19d45fb1898dd55";
            var redirect_uri = "https://localhost:44302/SocialMedia/AuthenticateSpotify";

            NameValueCollection parameters = new NameValueCollection();
            parameters.Add("client_id", client_id);
            parameters.Add("client_secret", client_secret);
            parameters.Add("grant_type", "authorization_code");
            parameters.Add("redirect_uri", redirect_uri);
            parameters.Add("code", code);

            WebClient client = new WebClient();
            var result = client.UploadValues("https://accounts.spotify.com/api/token", "POST", parameters);
            var response = System.Text.Encoding.Default.GetString(result);
            // deserializing nested JSON string to object  
            var jsToken = JsonConvert.DeserializeObject<SpotifyAccessToken>(response);

            return jsToken;
        }
    }
}
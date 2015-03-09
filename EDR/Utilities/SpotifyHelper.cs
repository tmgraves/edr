using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EDR.Models;
using System.Collections.Specialized;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Configuration;

namespace EDR.Utilities
{
    public class SpotifyHelper
    {
        public static List<SpotifyPlaylist> GetPlaylists(ref SpotifyAccessToken token, string spotifyid)
        {
            var lstPlaylists = new List<SpotifyPlaylist>();
            //NameValueCollection parameters = new NameValueCollection();
            //parameters.Add("access_token", token);

            WebClient client = new WebClient();
            try
            {
                var test = client.DownloadString("https://api.spotify.com/v1/me?access_token=" + token.Access_Token);
            }
            catch (WebException ex)
            {
                token = GetAccessToken(token.Refresh_Token, SpotifyGrantType.refresh_token);
            }
            
            var result = client.DownloadString("https://api.spotify.com/v1/users/" + spotifyid + "/playlists?access_token=" + token.Access_Token);

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

        public static SpotifyAccessToken GetAccessToken(string code, SpotifyGrantType grant_type = SpotifyGrantType.authorization_code)
        {
            var client_id = ConfigurationManager.AppSettings["SpotifyClientId"];
            var client_secret = ConfigurationManager.AppSettings["SpotifyAppSecret"];
            var redirect_uri = ConfigurationManager.AppSettings["SpotifyRedirectUri"];

            NameValueCollection parameters = new NameValueCollection();

            if (grant_type == SpotifyGrantType.authorization_code)
            {
                parameters.Add("client_id", client_id);
                parameters.Add("client_secret", client_secret);
                parameters.Add("grant_type", grant_type.ToString());
                parameters.Add("redirect_uri", redirect_uri);
                parameters.Add("code", code);
            }
            else
            {
                parameters.Add("client_id", client_id);
                parameters.Add("client_secret", client_secret);
                parameters.Add("grant_type", grant_type.ToString());
                parameters.Add("redirect_uri", redirect_uri);
                parameters.Add("refresh_token", code);
            }

            WebClient client = new WebClient();
            var result = client.UploadValues("https://accounts.spotify.com/api/token", "POST", parameters);
            var response = System.Text.Encoding.Default.GetString(result);
            // deserializing nested JSON string to object  
            var jsToken = JsonConvert.DeserializeObject<SpotifyAccessToken>(response);

            return jsToken;
        }
    }
}
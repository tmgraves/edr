using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EDR.Models
{
    public class SpotifyPlaylist
    {
        public string href { get; set; }
        public string id { get; set; }
        public string name  { get; set; }
        public string uri  { get; set; }
        public SpotifyTracks tracks { get; set; }
        public SpotifyImage[] images { get; set; }
    }

    public class SpotifyAccessToken
    {
        public string Access_Token { get; set; }
        public string Token_Type { get; set; }
        public int Expires_In { get; set; }
        public string Refresh_Token { get; set; }
    }

    public class SpotifyTracks
    {
        public string href;
        public string total;
    }

    public class SpotifyImage
    {
        public string height;
        public string url;
        public string width;
    }

    public class SpotifyPlaylistsResponse
    {
        public SpotifyPlaylist[] items { get; set; }
    }

    public enum SpotifyGrantType
    {
        authorization_code,
        refresh_token
    }
}
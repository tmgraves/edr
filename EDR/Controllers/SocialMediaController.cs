using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using System.Data.Entity;
using System.Text;
using EDR.Utilities;
using System.Configuration;
using TweetSharp;

namespace EDR.Controllers
{
    [RoutePrefix("SocialMedia")]
    public class SocialMediaController : BaseController
    {
        // GET: Instagram
        [Route("Index")]
        public ActionResult Index()
        {
            return View();
        }

        //  Get Auth Token
        [Route("AddInstagram")]
        [Authorize]
        public ActionResult AddInstagram()
        {
            var client_id = ConfigurationManager.AppSettings["InstagramClientId"];
            var redirect_uri = ConfigurationManager.AppSettings["InstagramRedirectUri"];
            return Redirect("https://api.instagram.com/oauth/authorize/?client_id=" + client_id + "&redirect_uri=" + redirect_uri + "&response_type=code");  
        }

        [Route("Authenticate")]
        [Authorize]
        public ActionResult Authenticate(string code)
        {
            var client_id = ConfigurationManager.AppSettings["InstagramClientId"];
            var client_secret = ConfigurationManager.AppSettings["InstagramClientSecret"];
            var redirect_uri = ConfigurationManager.AppSettings["InstagramRedirectUri"];
            NameValueCollection parameters = new NameValueCollection();
            parameters.Add("client_id", client_id);
            parameters.Add("client_secret", client_secret);
            parameters.Add("grant_type", "authorization_code");
            parameters.Add("redirect_uri", redirect_uri);
            parameters.Add("code", code);

            WebClient client = new WebClient();
            var result = client.UploadValues("https://api.instagram.com/oauth/access_token", "POST", parameters);
            var response = System.Text.Encoding.Default.GetString(result);
            // deserializing nested JSON string to object  
            var jsResult = (JObject)JsonConvert.DeserializeObject(response);
            string accessToken = (string)jsResult["access_token"];
            int id = (int)jsResult["user"]["id"];
            var username = (string)jsResult["user"]["username"];
            var userid = User.Identity.GetUserId();
            var user = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();
            user.InstagramToken = accessToken;
            user.InstagramId = id;
            user.InstagramUsername = username;
            DataContext.Entry(user).State = EntityState.Modified;
            DataContext.SaveChanges();

            return RedirectToAction("SocialMedia", "Dancer", new { username = User.Identity.Name });
        }

        //  Get Auth Token
        [Route("AddSpotify")]
        [Authorize]
        public ActionResult AddSpotify()
        {
            //  https://accounts.spotify.com/en/authorize?response_type=code&client_id=8897482848704f2a8f8d7c79726a70d4&redirect_uri=https:%2F%2Fdeveloper.spotify.com%2Fweb-api%2Fconsole%2Fcallback&scope=playlist-read-private%20playlist-modify-public
            var client_id = ConfigurationManager.AppSettings["SpotifyClientId"];
            //  var client_secret = "03b18ba62015498bb19d45fb1898dd55";
            var redirect_uri = ConfigurationManager.AppSettings["SpotifyRedirectUri"];
            return Redirect("https://accounts.spotify.com/en/authorize/?client_id=" + client_id + "&redirect_uri=" + redirect_uri + "&response_type=code&scope=playlist-read-private%20playlist-modify-public");
        }

        [Route("AuthenticateSpotify")]
        [Authorize]
        public ActionResult AuthenticateSpotify(string code)
        {
            var token = SpotifyHelper.GetAccessToken(code);

            WebClient client = new WebClient();
            var profileresult = client.DownloadString("https://api.spotify.com/v1/me?access_token=" + token.Access_Token);
            dynamic jsProfile = (JObject)JsonConvert.DeserializeObject<dynamic>(profileresult);
            var id = jsProfile.id;
            var userid = User.Identity.GetUserId();
            var user = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();
            user.SpotifyToken = token.Access_Token;
            user.SpotifyRefreshToken = token.Refresh_Token;
            user.SpotifyId = id;
            DataContext.Entry(user).State = EntityState.Modified;
            DataContext.SaveChanges();

            return RedirectToAction("SocialMedia", "Dancer", new { username = User.Identity.Name });
        }

        //  Twitter Authentication
        [Route("AddTwitter")]
        [Authorize]
        public ActionResult AddTwitter()
        {
            TwitterClientInfo twitterClientInfo = new TwitterClientInfo();
            twitterClientInfo.ConsumerKey = ConfigurationManager.AppSettings["TwitterAPIKey"]; //Read ConsumerKey out of the app.config
            twitterClientInfo.ConsumerSecret = ConfigurationManager.AppSettings["TwitterAPISecret"]; //Read the ConsumerSecret out the app.config
            TwitterService twitterService = new TwitterService(twitterClientInfo);

            //Firstly we need the RequestToken and the AuthorisationUrl
            var requestToken = twitterService.GetRequestToken("https://localhost:44302/SocialMedia/AuthenticateTwitter");
            var authUrl = twitterService.GetAuthorizationUri(requestToken);
            //authUrl is just a URL we can open IE and paste it in if we want
            //  Console.WriteLine("Please Allow This App to send Tweets on your behalf");
            return Redirect(authUrl.ToString()); //Launches a browser that'll go to the AuthUrl.

            //var API_key = "API key";
            //var API_secret = "API secret";
            //var Access_token_secret = "Access token";
            //var Access_token = "Access token";
            //var service = new TwitterService(API_key, API_secret);
            //service.AuthenticateWith(Access_token, Access_token_secret);
            //var token = service.GetRequestToken();
            //var api_key = ConfigurationManager.AppSettings["TwitterAPIKey"];
            //var client_secret = ConfigurationManager.AppSettings["TwitterAPISecret"];
            //var redirect_uri = ConfigurationManager.AppSettings["TwitterRedirectUri"];
            //return Redirect("http://twitter.com/oauth/authorize?client_id=" + client_id + "&redirect_uri=" + redirect_uri + "&response_type=code&scope=playlist-read-private%20playlist-modify-public");
            //return View();
        }

        [Route("AuthenticateTwitter")]
        public ActionResult AuthenticateTwitter(string oauth_token, string oauth_verifier)
        {
            var requestToken = new OAuthRequestToken { Token = oauth_token };

            TwitterService service = new TwitterService("key", "secret");
            OAuthAccessToken accessToken = service.GetAccessToken(requestToken, oauth_verifier);

            var userid = User.Identity.GetUserId();
            var user = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();
            user.TwitterUsername = accessToken.ScreenName;
            user.TwitterUserId = accessToken.UserId;
            user.TwitterToken = accessToken.Token;
            user.TwitterSecret = accessToken.TokenSecret;
            DataContext.Entry(user).State = EntityState.Modified;
            DataContext.SaveChanges();

            //  service.ListTweetsOnHomeTimeline()

            service.AuthenticateWith(accessToken.Token, accessToken.TokenSecret);

            return RedirectToAction("Index", "Home");
        }
        
        //public ActionResult AuthenticateTwitter(string oauth_token)
        //{
        //    TwitterClientInfo twitterClientInfo = new TwitterClientInfo();
        //    var service = new TwitterService();
        //    service.AuthenticateWith()

        //    return RedirectToAction("SocialMedia", "Dancer", new { username = User.Identity.Name });
        //}
        //  Twitter Authentication
    }
}
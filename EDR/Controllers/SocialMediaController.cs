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

namespace EDR.Controllers
{
    public class SocialMediaController : BaseController
    {
        // GET: Instagram
        public ActionResult Index()
        {
            return View();
        }

        //  Get Auth Token
        public ActionResult AddInstagram()
        {
            var client_id = "56c5b09843504985b032bd76c8657a16";
            var redirect_uri = "https://localhost:44302/SocialMedia/Authenticate";
            return Redirect("https://api.instagram.com/oauth/authorize/?client_id=" + client_id + "&redirect_uri=" + redirect_uri + "&response_type=code");  
        }

        public ActionResult Authenticate(string code)
        {
            var client_id = "56c5b09843504985b032bd76c8657a16";
            var client_secret = "2d6f838d229a4b45af5d54750cbeacb0";
            var redirect_uri = "https://localhost:44302/SocialMedia/Authenticate";
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
    }
}
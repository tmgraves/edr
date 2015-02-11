using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InstaSharp;
using EDR.Models;
using System.Collections.Specialized;
using System.Net;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace EDR.Utilities
{
    public class InstagramHelper
    {
        public static List<InstagramPicture> GetPictures(string token, int id)
        {
            var lstPictures = new List<InstagramPicture>();
            //NameValueCollection parameters = new NameValueCollection();
            //parameters.Add("access_token", token);

            WebClient client = new WebClient();
            var result = client.DownloadString("https://api.instagram.com/v1/users/" + id.ToString() + "/media/recent?access_token=" + token);
            //  var response = System.Text.Encoding.Default.GetString(result);
            // deserializing nested JSON string to object  
            dynamic jsResult = (JObject)JsonConvert.DeserializeObject<dynamic>(result);

            foreach (dynamic item in jsResult.data)
            {
                if (item.type == "image")
                {
                    dynamic images = item.images;
                    lstPictures.Add(new InstagramPicture() { InstagramId = item.id, Caption = item.caption != null ? item.caption.text : "No Caption", Thumbnail = images.thumbnail.url, Photo = images.standard_resolution.url, PhotoDate = UnixTimeStampToDateTime((double)item.created_time), Link = item.link });
                }
            }

            //string accessToken = (string)jsResult["access_token"];
            //int id = (int)jsResult["user"]["id"];
            //var username = (string)jsResult["user"]["username"];
            //var userid = User.Identity.GetUserId();
            //var user = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();

            return lstPictures;
        }

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
    }
}
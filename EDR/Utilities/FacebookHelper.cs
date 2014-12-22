using EDR.Models;
using Facebook;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EDR.Utilities
{
    public class FacebookHelper
    {
        public static List<FacebookPhoto> GetPhotos(string token)
        {
            var fb = new FacebookClient(token);
            ArrayList imageSize;
            //Get the album data
            var photoList = new List<FacebookPhoto>();
            dynamic albums = fb.Get("me/albums");
            foreach (dynamic albumInfo in albums.data)
            {
                //Get the Pictures inside the album this gives JASON objects list that has photo attributes 
                // described here http://developers.facebook.com/docs/reference/api/photo/
                dynamic albumsPhotos = fb.Get(albumInfo.id + "/photos");

                foreach (dynamic pictures in albumsPhotos.data)
                {
                    imageSize = new ArrayList();
                    foreach (dynamic rsa in pictures.images)
                    {
                        imageSize.Add(rsa.height);
                    }
                    photoList.Add(new FacebookPhoto()
                    {
                        Album = albumInfo.name,
                        Id = pictures.id,
                        Name = pictures.name,
                        Link = pictures.link,
                        Source = pictures.picture,
                        LargeSource = pictures.source,
                        PhotoDate = DateTime.Parse(pictures.created_time)
                    });
                }
            }
            return(photoList);
        }

        public static List<FacebookFriend> GetFriends(string token)
        {
            var fb = new FacebookClient(token);
            dynamic myInfo = fb.Get("/me/friends?fields=id,name,email,link");
            var friendsList = new List<FacebookFriend>();
            foreach (dynamic friend in myInfo.data)
            {
                friendsList.Add(new FacebookFriend()
                {
                    Id = friend.id,
                    Name = friend.name,
                    Link = friend.link,
                    ImageURL = @"https://graph.facebook.com/" + friend.id + "/picture?type=small",
                    Email = friend.email
                });
            }
            return(friendsList);
        }
    }
}
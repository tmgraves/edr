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

        public static List<FacebookEvent> GetEvents(string token)
        {
            var fb = new FacebookClient(token);
            dynamic myInfo = fb.Get("/me/events?fields=id,cover,description,end_time,is_date_only,location,name,owner,privacy,start_time,ticket_uri,timezone,updated_time");
            var eventsList = new List<FacebookEvent>();
            foreach (dynamic ev in myInfo.data)
            {
                if (DateTime.Parse(ev.start_time) >= DateTime.Today)
                {
                    FacebookPhoto coverPic = new FacebookPhoto();
                    if (ev.cover != null)
                    {
                        coverPic.Id = ev.cover.id;
                        coverPic.LargeSource = ev.cover.source;
                    }

                    eventsList.Add(new FacebookEvent()
                    {
                        Id = ev.id,
                        Description = ev.description,
                        EndTime = ev.end_time != null ? DateTime.Parse(ev.end_time) : null,
                        IsDateOnly = ev.is_date_only,
                        Location = ev.location,
                        Name = ev.name,
                        //  Owner = ev.owner,
                        Privacy = ev.privacy,
                        StartTime = DateTime.Parse(ev.start_time),
                        TicketUri = ev.ticket_uri,
                        Timezone = ev.timezone,
                        Updated = DateTime.Parse(ev.updated_time),
                        EventLink = @"https://www.facebook.com/events/" + ev.id,
                        CoverPhoto = coverPic
                    });
                }
            }
            return (eventsList);
        }
    }
}
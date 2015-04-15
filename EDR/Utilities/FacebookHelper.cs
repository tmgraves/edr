using EDR.Data;
using EDR.Models;
using Facebook;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using EDR.Controllers;
using System.Configuration;

namespace EDR.Utilities
{
    public class FacebookHelper : BaseController
    {
        public static string GetGlobalToken()
        {
            ApplicationDbContext context = new ApplicationDbContext();
            var email = ConfigurationManager.AppSettings["GlobalFacebookUserEmail"];
            var user = context.Users.Where(u => u.Email == email).FirstOrDefault();
            return user.FacebookToken;
        }

        public static dynamic GetData(string token, string query)
        {
            var fb = new FacebookClient(token);
            dynamic data = fb.Get(query);
            return (data);
        }

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

        public static List<FacebookPhoto> GetAlbumPhotos(string token, string albumid)
        {
            var fb = new FacebookClient(token);
            ArrayList imageSize;
            //Get the album data
            var photoList = new List<FacebookPhoto>();
            //Get the Pictures inside the album this gives JASON objects list that has photo attributes 
            // described here http://developers.facebook.com/docs/reference/api/photo/
            dynamic albumPhotos = fb.Get(albumid + "/photos");

            foreach (dynamic pictures in albumPhotos.data)
            {
                imageSize = new ArrayList();
                foreach (dynamic rsa in pictures.images)
                {
                    imageSize.Add(rsa.height);
                }
                photoList.Add(new FacebookPhoto()
                {
                    Id = pictures.id,
                    Name = pictures.name,
                    Link = pictures.link,
                    Source = pictures.picture,
                    LargeSource = pictures.source,
                    PhotoDate = DateTime.Parse(pictures.created_time)
                });
            }
            return (photoList);
        }

        public static FacebookPhoto GetPhoto(FacebookClient client, string id)
        {
            dynamic fbphoto = client.Get(id);

            var photo = new FacebookPhoto()
            {
                Id = fbphoto.id,
                Name = fbphoto.name,
                Link = fbphoto.link,
                Source = fbphoto.picture,
                LargeSource = fbphoto.source,
                PhotoDate = DateTime.Parse(fbphoto.created_time)
            };

            return(photo);
        }

        public static List<FacebookAlbum> GetAlbums(string token)
        {
            var fb = new FacebookClient(token);
            //Get the album data
            var albumList = new List<FacebookAlbum>();
            dynamic albums = fb.Get("me/albums");
            foreach (dynamic album in albums.data)
            {
                var newalbum = new FacebookAlbum()
                {
                        Id = album.id,
                        Name = album.name,
                        Count = album.count != null ? album.count : 0,
                        Created_Time = DateTime.Parse(album.created_time),
                        Description = album.description,
                        Link = album.link,
                        //Location = album.loaction,
                        //Privacy = album.privacy,
                        Updated_Time = DateTime.Parse(album.updated_time)
                };
                if (album.cover_photo != null)
                {
                    var cover = GetPhoto(fb, album.cover_photo);
                    newalbum.Cover_Photo = cover.LargeSource;
                    newalbum.Thumbnail = cover.Source;
                }

                albumList.Add(newalbum);
            }
            return (albumList);
        }

        private static FacebookAlbum GetAlbum(dynamic fbalbum, string token)
        {
            var fb = new FacebookClient(token);
            ArrayList imageSize;
            //Get the album data
            var album = new FacebookAlbum();
            album.Id = fbalbum.id;

            var photoList = new List<FacebookPhoto>();
            //Get the Pictures inside the album this gives JASON objects list that has photo attributes 
            // described here http://developers.facebook.com/docs/reference/api/photo/
            dynamic albumsPhotos = fb.Get(fbalbum.id + "/photos");

            foreach (dynamic pictures in albumsPhotos.data)
            {
                imageSize = new ArrayList();
                foreach (dynamic rsa in pictures.images)
                {
                    imageSize.Add(rsa.height);
                }
                photoList.Add(new FacebookPhoto()
                {
                    Album = fbalbum.name,
                    Id = pictures.id,
                    Name = pictures.name,
                    Link = pictures.link,
                    Source = pictures.picture,
                    LargeSource = pictures.source,
                    PhotoDate = DateTime.Parse(pictures.created_time)
                });
            }

            album.Photos = photoList;
            return (album);
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

        public static List<FacebookEvent> GetEvents(string token, DateTime? startdate = null)
        {
            var fb = new FacebookClient(token);
            var query = "/me/events?fields=id,cover,description,end_time,is_date_only,location,name,owner,privacy,start_time,ticket_uri,timezone,updated_time,venue,parent_group";
            if (startdate != null)
            {
                query = query + "&since=" + Convert.ToDateTime(startdate).ToString("o");
            }
            dynamic myInfo = fb.Get(query);
            var eventList = new List<FacebookEvent>();
            dynamic paging;
            dynamic next;
            if (myInfo != null)
            {
                foreach (dynamic ev in myInfo.data)
                {
                    eventList.Add(BuildEvent(fb, ev));
                }

                if (startdate == null)
                {
                    paging = myInfo.paging;
                    if (paging != null)
                    {
                        next = paging.next;
                        while (next != null)
                        {
                            myInfo = fb.Get(next);
                            foreach (dynamic ev in myInfo.data)
                            {
                                eventList.Add(BuildEvent(fb, ev));
                            }
                            paging = myInfo.paging;
                            if (paging != null)
                            {
                                next = paging.next;
                            }
                            else
                            {
                                next = null;
                            }
                        }
                    }
                }
            }
            return (eventList);
        }

        private static FacebookEvent BuildEvent(FacebookClient client, dynamic eventdata)
        {
            FacebookPhoto coverPic = new FacebookPhoto();
            if (eventdata.cover != null)
            {
                coverPic.Id = eventdata.cover.id;
                coverPic.LargeSource = eventdata.cover.source;
            }

            var add = new FacebookAddress();
            if (eventdata.venue != null)
            {
                add.City = eventdata.venue.city;
                add.Country = eventdata.venue.country;
                add.Latitude = eventdata.venue.latitude == null ? 0.0 : eventdata.venue.latitude;
                add.Longitude = eventdata.venue.longitude == null ? 0.0 : eventdata.venue.longitude;
                add.State = eventdata.venue.state;
                add.Street = eventdata.venue.street;
                add.ZipCode = eventdata.venue.zip;
                add.FacebookId = eventdata.venue.id;
                var address = Geolocation.ParseAddress(add.Street + " " + add.City + " " + add.State + " " + add.ZipCode);
                if (address != null)
                {
                    add.Country = address.Country;
                }

                //  Get Place from Page
                if (eventdata.venue.id != null)
                {
                    var place = client.Get(eventdata.venue.id);
                    add.WebsiteUrl = place.website;
                    add.FacebookUrl = "https://www.facebook.com/" + place.username;
                    add.Location = place.name;
                }
                //  Place is not a Page
                else
                {
                    if (eventdata.venue.name != null)
                    {
                        address = Geolocation.ParseAddress(eventdata.venue.name);

                        if (address != null)
                        {
                            add.City = address.City;
                            add.Country = address.Country;
                            add.Latitude = address.Latitude;
                            add.Longitude = address.Longitude;
                            add.State = address.State != null ? address.State.ToString() : null;
                            add.Street = address.Street;
                            add.ZipCode = address.ZipCode;
                        }
                    }
                }
                //  Get Place
            }

            var evt = new FacebookEvent()
            {
                Id = eventdata.id,
                Description = eventdata.description,
                EndTime = eventdata.end_time != null ? DateTime.Parse(eventdata.end_time) : null,
                IsDateOnly = eventdata.is_date_only,
                Location = add.Location,
                Name = eventdata.name,
                //  Owner = ev.owner,
                Privacy = eventdata.privacy,
                StartTime = DateTime.Parse(eventdata.start_time),
                TicketUri = eventdata.ticket_uri,
                Timezone = eventdata.timezone,
                Updated = DateTime.Parse(eventdata.updated_time),
                EventLink = @"https://www.facebook.com/events/" + eventdata.id,
                CoverPhoto = coverPic,
                Address = add
            };

            return evt;
        }


        public static FacebookEvent GetEvent(string id, string token, string fields = "id,cover,description,end_time,is_date_only,location,name,owner,privacy,start_time,ticket_uri,timezone,updated_time,venue,parent_group")
        {
            var fb = new FacebookClient(token);
            dynamic evt = fb.Get("/" + id + "?fields=" + fields);
            var fbevent = BuildEvent(fb, evt);
            return fbevent;
        }

        public static List<FacebookVideo> GetVideos(string token)
        {
            var fb = new FacebookClient(token);
            dynamic myInfo;
            var videosList = new List<FacebookVideo>();
            dynamic paging;
            dynamic next;

            //  Get Tagged Videos
            myInfo = fb.Get("/me/videos?fields=id,created_time,description,embed_html,format,from,icon,name,picture,source,updated_time");
            if (myInfo != null)
            {
                ParseVideos(myInfo.data, videosList);
                paging = myInfo.paging;
                if (paging != null)
                {
                    next = paging.next;
                    while (next != null)
                    {
                        myInfo = fb.Get(next);
                        
                        ParseVideos(myInfo.data, videosList);

                        paging = myInfo.paging;
                        if (paging != null)
                        {
                            next = paging.next;
                        }
                        else
                        {
                            next = null;
                        }
                    }
                }
            }
            //  Get Tagged Videos

            //  Get Uploaded Videos
            myInfo = fb.Get("/me/videos/uploaded?fields=id,created_time,description,embed_html,format,from,icon,name,picture,source,updated_time");
            if (myInfo != null)
            {
                ParseVideos(myInfo.data, videosList);

                paging = myInfo.paging;
                if (paging != null)
                {
                    next = paging.next;
                    while (next != null)
                    {
                        myInfo = fb.Get(next);

                        ParseVideos(myInfo.data, videosList);

                        paging = myInfo.paging;
                        if (paging != null)
                        {
                            next = paging.next;
                        }
                        else
                        {
                            next = null;
                        }
                    }
                }
            }
            //  Get Uploaded Videos

            return (videosList);
        }

        public static void ParseVideos(dynamic data, List<FacebookVideo> lstVideos)
        {
            foreach (dynamic vid in data)
            {
                dynamic from = vid.from;
                lstVideos.Add(new FacebookVideo()
                {
                    Id = vid.id,
                    Created_Time = Convert.ToDateTime(vid.created_time),
                    Description = vid.description,
                    Embed_Html = vid.embed_html,
                    Icon = vid.icon,
                    Name = vid.name,
                    Picture = vid.picture,
                    Source = vid.source,
                    Updated_Time = Convert.ToDateTime(vid.updated_time)
                });
            }
        }

        public static List<FacebookGroup> GetGroups(string token)
        {
            var fb = new FacebookClient(token);
            dynamic myInfo = fb.Get("/me/groups?fields=id,description,email,icon,link,name,owner,privacy,updated_time,feed");

            var groupsList = new List<FacebookGroup>();
            foreach (dynamic group in myInfo.data)
            {
                dynamic from = group.owner;
                groupsList.Add(new FacebookGroup()
                {
                    Id = group.id,
                    Description = group.description,
                    Email = group.email,
                    Icon = group.icon,
                    Link = group.link,
                    Name = group.name,
                    Privacy = group.privacy,
                    Updated_Time = Convert.ToDateTime(group.updated_time),
                    Posts = ParsePosts(group.feed)
                });
            }
            return (groupsList);
        }

        public static List<FacebookPost> ParsePosts(dynamic feed)
        {
            var posts = new List<FacebookPost>();
            if (feed != null)
            {
                foreach (dynamic postdata in feed.data)
                {
                    posts.Add(new FacebookPost() { Id = postdata.id, Message = postdata.message, Picture = postdata.picture, Link = postdata.link, Source = postdata.source, Description = postdata.description, Icon = postdata.icon, Type = postdata.type, Object_Id = postdata.object_id, Created_Time = Convert.ToDateTime(postdata.created_time), Updated_Time = Convert.ToDateTime(postdata.updated_time) });
                }
            }
            return posts;
        }

        public static List<FacebookPost> GetFeed(string objectId, string token)
        {
            try
            {
                var fb = new FacebookClient(token);
                dynamic feed = fb.Get(objectId + "/feed");

                var posts = new List<FacebookPost>();
                if (feed != null)
                {
                    foreach (dynamic postdata in feed.data)
                    {
                        posts.Add(new FacebookPost() { Id = postdata.id, Message = postdata.message, Picture = postdata.picture, Link = postdata.link, Source = postdata.source, Description = postdata.description, Icon = postdata.icon, Type = postdata.type, Object_Id = postdata.object_id, Created_Time = Convert.ToDateTime(postdata.created_time), Updated_Time = Convert.ToDateTime(postdata.updated_time) });
                    }
                }

                dynamic paging = feed.paging;
                dynamic next;
                if (paging != null)
                {
                    next = paging.next;
                    while (next != null)
                    {
                        feed = fb.Get(next);

                        foreach (dynamic postdata in feed.data)
                        {
                            posts.Add(new FacebookPost() { Id = postdata.id, Message = postdata.message, Picture = postdata.picture, Link = postdata.link, Source = postdata.source, Description = postdata.description, Icon = postdata.icon, Type = postdata.type, Object_Id = postdata.object_id, Created_Time = Convert.ToDateTime(postdata.created_time), Updated_Time = Convert.ToDateTime(postdata.updated_time) });
                        }

                        paging = feed.paging;
                        if (paging != null)
                        {
                            next = paging.next;
                        }
                        else
                        {
                            next = null;
                        }
                    }
                }

                return posts;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
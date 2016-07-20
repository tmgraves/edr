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
using EDR.Enums;
using System.Data.Entity;

namespace EDR.Utilities
{
    public class FacebookHelper : BaseController
    {
        public static string GetToken()
        {
            var fb = new FacebookClient();
            dynamic result = fb.Get("oauth/access_token", new
            {
                client_id = ConfigurationManager.AppSettings["FacebookAppId"], // "app_id",
                client_secret = ConfigurationManager.AppSettings["FacebookAppSecret"], // "app_secret",
                grant_type = "client_credentials"
            });
            return (result.access_token);
        }

        public static string GetGlobalToken()
        {
            ApplicationDbContext context = new ApplicationDbContext();
            var email = ConfigurationManager.AppSettings["GlobalFacebookUserEmail"];
            var user = context.Users.Where(u => u.Email == email).FirstOrDefault();
            return user.FacebookToken;
        }

        public static bool CheckToken(string token)
        {
            var valid = true;

            try
            {
                var client = new FacebookClient(token);
                dynamic result = client.Get("me/friends");
            }
            catch (FacebookOAuthException)
            {
                valid = false;
                // Our access token is invalid or expired
                // Here we need to do something to handle this.
            }

            return valid;
        }

        public static string GetExtendedAccessToken(string token)
        {
            FacebookClient client = new FacebookClient();
            string extendedToken = "";
            try
            {
                dynamic result = client.Get("/oauth/access_token", new
                {
                    grant_type = "fb_exchange_token",
                    client_id = ConfigurationManager.AppSettings["FacebookAppId"],
                    client_secret = ConfigurationManager.AppSettings["FacebookAppSecret"],
                    fb_exchange_token = token
                });
                extendedToken = result.access_token;
            }
            catch (Exception ex)
            {
                extendedToken = token;
            }
            return extendedToken;
        }
        
        public static dynamic GetData(string token, string query)
        {
            var fb = new FacebookClient(token);
            dynamic data = fb.Get(query);
            return (data);
        }

        public static List<FacebookPhoto> GetPhotos(string token)
        {
            try
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
                return (photoList);
            }
            catch (Exception ex)
            {
                return null;
            }

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
                query = query + "&since=" + Convert.ToDateTime(startdate).AddDays(-14).ToString("o");
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

        private static FacebookEvent BuildEvent(FacebookClient client, dynamic eventdata, bool rebuild = false)
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
                add.Location = eventdata.venue.name;
                if (add.FacebookId != null)
                {
                    dynamic placedata = client.Get(add.FacebookId);
                    add.CoverPhotoUrl = placedata.cover != null ? placedata.cover.source : null;
                    add.ThumbnailUrl = placedata.cover != null ? placedata.cover.source : null;
                    if (placedata.category_list != null)
                    {
                        foreach (dynamic category in placedata.category_list)
                        {
                            add.Categories.Add(new FacebookCategory() { Id = category.id, Name = category.name });
                        }
                    }
                    //  add.Categories = placedata.category_list != null ? placedata.category_list.Select(new FacebookCategory() { Id = placedata.category_list.id, Name = placedata.category_list.name }) : null;
                }

                var address = new Address();

                //if (!rebuild)
                //{
                //    if (add.City == null || add.State == null)
                //    {
                //        if (eventdata.location != null)
                //        {
                //            address = Geolocation.ParseAddress(eventdata.location);

                //            if (address != null)
                //            {
                //                add.City = address.City;
                //                add.Country = address.Country;
                //                add.Latitude = address.Latitude;
                //                add.Longitude = address.Longitude;
                //                add.State = address.State != null ? address.State.ToString() : null;
                //                add.Street = address.Street;
                //                add.ZipCode = address.ZipCode;
                //                add.Country = address.Country;
                //                add.GooglePlaceId = address.GooglePlaceId;
                //                add.GoogleUrl = address.GoogleUrl;
                //                add.GoogleRating = address.GoogleRating;
                //                if (add.Website == null)
                //                {
                //                    add.Website = address.Website;
                //                }
                //                if (add.Name == null)
                //                {
                //                    add.Name = address.Name;
                //                }
                //            }
                //        }
                //    }
                //    else
                //    {
                //        address = Geolocation.ParseAddress(add.Street + " " + add.City + " " + add.State + " " + add.ZipCode);
                //    }
                //}

                //if (address != null)
                //{
                //    add.Country = address.Country;
                //    add.GooglePlaceId = address.GooglePlaceId;
                //    add.GoogleUrl = address.GoogleUrl;
                //    add.GoogleRating = address.GoogleRating;
                //    if (add.Website == null)
                //    {
                //        add.Website = address.Website;
                //    }
                //    if (add.Name == null)
                //    {
                //        add.Name = address.Name;
                //    }
                //}

                //  Get Place from Page
                if (eventdata.venue.id != null)
                {
                    var place = client.Get(eventdata.venue.id);

                    add.WebsiteUrl = place.website;
                    add.FacebookUrl = place.link;
                    add.Location = place.name;
                }
                ////  Place is not a Page
                //else
                //{
                //    if (!rebuild)
                //    {
                //        if (eventdata.venue.name != null)
                //        {
                //            add.Location = eventdata.venue.name;
                //            address = Geolocation.ParseAddress(eventdata.venue.name);

                //            if (address != null)
                //            {
                //                add.City = address.City;
                //                add.Country = address.Country;
                //                add.Latitude = address.Latitude;
                //                add.Longitude = address.Longitude;
                //                add.State = address.State != null ? address.State.ToString() : null;
                //                add.Street = address.Street;
                //                add.ZipCode = address.ZipCode;
                //                add.Country = address.Country;
                //                add.GooglePlaceId = address.GooglePlaceId;
                //                add.GoogleUrl = address.GoogleUrl;
                //                add.GoogleRating = address.GoogleRating;
                //                if (add.Website == null)
                //                {
                //                    add.Website = address.Website;
                //                }
                //                if (add.Name == null)
                //                {
                //                    add.Name = address.Name;
                //                }
                //            }
                //        }
                //    }
                //}
                ////  Get Place
            }

            var feed = new List<FacebookPost>();
            if (eventdata.feed != null)
            {
                foreach (dynamic f in eventdata.feed.data)
                {
                    if (f.message != null || f.link != null)
                    {
                        feed.Add(new FacebookPost() { Link = f.link != null ? f.link : null, Message = f.message, Updated_Time = DateTime.Parse(f.updated_time) });
                    }
                }
            }

            var evt = new FacebookEvent()
            {
                Id = eventdata.id,
                Description = eventdata.description,
                EndTime = eventdata.end_time != null ? DateTime.Parse(eventdata.end_time) : null,
                //  IsDateOnly = eventdata.is_date_only,
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
                Address = add,
                Feeds = feed,
                UserToken = client.AccessToken
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

        public static FacebookGroup GetGroup(string id, string token, string fields = "id,description,email,icon,link,name,owner,privacy,updated_time")
        {
            var fb = new FacebookClient(token);
            dynamic fbgroup = fb.Get(id + "?fields=" + fields);

            var group = new FacebookGroup()
            {
                Id = fbgroup.id,
                Description = fbgroup.description,
                Email = fbgroup.email,
                Icon = fbgroup.icon,
                Link = fbgroup.link,
                Name = fbgroup.name,
                Privacy = fbgroup.privacy,
                Updated_Time = Convert.ToDateTime(fbgroup.updated_time),
                Posts = ParsePosts(fbgroup.feed)
            };
            return (group);
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
                fb.Version = "v2.6";
                dynamic feed = fb.Get(objectId + "/feed?fields=link,message,name,updated_time,full_picture,type,id,source,icon,object_id,created_time");

                var posts = new List<FacebookPost>();
                if (feed != null)
                {
                    foreach (dynamic postdata in feed.data)
                    {
                        posts.Add(new FacebookPost() { Id = postdata.id, Message = postdata.message, Picture = postdata.full_picture, Link = postdata.link, Source = postdata.source, Description = postdata.description, Icon = postdata.icon, Type = postdata.type, Object_Id = postdata.object_id, Created_Time = Convert.ToDateTime(postdata.created_time), Updated_Time = Convert.ToDateTime(postdata.updated_time) });
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

        public static void RefreshEvents()
        {
            ApplicationDbContext context = new ApplicationDbContext();

            //  Get Events/Facebook Tokens for all users
            var users = context.Users.Where(u => context.Events.Any(e => e.Creator.Id == u.Id)).ToList();
            var eventList = new List<FacebookEvent>();
            var evtList = new List<Event>();
            foreach (var u in users)
            {
                var evts = context.Events.Where(e => e.FacebookId != null && e.Creator.Id == u.Id).OrderBy(e => e.CheckedDate).Take(20).ToList();
                var uevts = String.Join(",", evts.Where(e => e.Creator.Id == u.Id).Select(s => s.FacebookId));
                eventList = GetEvents(uevts, u.FacebookToken, eventList);
                evtList.AddRange(evts);
            }

            //  Refresh event details in database
            foreach (var fevt in eventList)
            {
                var evnt = context.Events.Where(e => e.FacebookId == fevt.Id).Include("Feeds").Include("LinkedFacebookObjects").Include("Creator").FirstOrDefault();

                if (evnt.UpdatedDate < fevt.Updated)
                {
                    var available = (fevt.Privacy == "OPEN" || fevt.Privacy == "FRIENDS") ? true : false;
                    evnt.Name = fevt.Name;
                    evnt.Description = fevt.Description;
                    //evnt.StartDate = fevt.StartTime;
                    //evnt.StartTime = fevt.StartTime;
                    //evnt.EndDate = fevt.EndTime;
                    //evnt.EndTime = fevt.EndTime;
                    evnt.PhotoUrl = fevt.CoverPhoto.LargeSource;
                    evnt.FacebookLink = fevt.EventLink;
                    evnt.IsAvailable = available;
                    evnt.UpdatedDate = fevt.Updated;

                    //  Update Feeds
                    context.Feeds.RemoveRange(evnt.Feeds);
                    var feedList = fevt.Feeds.Select(f => new Feed() { Link = f.Link, Message = f.Message, UpdateTime = f.Updated_Time }).ToList();
                    ////  Add Linked Object Feeds
                    //feedList.AddRange(GetFeed(String.Join(",", evnt.LinkedFacebookObjects.Select(o => o.FacebookId)), evnt.Creator.FacebookToken).Select(f => new Feed() { Link = f.Link, Message = f.Message, UpdateTime = f.Updated_Time }));
                    ////  Add Linked Object Feeds
                    evnt.Feeds = feedList;

                    //  Place is a Page in Facebook
                    if (fevt.Address.FacebookId != null)
                    {
                        var place = context.Places.Where(p => p.FacebookId == fevt.Address.FacebookId).FirstOrDefault();

                        //  Existing Facebook place in the database
                        if (place != null)
                        {
                            evnt.Place = place;
                        }
                        //  New Place from Facebook
                        else
                        {
                            //  Place has a Page in Facebook
                            if (fevt.Address.FacebookId != null)
                            {
                                var fbplace = FacebookHelper.GetData(fevt.UserToken, fevt.Address.FacebookId);

                                var placetype = new PlaceType();
                                if (fbplace.category_list != null)
                                {
                                    foreach (dynamic category in fbplace.category_list)
                                    {
                                        string cat = category.name;
                                        //  Search for Dance Instruction category
                                        if (cat.Contains("Dance Instruction") || category.id == "203916779633178")
                                        {
                                            placetype = PlaceType.Studio;
                                            break;
                                        }
                                        else if (cat.Contains("Dance Club") || category.id == "176139629103647")
                                        {
                                            placetype = PlaceType.Nightclub;
                                            break;
                                        }
                                        else if (category.id == "273819889375819" || cat.Contains("Restaurant"))
                                        {
                                            placetype = PlaceType.Restaurant;
                                            break;
                                        }
                                        else if (cat.Contains("Hotel") || category.id == "164243073639257")
                                        {
                                            placetype = PlaceType.Hotel;
                                            break;
                                        }
                                        else if (cat.Contains("Meeting Room") || category.id == "210261102322291")
                                        {
                                            placetype = PlaceType.ConferenceCenter;
                                            break;
                                        }
                                        else if (cat.Contains("Theater") || category.id == "173883042668223")
                                        {
                                            placetype = PlaceType.Theater;
                                            break;
                                        }
                                        else
                                        {
                                            placetype = PlaceType.OtherPlace;
                                        }
                                    }
                                }

                                evnt.Place = new Place() { Name = fevt.Location, Address = fevt.Address.Street, City = fevt.Address.City, State = fevt.Address.State != null ? (State)Enum.Parse(typeof(State), fevt.Address.State) : State.CA, Zip = fevt.Address.ZipCode, Country = fevt.Address.Country, Latitude = fevt.Address.Latitude, Longitude = fevt.Address.Longitude, FacebookId = fevt.Address.FacebookId, PlaceType = placetype, Public = true, Website = fevt.Address.WebsiteUrl, FacebookLink = fevt.Address.FacebookUrl, Filename = fbplace.cover != null ? fbplace.cover.source : null, ThumbnailFilename = fbplace.cover != null ? fbplace.cover.source : null };
                            }
                        }
                    }

                }
                evnt.CheckedDate = DateTime.Now;

                if (evnt.EndDate == null)
                {
                    evnt.EndDate = evnt.StartDate;
                }

                //  context.Entry(evnt).State = EntityState.Modified;
                //  context.SaveChanges();
            }
            //  Set Update Date for missing facebook events
            //  var eIds = videos.Where(v => v.FacebookId != null).Select(v => v.FacebookId).ToArray(); 
            //  var che = context.Events.Where(e => evtList.Any(el => el.Id == e.Id) && !eventList.Any(fe => fe.Id == e.FacebookId)).ToList();
            var evntids = evtList.Select(e => e.Id).ToArray();
            var fevntids = eventList.Select(e => e.Id).ToArray();
            context.Events.Where(e => evntids.Contains(e.Id) && !fevntids.Contains(e.FacebookId)).ToList().ForEach(e => { e.UpdatedDate = DateTime.Now; e.CheckedDate = DateTime.Now; });
            //  context.SaveChanges();
            //  Set Update Date for missing facebook events

        }

        public static List<FacebookEvent> GetEvents(string ids, string token, List<FacebookEvent> eventList)
        {
            try
            {
                var fb = new FacebookClient(token);
                dynamic fevts = fb.Get("?ids=" + ids + "&fields=id,cover,description,end_time,location,name,owner,privacy,start_time,ticket_uri,timezone,updated_time,venue,parent_group,feed");

                if (fevts != null)
                {
                    foreach (dynamic ev in fevts)
                    {
                        eventList.Add(BuildEvent(fb, ev.Value, true));
                    }
                }
                return eventList;
            }
            catch(Exception ex)
            {
                return eventList;
            }
        }

        public static PlaceType ParsePlaceType(List<FacebookCategory> list)
        {
            var type = new PlaceType();
            foreach (var cat in list)
            {
                //  Search for Dance Instruction category
                if (cat.Name.Contains("Dance Instruction") || cat.Id == "203916779633178")
                {
                    type = PlaceType.Studio;
                    break;
                }
                else if (cat.Name.Contains("Dance Club") || cat.Id == "176139629103647")
                {
                    type = PlaceType.Nightclub;
                    break;
                }
                else if (cat.Id == "273819889375819" || cat.Name.Contains("Restaurant"))
                {
                    type = PlaceType.Restaurant;
                    break;
                }
                else if (cat.Name.Contains("Hotel") || cat.Id == "164243073639257")
                {
                    type = PlaceType.Hotel;
                    break;
                }
                else if (cat.Name.Contains("Meeting Room") || cat.Id == "210261102322291")
                {
                    type = PlaceType.ConferenceCenter;
                    break;
                }
                else if (cat.Name.Contains("Theater") || cat.Id == "173883042668223")
                {
                    type = PlaceType.Theater;
                    break;
                }
                else
                {
                    type = PlaceType.OtherPlace;
                    break;
                }
            }
            return type;
        }
    }
}
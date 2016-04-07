using EDR.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using EDR.Models;
using System.Data.Entity.Validation;
using EDR.Enums;
using System.Data.Entity;
using EDR.Utilities;
using System.IO;
using Microsoft.AspNet.Identity.EntityFramework;

namespace EDR.Controllers
{
    public class EventController : BaseController
    {
        #region reviews
        public ActionResult Reviews(int id, EventType eventType)
        {
            var model = LoadEvent(id, eventType);
            var reviews = new EventReviewsViewModel();
            reviews.EventReviews = DataContext.Reviews.Where(x => x.Event.Id == id);
            reviews.EventId = id;
            reviews.NewReview = new Review();
            reviews.NewReview.Like = true;
            var userid = User.Identity.GetUserId();
            reviews.NewReview.Author = DataContext.Users.Where(x => x.Id == userid).FirstOrDefault();

            model.Reviews = reviews;

            return View(model);
        }

        public ActionResult Reviews2(int id, EventType eventType)
        {
            var reviews = new EventReviewsViewModel();
            reviews.EventReviews = DataContext.Reviews.Where(x => x.Event.Id == id);
            reviews.EventId = id;
            reviews.NewReview = new Review();
            reviews.NewReview.Like = true;
            var userid = User.Identity.GetUserId();
            reviews.NewReview.Author = DataContext.Users.Where(x => x.Id == userid).FirstOrDefault();

            return View(reviews);
        }

        public ActionResult Reviews_Insert(EventReviewsViewModel model)
        {
            var er = ModelState.IsValid;
            var userid = User.Identity.GetUserId();
            model.NewReview.Author = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();
            model.NewReview.Event = DataContext.Events.Where(e => e.Id == model.EventId).FirstOrDefault();
            var today = DateTime.Today;
            model.NewReview.ReviewDate = today;
            DataContext.Reviews.Add(model.NewReview);
            DataContext.SaveChanges();

            var reviews = DataContext.Reviews.Where(x => x.Event.Id == model.NewReview.Event.Id);
            return PartialView("~/Views/Shared/Events/_Reviews.cshtml", reviews);
        }
        #endregion

        #region attendees
        public ActionResult Attendees(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var model = new EventAttendeesViewModel();

            model.Event = DataContext.Events.Where(e => e.Id == id).FirstOrDefault();

            if (model.Event == null)
            {
                return HttpNotFound();
            }

            return View(model);
        }
        [HttpPost]
        public ActionResult Attendees(EventAttendeesViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var ev = DataContext.Events.Where(e => e.Id == model.Event.Id).Include("Attendees").FirstOrDefault();
                    ev.Attendees.Add(new EventAttendee() { Name = model.Name, Email = model.Email });
                    DataContext.Entry(ev).State = EntityState.Modified;
                    DataContext.SaveChanges();

                    var eventType = ev is Class ? EventType.Class : EventType.Social;
                    return RedirectToAction("Attendees", new { id = ev.Id, eventType = eventType });
                }
                else
                {
                    return View(model);
                }
            }
            catch
            {
                return View(model);
            }
        }
        #endregion

        //public ActionResult Details(int id, EventType eventType)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }

        //    var model = new EventDetailViewModel();
        //    model.Event = DataContext.Events.Include("DanceStyles").Include("Reviews").Where(x => x.Id == id).FirstOrDefault();
        //    model.EventType = eventType;

        //    if (model.Event == null)
        //    {
        //        return HttpNotFound();
        //    }

        //    if (model.Event is Class)
        //    {
        //        model.Teachers = DataContext.Teachers.Where(t => t.Classes.Any(c => c.Id == id)).ToList();
        //    }
        //    else if (model.Event is Workshop)
        //    {
        //        model.Teachers = DataContext.Teachers.Where(t => t.Workshops.Any(w => w.Id == id)).ToList();
        //    }

        //    return View(model);
        //}

        public ActionResult View(int id, EventType eventType)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var model = LoadEvent(id, eventType);

            model.EventType = eventType;

            ////  Update Facebook Event with Current Info
            //if (model.Event.FacebookId != null && model.Event.UpdatedDate < DateTime.Now.AddDays(-7))
            //{
            //    UpdateFacebookEvent(model.Event);
            //}
            ////  Get Current Facebook Picture/Video

            model.LinkedFacebookObjects = DataContext.Events.Where(e => e.Id == id).Include("LinkedFacebookObjects").FirstOrDefault().LinkedFacebookObjects;

            if (model.Event == null)
            {
                return HttpNotFound();
            }

            return View(model);
        }

        protected void UpdateFacebookEvent(Event rEvent)
        {
            var evt = FacebookHelper.GetEvent(rEvent.FacebookId, rEvent.Creator.FacebookToken);

            if (evt != null)
            {
                //  Update Event
                rEvent.PhotoUrl = evt.CoverPhoto != null ? evt.CoverPhoto.LargeSource : rEvent.PhotoUrl;
                rEvent.Name = evt.Name;
                rEvent.Description = evt.Description;
                rEvent.StartDate = evt.StartTime;
                rEvent.StartTime = evt.StartTime;
                rEvent.EndTime = evt.EndTime;
                rEvent.IsAvailable = (evt.Privacy == "OPEN" || evt.Privacy == "FRIENDS") ? true : false;
                //  Update Event

                //  Update Place
                //  Does Event Place match Facebook Place
                if (rEvent.Place.FacebookId != evt.Address.FacebookId)
                {
                    //  Is the Place not tied to a Page
                    if (evt.Address.FacebookId == null)
                    {
                        var place = new Place() { Name = evt.Location, Address = evt.Address.Street, City = evt.Address.City, State = evt.Address.State != null ? (State)Enum.Parse(typeof(State), evt.Address.State) : State.CA, Zip = evt.Address.ZipCode, Country = evt.Address.Country, Latitude = evt.Address.Latitude, Longitude = evt.Address.Longitude, PlaceType = PlaceType.OtherPlace, Public = false };
                        rEvent.Place = place;
                    }
                    else
                    {
                        //  Look up place in database
                        var newplace = DataContext.Places.Where(p => p.FacebookId == evt.Address.FacebookId).FirstOrDefault();

                        //  if not exists, create new place
                        if (newplace == null)
                        {
                            //  Create New Place from Facebook
                            var fbplace = FacebookHelper.GetData(rEvent.Creator.FacebookToken, evt.Address.FacebookId);

                            var placetype = new PlaceType();
                            if (fbplace.category_list != null)
                            {
                                foreach (dynamic category in fbplace.category_list)
                                {
                                    //  Search for Dance Instruction category
                                    if (category.name == "Dance Instruction" || category.id == "203916779633178")
                                    {
                                        placetype = PlaceType.Studio;
                                        break;
                                    }
                                    else if (category.name == "Dance Club" || category.id == "176139629103647")
                                    {
                                        placetype = PlaceType.Nightclub;
                                        break;
                                    }
                                    else if (category.name == "Restaurant" || category.id == "273819889375819")
                                    {
                                        placetype = PlaceType.Restaurant;
                                        break;
                                    }
                                    else if (category.name == "Hotel" || category.id == "164243073639257")
                                    {
                                        placetype = PlaceType.Hotel;
                                        break;
                                    }
                                    else if (category.name == "Meeting Room" || category.id == "210261102322291")
                                    {
                                        placetype = PlaceType.ConferenceCenter;
                                        break;
                                    }
                                    else if (category.name == "Theater" || category.id == "173883042668223")
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

                            newplace = new Place() { Name = evt.Location, Address = evt.Address.Street, City = evt.Address.City, State = evt.Address.State != null ? (State)Enum.Parse(typeof(State), evt.Address.State) : State.CA, Zip = evt.Address.ZipCode, Country = evt.Address.Country, Latitude = evt.Address.Latitude, Longitude = evt.Address.Longitude, FacebookId = evt.Address.FacebookId, PlaceType = placetype, Public = true, Website = evt.Address.WebsiteUrl, FacebookLink = evt.Address.FacebookUrl, Filename = fbplace.cover != null ? fbplace.cover.source : null, ThumbnailFilename = fbplace.cover != null ? fbplace.cover.source : null };
                        }

                        var oldplace = rEvent.Place;
                        rEvent.Place = newplace;

                        //  Remove null Places
                        if (oldplace.FacebookId == null)
                        {
                            DataContext.Places.Remove(oldplace);
                        }
                    }
                }
                else
                {
                    rEvent.Place.Name = evt.Location;
                    rEvent.Place.Address = evt.Address.Street;
                    rEvent.Place.City = evt.Address.City;
                    rEvent.Place.State = evt.Address.State != null ? (State)Enum.Parse(typeof(State), evt.Address.State) : State.CA;
                    rEvent.Place.Zip = evt.Address.ZipCode;
                }

                try
                {
                    DataContext.Entry(rEvent).State = EntityState.Modified;
                    DataContext.SaveChanges();
                }
                catch (DbEntityValidationException e)
                {
                    var msg = "";
                    foreach (var eve in e.EntityValidationErrors)
                    {
                        msg = eve.Entry.Entity.GetType().Name + " " + eve.Entry.State;
                        foreach (var ve in eve.ValidationErrors)
                        {
                            msg = ve.PropertyName + " " + ve.ErrorMessage;
                        }
                    }
                }
                //  Update Page
            }

        }

        public ActionResult Pictures(int id, EventType eventType)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var model = LoadEvent(id, eventType);

            if (model.Event == null)
            {
                return HttpNotFound();
            }

            return View(model);
        }

        public ActionResult Videos(int id, EventType eventType)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var model = LoadEvent(id, eventType);
            model.EventType = eventType;

            if (model.Event == null)
            {
                return HttpNotFound();
            }

            return View(model);
        }
        public ActionResult GetVideos(int id, EventType eventType)
        {
            var model = new EventVideos();
            model.EventType = eventType;
            model.ReturnUrl = Url.Action("Videos", new { id = id, eventType = eventType });

            var evt = DataContext.Events.Where(e => e.Id == id)
                    .Include("Creator")
                    .Include("Videos")
                    .Include("Videos.Author")
                    .Include("Playlists")
                    .Include("Playlists.Author")
                    .Include("LinkedFacebookObjects")
                    .FirstOrDefault();

            var lstMedia = new List<Media>();
            EventHelper.BuildVideos(evt, MediaTarget.Event, ref lstMedia);
            var lstVideos = new List<EventVideo>();
            foreach (var m in lstMedia)
            {
                    lstVideos.Add(new EventVideo() { Event = model.Event, Id = m.Id, Author = m.Author, PublishDate = m.MediaDate, PhotoUrl = m.PhotoUrl, VideoUrl = m.MediaUrl, Title = m.Title, MediaSource = m.MediaSource, PlayList = m.Playlist });
            }

            ////  Get Playlist Videos
            //foreach(var l in evt.Playlists)
            //{
            //    var pvids = YouTubeHelper.GetPlaylistVideos(l.YouTubeId);
            //    var vidids = lstVideos.Select(v => v.YoutubeId).ToArray();
            //    lstVideos.AddRange(pvids.Where(v => !vidids.Contains(v.Id)).Select(v => new EventVideo() { Event = evt, Id = evt.Id, Author = evt.Creator, PublishDate = v.PubDate, PhotoUrl = v.Thumbnail.ToString(), VideoUrl = v.VideoLink.ToString(), Title = v.Title, MediaSource = MediaSource.YouTube, PlayList = l, YoutubeId = v.Id }));
            //}
            
            model.Videos = lstVideos;
            return PartialView("~/Views/Shared/Events/_VideosPartial.cshtml", model);
        }

        public ActionResult GetPictures(int id, EventType eventType)
        {
            var model = new EventPictures();
            model.EventType = eventType;
            model.ReturnUrl = Url.Action("Pictures", new { id = id, eventType = eventType });

            var evt = new Event();
            if (eventType == EventType.Class)
            {
                evt =   DataContext.Events.OfType<Class>()
                    .Where(e => e.Id == id)
                    .Include("Creator")
                    .Include("Pictures")
                    .Include("Pictures.PostedBy")
                    .Include("Albums")
                    .Include("Albums.PostedBy")
                    .Include("LinkedFacebookObjects")
                    .Include("Teachers")
                    .Include("Teachers.ApplicationUser")
                    .Include("Owners")
                    .Include("Owners.ApplicationUser")
                    .FirstOrDefault();
            }
            else
            {
                evt = DataContext.Events.OfType<Social>()
                    .Where(e => e.Id == id)
                    .Include("Creator")
                    .Include("Pictures")
                    .Include("Pictures.PostedBy")
                    .Include("Albums")
                    .Include("Albums.PostedBy")
                    .Include("LinkedFacebookObjects")
                    .Include("Promoters")
                    .Include("Promoters.ApplicationUser")
                    .Include("Owners")
                    .Include("Owners.ApplicationUser")
                    .FirstOrDefault();
            }

            var lstPictures = new List<EventPicture>();
            lstPictures = evt.Pictures.ToList();

            var lstAlbums = new List<EventAlbum>();
            lstAlbums = evt.Albums.ToList();

            foreach (var album in lstAlbums)
            {
                if (album.PostedBy.FacebookToken != null)
                {
                    album.Pictures = FacebookHelper.GetAlbumPhotos(album.PostedBy.FacebookToken, album.FacebookId).Select(p => new Picture() { Album = album, Title = p.Name, PhotoDate = p.PhotoDate, Filename = p.LargeSource, ThumbnailFilename = p.Source, MediaSource = MediaSource.Facebook }).ToList();
                }
            }
            model.Albums = lstAlbums;

            foreach (var ob in evt.LinkedFacebookObjects)
            {
                if (evt.Creator != null && evt.Creator.FacebookToken != null)
                {
                    var posts = new List<FacebookPost>();
                    if (Session["FacebookPosts"] != null)
                    {
                        posts = (List<FacebookPost>)Session["FacebookPosts"];
                    }
                    else
                    {
                        posts = FacebookHelper.GetFeed(ob.FacebookId, evt.Creator.FacebookToken);
                        Session["FacebookPosts"] = posts;
                    }

                    foreach (var post in posts.Where(p => p.Type == "photo"))
                    {
                        lstPictures.Add(new EventPicture() { Event = evt, PostedBy = evt.Creator, PhotoDate = post.Created_Time, Filename = post.Link, ThumbnailFilename = post.Picture, Title = post.Description, MediaSource = MediaSource.Facebook });
                    }
                }
            }

            model.Pictures = lstPictures;

            return PartialView("~/Views/Shared/Events/_PicturesPartial.cshtml", model);
        }

        public ActionResult GetFacebookPictures(int id, EventType eventType)
        {
            var model = new EventFacebookPictureContainer();
            model.EventType = eventType;
            var evt = DataContext.Events.Where(e => e.Id == id).Include("Creator").Include("Pictures").FirstOrDefault();
            model.Event = evt;

            var userid = User.Identity.GetUserId();
            var user = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();
            var token = user.FacebookToken;
            if (token != null)
            {
                if (Session["FacebookPictures"] == null)
                {
                    Session["FacebookPictures"] = FacebookHelper.GetPhotos(token);

                }
                var facebookIds = evt.Pictures.Where(p => p.FacebookId != null).Select(p => p.FacebookId).ToArray();

                model.FacebookPictures = ((List<FacebookPhoto>)Session["FacebookPictures"]).Where(p => !facebookIds.Any(f => f.Contains(p.Id)));
            }

            return PartialView("~/Views/Shared/Events/_AddFacebookPicturesPartial.cshtml", model);
        }

        public ActionResult GetFacebookAlbums(int id, EventType eventType)
        {
            var model = new EventFacebookAlbumContainer();
            model.EventType = eventType;
            var evt = DataContext.Events.Where(e => e.Id == id).Include("Creator").Include("Albums").FirstOrDefault();
            model.Event = evt;

            var userid = User.Identity.GetUserId();
            var user = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();
            var token = user.FacebookToken;
            if (token != null)
            {
                if (Session["FacebookAlbums"] == null)
                {
                    Session["FacebookAlbums"] = FacebookHelper.GetAlbums(token);

                }
                var facebookIds = evt.Albums.Where(a => a.FacebookId != null).Select(a => a.FacebookId).ToArray();

                model.FacebookAlbums = ((List<FacebookAlbum>)Session["FacebookAlbums"]).Where(a => !facebookIds.Any(f => f.Contains(a.Id)));
            }

            return PartialView("~/Views/Shared/Events/_AddFacebookAlbumsPartial.cshtml", model);
        }

        public ActionResult GetFacebookVideos(int id, EventType eventType)
        {
            var model = new EventFacebookVideosContainer();
            model.EventType = eventType;
            var evt = DataContext.Events.Where(e => e.Id == id).Include("Creator").Include("Videos").FirstOrDefault();
            model.Event = evt;

            var userid = User.Identity.GetUserId();
            var user = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();
            var token = user.FacebookToken;
            if (token != null)
            {
                if (Session["FacebookVideos"] == null)
                {
                    Session["FacebookVideos"] = FacebookHelper.GetVideos(token);

                }
                var facebookIds = evt.Videos.Where(p => p.FacebookId != null).Select(p => p.FacebookId).ToArray();

                model.FacebookVideos = ((List<FacebookVideo>)Session["FacebookVideos"]).Where(p => !facebookIds.Any(f => f.Contains(p.Id)));
            }

            return PartialView("~/Views/Shared/Events/_AddFacebookVideosPartial.cshtml", model);
        }

        public ActionResult GetYouTubeVideos(int id, EventType eventType)
        {
            var model = new EventYouTubeVideosContainer();
            model.EventType = eventType;
            var evt = DataContext.Events.Where(e => e.Id == id).Include("Creator").Include("Videos").FirstOrDefault();
            model.Event = evt;

            var userid = User.Identity.GetUserId();
            var user = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();
            var ytUsername = user.YouTubeUsername;
            if (ytUsername != null)
            {
                if (Session["YouTubeVideos"] == null)
                {
                    Session["YouTubeVideos"] = YouTubeHelper.GetVideos(ytUsername);
                }
                var youtubeIds = evt.Videos.Where(p => p.YoutubeId != null).Select(p => p.YoutubeId).ToArray();

                model.YouTubeVideos = ((List<YouTubeVideo>)Session["YouTubeVideos"]).Where(p => !youtubeIds.Any(f => f.Contains(p.Id)));
            }

            return PartialView("~/Views/Shared/Events/_AddYouTubeVideosPartial.cshtml", model);
        }

        public ActionResult GetYouTubePlaylists(int id, EventType eventType)
        {
            var model = new EventYouTubePlaylistsContainer();
            model.EventType = eventType;
            var evt = DataContext.Events.Where(e => e.Id == id).Include("Creator").Include("Playlists").FirstOrDefault();
            model.Event = evt;

            var userid = User.Identity.GetUserId();
            var user = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();
            var ytUsername = user.YouTubeUsername;
            if (ytUsername != null)
            {
                if (Session["YouTubePlaylists"] == null)
                {
                    Session["YouTubePlaylists"] = YouTubeHelper.GetPlaylists(ytUsername);
                }
                var playlistIds = evt.Playlists.Where(p => p.YouTubeId != null).Select(p => p.YouTubeId).ToArray();

                model.YouTubePlaylists = ((List<YouTubePlaylist>)Session["YouTubePlaylists"]).Where(p => !playlistIds.Any(f => f.Contains(p.Id)));
            }

            return PartialView("~/Views/Shared/Events/_AddYouTubePlaylistsPartial.cshtml", model);
        }

        public ActionResult GetInstagramPictures(int id, EventType eventType)
        {
            var model = new EventInstagramPicturesContainer();
            model.EventType = eventType;
            var evt = DataContext.Events.Where(e => e.Id == id).Include("Creator").Include("Pictures").FirstOrDefault();
            model.Event = evt;

            var userid = User.Identity.GetUserId();
            var user = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();
            var token = user.InstagramToken;
            if (token != null)
            {
                if (Session["InstagramPictures"] == null)
                {
                    Session["InstagramPictures"] = InstagramHelper.GetPictures(token, (int)user.InstagramId);

                }
                var instagramIds = evt.Pictures.Where(p => p.InstagramId != null).Select(p => p.InstagramId).ToArray();

                model.InstagramPictures = ((List<InstagramPicture>)Session["InstagramPictures"]).Where(p => !instagramIds.Any(f => f.Contains(p.InstagramId)));
            }

            return PartialView("~/Views/Shared/Events/_AddInstagramPicturesPartial.cshtml", model);
        }

        public ActionResult GetUpdates(int id)
        {
            //  Media Updates
            var lstMedia = new List<EventMedia>();
            //var updates = new EventUpdates();
            //var rebuild = false;

            //if (Session["Event" + id.ToString() + "Updates"] != null)
            //{
            //    updates = (EventUpdates)Session["Event" + id.ToString() + "Updates"];
            //    if (updates.Created < DateTime.Now.AddMinutes(-5))
            //    {
            //        rebuild = true;
            //    }
            //}
            //else
            //{
            //    rebuild = true;
            //}

            //if (rebuild)
            //{
            //    var evt = DataContext.Events.Where(e => e.Id == id)
            //            .Include("Creator")
            //            .Include("Pictures")
            //            .Include("Pictures.PostedBy")
            //            .Include("Albums")
            //            .Include("Albums.PostedBy")
            //            .Include("Videos")
            //            .Include("Videos.Author")
            //            .Include("Playlists")
            //            .Include("Playlists.Author")
            //            .Include("LinkedFacebookObjects")
            //            .FirstOrDefault();

            //EventHelper.BuildUpdates(evt, MediaTarget.Event, ref lstMedia);
            //Session["Event" + id.ToString() + "Updates"] = new EventUpdates() { Media = lstMedia, Created = DateTime.Now };
            //}
            //updates = (EventUpdates)Session["Event" + id.ToString() + "Updates"];
            //lstMedia = updates.Media;

            //  Test code
            var evt = DataContext.Events.Where(e => e.Id == id).Include("Feeds").FirstOrDefault();
            lstMedia =  evt.Feeds.Select(f => new EventMedia() { Event = evt, MediaUrl = f.Link, Text= f.Message, Title = f.Message, MediaType = MediaType.Comment, MediaDate = DateTime.Today }).ToList();
            //  Test code

            return PartialView("~/Views/Shared/_MediaUpdatesPartial.cshtml", lstMedia);

            //foreach (var p in evt.Pictures)
            //{
            //    lstMedia.Add(new EventMedia() { Id = p.Id, SourceName = p.Title, SourceLink = p.SourceLink, Author = p.PostedBy, MediaDate = p.PhotoDate, MediaType = Enums.MediaType.Picture, PhotoUrl = p.Filename, Title = p.Title, MediaSource = p.MediaSource, Link = p.Filename });
            //}
            //foreach (var v in evt.Videos)
            //{
            //    lstMedia.Add(new EventMedia() { Id = v.Id, SourceName = v.Title, SourceLink = v.VideoUrl, Author = v.Author, MediaDate = v.PublishDate, MediaType = Enums.MediaType.Video, PhotoUrl = v.PhotoUrl, MediaUrl = v.VideoUrl, Title = v.Title, MediaSource = v.MediaSource });
            //}
            //foreach (var lst in evt.Playlists)
            //{
            //    var videos = YouTubeHelper.GetPlaylistVideos(lst.YouTubeId);

            //    foreach (var movie in videos)
            //    {
            //        lstMedia.Add(new EventMedia() { SourceName = movie.Title, SourceLink = movie.VideoLink.ToString(), Author = lst.Author, MediaDate = movie.PubDate, MediaType = Enums.MediaType.Video, PhotoUrl = movie.Thumbnail.ToString(), MediaUrl = movie.VideoLink.ToString(), Title = movie.Title, MediaSource = lst.MediaSource });
            //    }
            //}

            //foreach (var fbob in evt.LinkedFacebookObjects.Where(f => f.MediaSource == MediaSource.Facebook))
            //{
            //    if (evt.Creator != null && evt.Creator.FacebookToken != null)
            //    {
            //        var posts = FacebookHelper.GetFeed(fbob.Id, evt.Creator.FacebookToken);
            //        foreach (var post in posts)
            //        {
            //            if (post.Type == "video")
            //            {
            //                lstMedia.Add(new EventMedia() { SourceName = fbob.Name, SourceLink = fbob.Url, Author = evt.Creator, MediaDate = post.Created_Time, MediaType = Enums.MediaType.Video, PhotoUrl = post.Picture, MediaUrl = post.Source, Text = post.Description, MediaSource = MediaSource.Facebook, Link = post.Source });
            //            }
            //            else if (post.Type == "photo")
            //            {
            //                lstMedia.Add(new EventMedia() { SourceName = fbob.Name, SourceLink = fbob.Url, Author = evt.Creator, MediaDate = post.Created_Time, MediaType = Enums.MediaType.Picture, PhotoUrl = post.Picture, Text = post.Description, MediaSource = MediaSource.Facebook, Link = post.Link });
            //            }
            //            else if (post.Type == "status")
            //            {
            //                lstMedia.Add(new EventMedia() { SourceName = fbob.Name, SourceLink = fbob.Url, Author = evt.Creator, MediaDate = post.Created_Time, MediaType = Enums.MediaType.Comment, Title = post.Name, Text = post.Message, MediaSource = MediaSource.Facebook, Link = post.Source });
            //            }
            //        }
            //    }
            //}
            //  Media Updates

        }

        private EventViewModel LoadEvent(int id, EventType eventType)
        {
            var model = new EventViewModel();
            model.EventType = eventType;
            model.Event = DataContext.Events.Where(x => x.Id == id)
                    .Include("Place")
                    .Include("DanceStyles")
                    .Include("Reviews")
                    .Include("EventMembers")
                    .Include("EventMembers.Member")
                    .Include("EventInstances")
                    .Include("Creator")
                    .FirstOrDefault();

            if (eventType == EventType.Class)
            {
                model.Class = DataContext.Events.OfType<Class>().Where(x => x.Id == id)
                    .Include("Teachers")
                    .Include("Teachers.ApplicationUser")
                    .Include("Owners")
                    .Include("Owners.ApplicationUser")
                    .Include("School")
                    .FirstOrDefault();
                model.ClassTeacherInvitations = DataContext.ClassTeacherInvitations.Where(i => i.ClassId == id)
                                                        .Include("Teacher")
                                                        .Include("Teacher.ApplicationUser")
                                                        .Include("Teacher.ApplicationUser.UserPictures");
            }
            else if (eventType == EventType.Social)
            {
                model.Social = DataContext.Events.OfType<Social>().Where(x => x.Id == id)
                    .Include("Promoters")
                    .Include("Promoters.ApplicationUser")
                    .Include("Owners")
                    .Include("Owners.ApplicationUser")
                    .FirstOrDefault();
            }

            return model;
        }

        [Authorize]
        public ActionResult ApproveTeacher(int teacherId, int classId, string returnUrl)
        {
            var invite = DataContext.ClassTeacherInvitations.Where(i => i.TeacherId == teacherId && i.ClassId == classId).FirstOrDefault();
            invite.Approved = true;
            DataContext.Entry(invite).State = EntityState.Modified;
            var cl = DataContext.Events.OfType<Class>().Where(c => c.Id == classId).Include("Teachers").FirstOrDefault();
            cl.Teachers.Add(DataContext.Teachers.Where(t => t.Id == teacherId).FirstOrDefault());
            DataContext.Entry(cl).State = EntityState.Modified;
            DataContext.SaveChanges();
            ViewBag.Message = "Teacher was approved";
            return Redirect(returnUrl);
        }
        
        #region pictures
        [Authorize]
        public ActionResult ChangeCover(int id, EventType eventType)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var model = new EventChangeCoverViewModel();
            model.EventType = eventType;
            model.Event = DataContext.Events.Where(x => x.Id == id)
                        .Include("Creator")
                        .Include("Pictures")
                        .Include("Pictures.PostedBy")
                        .Include("Videos")
                        .Include("Videos.Author")
                        .Include("Playlists")
                        .Include("Playlists.Author")
                        .Include("LinkedFacebookObjects")
                        .FirstOrDefault();
            var lstMedia = new List<Media>();
            EventHelper.BuildUpdates(model.Event, MediaTarget.Event, ref lstMedia, true);
            model.Media = lstMedia;

            return View(model);
        }
        [Authorize]
        public ActionResult UploadPicture()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public ActionResult UploadPicture(HttpPostedFileBase file, EventChangePictureViewModel model)
        {
            UploadFile newFile = ApplicationUtility.LoadPicture(file);

            if (newFile.UploadStatus == "Success")
            {
                var ev = DataContext.Events.Where(x => x.Id == model.Event.Id).Include("Pictures").FirstOrDefault();
                var today = DateTime.Now;
                var userid = User.Identity.GetUserId();
                var user = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();
                ev.Pictures.Add(new EventPicture() { Title = newFile.FileName, Filename = newFile.FilePath, ThumbnailFilename = newFile.ThumbnailFilePath, PhotoDate = today, PostedBy = user });
                DataContext.Entry(ev).State = EntityState.Modified;
                DataContext.SaveChanges();
                if (ev is Class)
                {
                    return RedirectToAction("View", new { id = model.Event.Id, eventType = EventType.Class });
                }
                else
                {
                    return RedirectToAction("View", new { id = model.Event.Id, eventType = EventType.Social });
                }
            }
            else
            {
                ViewBag.Message = newFile.UploadStatus;
                return View();
            }
        }

        [Authorize]
        public ActionResult DeletePicture(int pictureId, string returnUrl)
        {
            var picture = DataContext.Pictures.Find(pictureId);
            DataContext.Pictures.Remove(picture);
            DataContext.Entry(picture).State = EntityState.Deleted;
            DataContext.SaveChanges();
            ViewBag.Message = ApplicationUtility.DeletePicture(picture);
            return Redirect(returnUrl);
        }

        [Authorize]
        public ActionResult DeleteAlbum(int albumId, string returnUrl)
        {
            var album = DataContext.Albums.Find(albumId);
            DataContext.Albums.Remove(album);
            //  DataContext.Entry(album).State = EntityState.Deleted;
            DataContext.SaveChanges();
            return Redirect(returnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult ChangeCover(ChangePictureViewModel model)
        {
            return View(model);
        }

        [Authorize]
        public ActionResult Cover(int eventId, string photoUrl, string videoUrl, string returnUrl )
        {
            try
            {
                var ev = DataContext.Events.Where(x => x.Id == eventId).Include("Pictures").Include("Pictures.PostedBy").FirstOrDefault();
                ev.PhotoUrl = photoUrl;
                ev.VideoUrl = videoUrl;
                
                DataContext.Entry(ev).State = EntityState.Modified;
                DataContext.SaveChanges();
                return Redirect(returnUrl);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ChangePicture", "Event", new { id = eventId });
            }
        }

        [Authorize]
        public ActionResult AddFacebookPicture(int eventId, string id, string returnUrl)
        {
            try
            {
                string userId = User.Identity.GetUserId();
                var ev = DataContext.Events.Where(e => e.Id == eventId).Include("Pictures").FirstOrDefault();
                var picture = ((List<FacebookPhoto>)Session["FacebookPictures"]).Where(p => p.Id == id).FirstOrDefault();
                var userid = User.Identity.GetUserId();
                var postedby = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();
                ev.Pictures.Add(new EventPicture() { Title = picture.Name, ThumbnailFilename = picture.Source, Filename = picture.LargeSource, PhotoDate = picture.PhotoDate, PostedBy = postedby, MediaSource = MediaSource.Facebook, FacebookId = picture.Id, SourceLink = picture.Link });
                DataContext.Entry(ev).State = EntityState.Modified;
                DataContext.SaveChanges();
                return Redirect(returnUrl);
            }
            catch (Exception ex)
            {
                return Redirect(returnUrl);
            }
        }

        [Authorize]
        [HttpPost]
        public ActionResult AddFacebookPictures(int eventId, string[] photoIds)
        {
            var ev = DataContext.Events.Where(e => e.Id == eventId).Include("Pictures").FirstOrDefault();
            var returnUrl = "";
            if (ev is Class)
            {
                returnUrl = Url.Action("Pictures", "Event", new { id = eventId, eventType = EventType.Class });
            }
            else
            {
                returnUrl = Url.Action("Pictures", "Event", new { id = eventId, eventType = EventType.Social });
            }

            try
            {
                foreach (var id in photoIds)
                {
                    string userId = User.Identity.GetUserId();
                    var picture = ((List<FacebookPhoto>)Session["FacebookPictures"]).Where(p => p.Id == id).FirstOrDefault();
                    var userid = User.Identity.GetUserId();
                    var postedby = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();
                    ev.Pictures.Add(new EventPicture() { Title = picture.Name == null ? "No Title" : picture.Name, ThumbnailFilename = picture.Source, Filename = picture.LargeSource, PhotoDate = picture.PhotoDate, PostedBy = postedby, MediaSource = MediaSource.Facebook, FacebookId = picture.Id, SourceLink = picture.Link });
                }

                DataContext.Entry(ev).State = EntityState.Modified;
                DataContext.SaveChanges();
                return Redirect(returnUrl);
            }
            catch (Exception ex)
            {
                return Redirect(returnUrl);
            }
        }

        [Authorize]
        [HttpPost]
        public ActionResult AddFacebookAlbums(int eventId, string[] albumIds)
        {
            var ev = DataContext.Events.Where(e => e.Id == eventId).Include("Albums").FirstOrDefault();
            var returnUrl = "";
            if (ev is Class)
            {
                returnUrl = Url.Action("Pictures", "Event", new { id = eventId, eventType = EventType.Class });
            }
            else
            {
                returnUrl = Url.Action("Pictures", "Event", new { id = eventId, eventType = EventType.Social });
            }

            try
            {
                foreach (var id in albumIds)
                {
                    string userId = User.Identity.GetUserId();
                    var album = ((List<FacebookAlbum>)Session["FacebookAlbums"]).Where(a => a.Id == id).FirstOrDefault();
                    var userid = User.Identity.GetUserId();
                    var postedby = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();
                    ev.Albums.Add(new EventAlbum() { Name = album.Name == null ? "No Title" : album.Name, CoverThumbnail = album.Thumbnail, CoverPhoto = album.Cover_Photo, AlbumDate = album.Created_Time, PostedBy = postedby, MediaSource = MediaSource.Facebook, FacebookId = album.Id, SourceLink = album.Link, PhotoCount = Convert.ToInt32(album.Count) });
                }

                DataContext.Entry(ev).State = EntityState.Modified;
                DataContext.SaveChanges();
                return Redirect(returnUrl);
            }
            catch (Exception ex)
            {
                return Redirect(returnUrl);
            }
        }

        [Authorize]
        public ActionResult AddInstagramPicture(int eventId, string id, string returnUrl)
        {
            try
            {
                string userId = User.Identity.GetUserId();
                var ev = DataContext.Events.Where(e => e.Id == eventId).Include("Pictures").FirstOrDefault();
                var picture = ((List<InstagramPicture>)Session["InstagramPictures"]).Where(p => p.InstagramId == id).FirstOrDefault();
                var userid = User.Identity.GetUserId();
                var postedby = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();
                ev.Pictures.Add(new EventPicture() { Title = picture.Caption, ThumbnailFilename = picture.Thumbnail, Filename = picture.Photo, PhotoDate = picture.PhotoDate, PostedBy = postedby, MediaSource = MediaSource.Instagram, InstagramId = picture.InstagramId, SourceLink = picture.Link });
                DataContext.Entry(ev).State = EntityState.Modified;
                DataContext.SaveChanges();
                return Redirect(returnUrl);
            }
            catch (Exception ex)
            {
                return Redirect(returnUrl);
            }
        }

        [Authorize]
        [HttpPost]
        public ActionResult AddInstagramPictures(int eventId, string[] pictureids)
        {
            var ev = DataContext.Events.Where(e => e.Id == eventId).Include("Pictures").FirstOrDefault();
            var returnUrl = "";
            if (ev is Class)
            {
                returnUrl = Url.Action("Pictures", "Event", new { id = eventId, eventType = EventType.Class });
            }
            else
            {
                returnUrl = Url.Action("Pictures", "Event", new { id = eventId, eventType = EventType.Social });
            }

            try
            {
                foreach(var id in pictureids)
                {
                    string userId = User.Identity.GetUserId();
                    var picture = ((List<InstagramPicture>)Session["InstagramPictures"]).Where(p => p.InstagramId == id).FirstOrDefault();
                    var userid = User.Identity.GetUserId();
                    var postedby = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();
                    ev.Pictures.Add(new EventPicture() { Title = picture.Caption == null ? "No Title" : picture.Caption, ThumbnailFilename = picture.Thumbnail, Filename = picture.Photo, PhotoDate = picture.PhotoDate, PostedBy = postedby, MediaSource = MediaSource.Instagram, InstagramId = picture.InstagramId, SourceLink = picture.Link });
                }
                DataContext.Entry(ev).State = EntityState.Modified;
                DataContext.SaveChanges();
                return Redirect(returnUrl);
            }
            catch (Exception ex)
            {
                return Redirect(returnUrl);
            }
        }

        [Authorize]
        public ActionResult PostPicture(int id, EventType eventType)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var model = new EventPostPictureViewModel();
            model.Event = DataContext.Events.Where(x => x.Id == id).Include("Place").Include("Pictures").FirstOrDefault();
            model.EventType = eventType;
            //var userid = User.Identity.GetUserId();
            //var token = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault().FacebookToken;
            //if (token != null)
            //{
            //    model.FacebookPictures = FacebookHelper.GetPhotos(token);
            //    Session["FacebookPictures"] = model.FacebookPictures;
            //}

            return View(model);
        }

#endregion

        #region videos
        [Authorize]
        public ActionResult PostVideo(int id, EventType eventType)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var model = new EventPostVideoViewModel();
            model.EventType = eventType;
            model.Event = DataContext.Events.Where(x => x.Id == id)
                                .Include("Place")
                                .Include("Videos")
                                .Include("Videos.Author")
                                .Include("Playlists")
                                .Include("Playlists.Author")
                                .FirstOrDefault();
            var userid = User.Identity.GetUserId();
            var user = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();
            var youtubeUsername = user.YouTubeUsername;
            if (youtubeUsername != null)
            {
                model.YoutubeVideos = YouTubeHelper.GetVideos(youtubeUsername);
                Session["YouTubeVideos"] = model.YoutubeVideos;
                model.YouTubePlaylists = YouTubeHelper.GetPlaylists(youtubeUsername);
                Session["YouTubePlaylists"] = model.YouTubePlaylists;
            }
            var facebookToken = user.FacebookToken;
            if (facebookToken != null)
            {
                model.FacebookVideos = FacebookHelper.GetVideos(facebookToken);
                Session["FacebookVideos"] = model.FacebookVideos;
            }

            return View(model);
        }
        [Authorize]
        public ActionResult ImportYouTubeVideo(string videoId, int eventId, string returnUrl)
        {
            var ytVideo = ((List<YouTubeVideo>)Session["YouTubeVideos"]).Where(x => x.Id == videoId).FirstOrDefault();
            var userid = User.Identity.GetUserId();

            var auth = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();

            var eVideo = new EventVideo() { Title = ytVideo.Title, PublishDate = ytVideo.PubDate, YoutubeId = videoId, Author = auth, VideoUrl = "https://www.youtube.com/watch?v=" + videoId + "&feature=player_embedded", PhotoUrl = "https://img.youtube.com/vi/" + videoId + "/mqdefault.jpg", MediaSource = MediaSource.YouTube };

            var ev = DataContext.Events.Where(e => e.Id == eventId).Include("Videos").FirstOrDefault();
            ev.Videos.Add(eVideo);
            DataContext.Entry(ev).State = EntityState.Modified;
            DataContext.SaveChanges();
            ViewBag.Message = "Video was imported";
            return Redirect(returnUrl);
        }
        [Authorize]
        [HttpPost]
        public ActionResult ImportYouTubeVideos(string[] videoIds, int eventId, string returnUrl)
        {
            var ev = DataContext.Events.Where(e => e.Id == eventId).Include("Videos").FirstOrDefault();
            if (videoIds != null)
            {
                foreach (var videoId in videoIds)
                {
                    var ytVideo = ((List<YouTubeVideo>)Session["YouTubeVideos"]).Where(x => x.Id == videoId).FirstOrDefault();
                    var userid = User.Identity.GetUserId();

                    var auth = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();

                    var eVideo = new EventVideo() { Title = ytVideo.Title, PublishDate = ytVideo.PubDate, YoutubeId = videoId, Author = auth, VideoUrl = "https://www.youtube.com/watch?v=" + videoId + "&feature=player_embedded", PhotoUrl = "https://img.youtube.com/vi/" + videoId + "/mqdefault.jpg", MediaSource = MediaSource.YouTube };

                    ev.Videos.Add(eVideo);

                }
                DataContext.Entry(ev).State = EntityState.Modified;
                DataContext.SaveChanges();
                ViewBag.Message = "Video was imported";
            }

            if (ev is Class)
            {
                return RedirectToAction("Videos", "Event", new { id = eventId, eventType = EventType.Class });
            }
            else
            {
                return RedirectToAction("Videos", "Event", new { id = eventId, eventType = EventType.Social });
            }
        }
        [Authorize]
        public ActionResult ImportFacebookVideo(string videoId, int eventId, string returnUrl)
        {
            var video = ((List<FacebookVideo>)Session["FacebookVideos"]).Where(x => x.Id == videoId).FirstOrDefault();
            var userid = User.Identity.GetUserId();

            var auth = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();

            var eVideo = new EventVideo() { Title = video.Name != null ? video.Name : "No Title", PublishDate = video.Created_Time, FacebookId = videoId, Author = auth, VideoUrl = "https://www.facebook.com/video.php?v=" + videoId, PhotoUrl = video.Picture, MediaSource = MediaSource.Facebook };

            var ev = DataContext.Events.Where(e => e.Id == eventId).Include("Videos").FirstOrDefault();
            ev.Videos.Add(eVideo);
            DataContext.Entry(ev).State = EntityState.Modified;
            DataContext.SaveChanges();
            ViewBag.Message = "Video was imported";
            return Redirect(returnUrl);
        }

        [Authorize]
        [HttpPost]
        public ActionResult ImportFacebookVideos(string[] videoIds, int eventId)
        {
            var ev = DataContext.Events.Where(e => e.Id == eventId).Include("Videos").FirstOrDefault();
            if (videoIds != null)
            {
                foreach (var videoId in videoIds)
                {
                    var video = ((List<FacebookVideo>)Session["FacebookVideos"]).Where(x => x.Id == videoId).FirstOrDefault();
                    var userid = User.Identity.GetUserId();

                    var auth = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();

                    var eVideo = new EventVideo() { Title = video.Name != null ? video.Name : "No Title", PublishDate = video.Created_Time, FacebookId = videoId, Author = auth, VideoUrl = "https://www.facebook.com/video.php?v=" + videoId, PhotoUrl = video.Picture, MediaSource = MediaSource.Facebook };

                    ev.Videos.Add(eVideo);
                }
                DataContext.Entry(ev).State = EntityState.Modified;
                DataContext.SaveChanges();
                ViewBag.Message = "Videos were imported";
            }
            if (ev is Class)
            {
                return RedirectToAction("Videos", "Event", new { id = eventId, eventType = EventType.Class });
            }
            else
            {
                return RedirectToAction("Videos", "Event", new { id = eventId, eventType = EventType.Social });
            }
        }
        
        [Authorize]
        public ActionResult ImportYouTubeList(string Id, int eventId, string returnUrl)
        {
            var ytPlaylist = ((List<YouTubePlaylist>)Session["YouTubePlaylists"]).Where(x => x.Id == Id).FirstOrDefault();
            var userid = User.Identity.GetUserId();
            var auth = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();
            var ev = DataContext.Events.Where(e => e.Id == eventId).Include("Playlists").FirstOrDefault();
            var ePlaylist = new EventPlaylist() { Title = ytPlaylist.Name, PublishDate = ytPlaylist.PubDate, YouTubeId = ytPlaylist.Id, Author = auth, YouTubeUrl = ytPlaylist.Url, CoverPhoto = ytPlaylist.ThumbnailUrl, MediaSource = MediaSource.YouTube };

            ev.Playlists.Add(ePlaylist);
            DataContext.Entry(ev).State = EntityState.Modified;
            DataContext.SaveChanges();
            ViewBag.Message = "Playlist was imported";
            return Redirect(returnUrl);
        }
        [Authorize]
        public ActionResult ImportYouTubePlaylistLink(string playlistUrl, int eventId, string returnUrl)
        {
            var ytPlaylist = YouTubeHelper.GetPlaylist(playlistUrl);
            var userid = User.Identity.GetUserId();
            var auth = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();
            var ev = DataContext.Events.Where(e => e.Id == eventId).Include("Playlists").FirstOrDefault();
            var ePlaylist = new EventPlaylist() { Title = ytPlaylist.Name, PublishDate = ytPlaylist.PubDate, YouTubeId = ytPlaylist.Id, Author = auth, YouTubeUrl = ytPlaylist.Url, CoverPhoto = ytPlaylist.ThumbnailUrl, MediaSource = MediaSource.YouTube };

            ev.Playlists.Add(ePlaylist);
            DataContext.Entry(ev).State = EntityState.Modified;
            DataContext.SaveChanges();
            ViewBag.Message = "Playlist was imported";
            return Redirect(returnUrl);
        }
        [Authorize]
        public ActionResult ImportPlayListVideoLink(string videoUrl, int eventId, string returnUrl)
        {
            var ytVideo = YouTubeHelper.GetVideo(videoUrl);
            var userid = User.Identity.GetUserId();
            var auth = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();
            var ev = DataContext.Events.Where(e => e.Id == eventId).Include("Videos").FirstOrDefault();
            var eVideo = new EventVideo() { Title = ytVideo.Title, PublishDate = ytVideo.PubDate, YoutubeId = ytVideo.Id, Author = auth, VideoUrl = "https://www.youtube.com/watch?v=" + ytVideo.Id + "&feature=player_embedded", PhotoUrl = "https://img.youtube.com/vi/" + ytVideo.Id + "/mqdefault.jpg", MediaSource = MediaSource.YouTube };

            ev.Videos.Add(eVideo);
            DataContext.Entry(ev).State = EntityState.Modified;
            DataContext.SaveChanges();
            ViewBag.Message = "Video was imported";
            return Redirect(returnUrl);
        }

        [Authorize]
        public ActionResult DeleteVideo(int videoId, string returnUrl)
        {
            DataContext.Videos.Remove(DataContext.Videos.Where(v => v.Id == videoId).FirstOrDefault());
            DataContext.SaveChanges();
            ViewBag.Message = "Video was deleted";
            return Redirect(returnUrl);
        }
        [Authorize]
        public ActionResult DeletePlaylist(int listId, string returnUrl)
        {
            DataContext.Playlists.Remove(DataContext.Playlists.Where(l => l.Id == listId).FirstOrDefault());
            DataContext.SaveChanges();
            ViewBag.Message = "Playlist was removed";
            return Redirect(returnUrl);
        }
        #endregion

        [Authorize]
        public ActionResult JoinTeachers(int eventId, string returnUrl)
        {
            var userid = User.Identity.GetUserId();
            var teacher = DataContext.Teachers.Where(t => t.ApplicationUser.Id == userid).FirstOrDefault();
            var cls = DataContext.Events.OfType<Class>().Where(c => c.Id == eventId).FirstOrDefault();
            DataContext.ClassTeacherInvitations.Add(new ClassTeacherInvitation() { Teacher = teacher, Class = cls });
            DataContext.SaveChanges();
            ViewBag.Message = "You requested to join this class as a teacher";
            return Redirect(returnUrl);
        }

        [Authorize]
        public ActionResult Signup(int id, string returnUrl)
        {
            var userId = User.Identity.GetUserId();
            var user = DataContext.Users.Where(x => x.Id == userId).FirstOrDefault();
            var evt = DataContext.Events.Where(e => e.Id == id).FirstOrDefault();
            if (DataContext.EventMembers.Where(x => x.Member.Id == user.Id && x.Event.Id == id).ToList().Count == 0)
            {
                var newMember = new EventMember() { Event = evt, Member = user };
                DataContext.EventMembers.Add(newMember);
            }

            if (evt is Class)
            {
                var teachers = DataContext.Teachers.Where(t => t.Classes.Any(c => c.Id == id)).ToList();
                foreach (Teacher t in teachers)
                {
                    if (DataContext.Students.Where(x => x.DancerId == user.Id && x.TeacherId == t.Id).ToList().Count == 0)
                    {
                        DataContext.Students.Add(new Student() { Teacher = t, Dancer = user });
                    }
                }
            }
            DataContext.SaveChanges();
            return Redirect(returnUrl);
        }

        [Authorize]
        public ActionResult LeaveEvent(int id, string returnUrl)
        {
            var userId = User.Identity.GetUserId();
            var user = DataContext.Users.Where(x => x.Id == userId).FirstOrDefault();
            var evt = DataContext.Events.Where(e => e.Id == id).FirstOrDefault();
            DataContext.EventMembers.Remove(DataContext.EventMembers.Where(m => m.UserId == userId && m.EventId == id).FirstOrDefault());
            DataContext.SaveChanges();
            return Redirect(returnUrl);
        }

        [Authorize(Roles = "Owner,Promoter,Teacher")]
        public ActionResult Create(EventType eventType, int schoolId, RoleName role)
        {
            var model = new EventCreateViewModel(eventType, schoolId, role);
            //model.EventType = eventType;
            //model.SchoolId = schoolId;
            //var userid = User.Identity.GetUserId();
            //var user = DataContext.Users.Where(u => u.Id == userid).Include("Places").FirstOrDefault();
            //  model.Places = new List<PlaceItem>();
            //model.NewPlace = new Place() { Id = 0, Latitude = 0.0, Longitude = 0.0, Public = false, PlaceType = PlaceType.OtherPlace };

            ////  New Event
            //if (model.Event == null)
            //{
            //    if (model.EventType == EventType.Class)
            //    {
            //        model.Event = new Class() { StartDate = DateTime.Today, Place = new Place(), SchoolId = schoolId };
            //    }
            //    else
            //    {
            //        model.Event = new Social() { StartDate = DateTime.Today, Place = new Place() };
            //    }
            //}
            ////  New Event

            LoadCreateModel(model);

            return View(model);
        }

        //[Authorize(Roles = "Owner,Promoter,Teacher")]
        //[HttpPost]
        //public ActionResult CreateFacebookEvent(EventCreateViewModel model)
        //{
        //    var userid = User.Identity.GetUserId();
        //    var user = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();
        //    var f = new FacebookEvent();
        //    if (model.FacebookLink != null)
        //    {
        //        f = FacebookHelper.GetEvent(ParseFacebookLink(model.FacebookLink), user.FacebookToken);
        //    }
        //    else
        //    {
        //        f = FacebookHelper.GetEvent(model.SelectedFacebookEventId, user.FacebookToken);
        //    }
        //    model.Event = new Event() { Description = f.Description, Name = f.Name, StartDate = f.StartTime, StartTime = f.StartTime, FacebookId = f.Id, FacebookLink = f.EventLink, Place = new Place() { Name = f.Address.Location, Address = f.Address.Street, City = f.Address.City, State = f.Address.State != null ? (State)Enum.Parse(typeof(State), f.Address.State) : State.CA, Zip = f.Address.ZipCode, Country = f.Address.Country, Latitude = f.Address.Latitude, Longitude = f.Address.Longitude, FacebookId = f.Address.FacebookId, PlaceType = FacebookHelper.ParsePlaceType(f.Address.Categories), Public = true, Website = f.Address.WebsiteUrl, FacebookLink = f.Address.FacebookUrl, Filename = f.Address.CoverPhotoUrl, ThumbnailFilename = f.Address.ThumbnailUrl } };
        //    model.CreateAction = "Facebook";
        //    return RedirectToAction("Create", model);
        //}

        private void LoadCreateModel(EventCreateViewModel model)
        {
            var userid = User.Identity.GetUserId();
            var user = DataContext.Users.Single(u => u.Id == userid);

            //  Fill Places
            var places = new List<Place>();
            int? placeid = model.Event.Place != null ? (int?)model.Event.Place.Id : null;
            //  Fill Places
            places = DataContext.Places.Where(p => p.Teachers.Any(t => t.ApplicationUser.Id == userid) || p.Owners.Any(o => o.ApplicationUser.Id == userid) || p.Promoters.Any(pr => pr.ApplicationUser.Id == userid) || p.Users.Any(u => u.Id == userid) || p.Id == model.Event.Place.Id).ToList();
            model.Places = new List<PlaceItem>();
            model.Places.Add(new PlaceItem() { Id = 0, Latitude = 0.0, Longitude = 0.0 });
            model.Places.AddRange(DataContext.Places.Where(p => p.Teachers.Any(t => t.ApplicationUser.Id == userid) || p.Owners.Any(o => o.ApplicationUser.Id == userid) || p.Promoters.Any(pr => pr.ApplicationUser.Id == userid) || p.Users.Any(u => u.Id == userid) || p.Id == model.Event.Place.Id).AsEnumerable().Select(p => new PlaceItem() { Address = p.Address, Address2 = p.Address2, City = p.City, Country = p.Country, FacebookId = p.FacebookId, FacebookLink = p.FacebookLink, Filename = p.Filename, Id = p.Id, Latitude = p.Latitude, Longitude = p.Longitude, Name = p.Name, PlaceType = p.PlaceType, State = p.State, ThumbnailFilename = p.ThumbnailFilename, Website = p.Website, Zip = p.Zip, Selected = (model.Event.Place != null && model.Event.Place.Id == p.Id) ? true : false }));
            //  Fill Places

            //  Load Facebook Events
            //  For Dance Styles Checkbox List
            model.StylesCheckboxList.AvailableItems = DataContext.DanceStyles.Select(s => new SelectListItem() { Value = s.Id.ToString(), Text = s.Name }).OrderBy(s => s.Text).ToList();
            //  For Dance Styles Checkbox List

            //  For Month Days Checkbox List
            if (model.Event.MonthDays != null)
            {
                model.MonthDays.SelectedItems = model.Event.MonthDays.Split(new char[] { '-' }).Select(d => new SelectListItem() { Text = d, Value = d }).ToList();
            }
            model.MonthDays.AvailableItems = new List<SelectListItem>() { new SelectListItem() { Value = "1", Text = "1st" }, new SelectListItem() { Value = "2", Text = "2nd" }, new SelectListItem() { Value = "3", Text = "3rd" }, new SelectListItem() { Value = "4", Text = "4th" } };

            //  Set Month day text
            var daysofmonth = new string[] { "blank", "1st", "2nd", "3rd", "4th", "5th", "6th", "7th", "8th", "9th", "10th", "11th", "12th", "13th", "14th", "15th", "16th", "17th", "18th", "19th", "20th", "21st", "22nd", "23rd", "24th", "25th", "26th", "27th", "28th", "29th", "30th", "31st" };
            model.MonthDay = daysofmonth[model.Event.StartDate.Day];
            //  Set Month day text

            //  Set Facebook List
            if (user.FacebookToken != null)
            {
                model.FacebookEvents = FacebookHelper.GetEvents(user.FacebookToken, DateTime.Now).Where(fe => !DataContext.Events.Select(e => e.FacebookId).Contains(fe.Id));
            }
            //  Set Facebook List

            //  Set Tikcets
            model.Tickets = DataContext.Tickets.Where(t => t.SchoolId == model.SchoolId).ToList();
            //  Set Tickets
        }

        [Authorize(Roles = "Owner,Promoter,Teacher")]
        [HttpPost]
        public ActionResult Create(EventCreateViewModel model)
        {
            var userid = User.Identity.GetUserId();
            var user = DataContext.Users.Single(u => u.Id == userid);

            //  Pick a Facebook Event
            if (model.FacebookId != null)
            {
                var f = FacebookHelper.GetEvent(model.FacebookId, user.FacebookToken);
                model.Event = new Event() { Description = f.Description, Name = f.Name, StartDate = f.StartTime, StartTime = f.StartTime, EndDate = f.EndTime, EndTime = f.EndTime, FacebookId = f.Id, FacebookLink = f.EventLink, PhotoUrl = f.CoverPhoto.LargeSource, Place = new Place() { Name = f.Address.Location, Address = f.Address.Street, City = f.Address.City, State = f.Address.State != null && Enum.IsDefined(typeof(State), f.Address.State) ? (State)Enum.Parse(typeof(State), f.Address.State) : State.CA, Zip = f.Address.ZipCode, Country = f.Address.Country, Latitude = f.Address.Latitude, Longitude = f.Address.Longitude, FacebookId = f.Address.FacebookId, PlaceType = FacebookHelper.ParsePlaceType(f.Address.Categories), Public = true, Website = f.Address.WebsiteUrl, FacebookLink = f.Address.FacebookUrl, Filename = f.Address.CoverPhotoUrl, ThumbnailFilename = f.Address.ThumbnailUrl } };
                model.FacebookId = null;

                ModelState.Clear();
                LoadCreateModel(model);
                return View(model);
            }
            //  Pick a Facebook Event

            int id = 0;
            if (model.Event.PlaceId >= 0)
            {
                ModelState["Event.Place.Name"].Errors.Clear();
                ModelState["Event.Place.Address"].Errors.Clear();
                ModelState["Event.Place.City"].Errors.Clear();
                ModelState["Event.Place.State"].Errors.Clear();
                ModelState["Event.Place.Zip"].Errors.Clear();
            }

            if (ModelState.IsValid)
            {
                if (model.Event.PlaceId == 0)
                {
                    var add = Geolocation.ParseAddress(model.Event.Place.Address + ", " + model.Event.Place.City + ", " + model.Event.Place.State + ", " + model.Event.Place.Zip);
                    model.Event.Place.Latitude = add.Latitude;
                    model.Event.Place.Longitude = add.Longitude;
                }

                if (model.EventType == EventType.Class)
                {
                    var cls = new Class();
                    TryUpdateModel(cls, "Event");
                    TryUpdateModel(cls);
                    //  TryUpdateModel(cls.EventInstances, "EventInstances");

                    //  Update Month Days
                    if (model.MonthDays.PostedItems != null)
                    {
                        cls.MonthDays = String.Join("-", model.MonthDays.PostedItems) + (model.HiddenMonthDay != "" ? ("-" + model.HiddenMonthDay) : "");
                    }
                    else
                    {
                        cls.MonthDays = model.HiddenMonthDay;
                    }
                    //  Update Month Days

                    if (model.Role == RoleName.Teacher)
                    {
                        cls.Teachers.Add(DataContext.Teachers.Single(t => t.ApplicationUser.Id == userid));
                    }
                    else
                    {
                        cls.Owners.Add(DataContext.Owners.Single(o => o.ApplicationUser.Id == userid));
                    }
                    cls.Creator = user;
                    if (cls.PlaceId == 0)
                    {
                        cls.Place.Latitude = model.Event.Place.Latitude;
                        cls.Place.Longitude = model.Event.Place.Longitude;
                    }
                    else
                    {
                        cls.Place = null;
                    }

                    cls.DanceStyles = DataContext.DanceStyles.Where(s => model.StylesCheckboxList.PostedItems.Contains(s.Id.ToString())).ToList();

                    //  Add Recurring Events
                    if (model.Event.Recurring)
                    {
                        var sdate = model.Event.StartDate;
                        var edate = model.Event.EndDate;
                        sdate = ApplicationUtility.GetNextDate(sdate, model.Event.Frequency, (int)model.Event.Interval, model.Event.Day, sdate, model.Event.MonthDays);

                        while (sdate <= edate)
                        {
                            cls.EventInstances.Add(new EventInstance() { Event = cls, DateTime = sdate });
                            sdate = ApplicationUtility.GetNextDate(sdate, model.Event.Frequency, (int)model.Event.Interval, model.Event.Day, sdate.AddDays(1), model.Event.MonthDays);
                        }
                    }
                    //  Add Recurring Events

                    DataContext.Classes.Add(cls);
                    DataContext.SaveChanges();
                    id = cls.Id;
                    //  Add Tickets

                    cls.EventTickets = DataContext.Tickets.Where(t => model.TicketId.Contains(t.Id.ToString())).AsEnumerable().Select(t => new EventTicket() { TicketId = t.Id, EventId = cls.Id }).ToList();
                    DataContext.SaveChanges();


                }
                //  Social
                else
                {
                    var soc = new Social();
                    TryUpdateModel(soc, "Event");
                    TryUpdateModel(soc);
                    //  TryUpdateModel(soc.EventInstances, "EventInstances");

                    //  Update Month Days
                    if (model.MonthDays.PostedItems != null)
                    {
                        soc.MonthDays = String.Join("-", model.MonthDays.PostedItems) + (model.HiddenMonthDay != "" ? ("-" + model.HiddenMonthDay) : "");
                    }
                    else
                    {
                        soc.MonthDays = model.HiddenMonthDay;
                    }
                    //  Update Month Days

                    if (model.Role == RoleName.Promoter)
                    {
                        soc.Promoters.Add(DataContext.Promoters.Single(t => t.ApplicationUser.Id == userid));
                    }
                    else
                    {
                        soc.Owners.Add(DataContext.Owners.Single(o => o.ApplicationUser.Id == userid));
                    }
                    soc.Creator = user;
                    if (soc.PlaceId == 0)
                    {
                        soc.Place.Latitude = model.Event.Place.Latitude;
                        soc.Place.Longitude = model.Event.Place.Longitude;
                    }
                    else
                    {
                        soc.Place = null;
                    }
                    soc.DanceStyles = DataContext.DanceStyles.Where(s => model.StylesCheckboxList.PostedItems.Contains(s.Id.ToString())).ToList();

                    //  Add Recurring Events
                    if (model.Event.Recurring)
                    {
                        var sdate = model.Event.StartDate;
                        var edate = model.Event.EndDate;
                        sdate = ApplicationUtility.GetNextDate(sdate, model.Event.Frequency, (int)model.Event.Interval, model.Event.Day, sdate, model.Event.MonthDays);

                        while (sdate <= edate)
                        {
                            soc.EventInstances.Add(new EventInstance() { Event = soc, DateTime = sdate });
                            sdate = ApplicationUtility.GetNextDate(sdate, model.Event.Frequency, (int)model.Event.Interval, model.Event.Day, sdate.AddDays(1), model.Event.MonthDays);
                        }
                    }
                    //  Add Recurring Events

                    DataContext.Socials.Add(soc);
                    DataContext.SaveChanges();
                    id = soc.Id;
                    //  Add Tickets
                    soc.EventTickets = DataContext.Tickets.Where(t => model.TicketId.Contains(t.Id.ToString())).AsEnumerable().Select(t => new EventTicket() { TicketId = t.Id, EventId = soc.Id }).ToList();
                    DataContext.SaveChanges();
                }
            }
            else
            {
                LoadCreateModel(model);

                return View(model);
            }

            return RedirectToAction("View", new { id = id, eventType = model.EventType });
        }

        [Authorize(Roles = "Owner,Promoter,Teacher")]
        [HttpPost]
        public ActionResult ImportFromFacebook(EventCreateViewModel model)
        {
            var userid = User.Identity.GetUserId();
            var user = DataContext.Users.Single(u => u.Id == userid);

            if (model.FacebookId != null)
            {
                var fbevent = FacebookHelper.GetEvent(model.FacebookId, user.FacebookToken, null);
                model.Event = new Event() { Name = fbevent.Name, Description = fbevent.Description, StartDate = fbevent.StartTime, StartTime = fbevent.StartTime, EndDate = fbevent.EndTime, EndTime = fbevent.EndTime, PhotoUrl = fbevent.CoverPhoto.LargeSource, FacebookId = fbevent.Id, FacebookLink = fbevent.EventLink, Interval = 1, IsAvailable = true, UpdatedDate = fbevent.Updated };
            }

            return RedirectToAction("Create", model);
        }

        [Authorize]
        public ActionResult Edit(int? id, EventType eventType, int? placeId)
        {
            var model = new EventEditViewModel();
            model.EventType = eventType;

            if (id != null)
            {
                var ev = DataContext.Events.Where(e => e.Id == id).Include("Creator").FirstOrDefault();

                if (ev is Class)
                {
                    var cls = DataContext.Events.OfType<Class>().Where(e => e.Id == id).Include("DanceStyles").Include("Place").FirstOrDefault();
                    model.Event = cls;
                    model.ClassType = cls.ClassType;
                    model.SkillLevel = cls.SkillLevel;
                }
                else
                {
                    var soc = DataContext.Events.OfType<Social>().Where(e => e.Id == id).Include("DanceStyles").Include("Place").FirstOrDefault();
                    model.Event = soc;
                    model.SocialType = soc.SocialType;
                }

                //  Update Facebook Details
                if (ev.FacebookId != null)
                {
                    var fbev = FacebookHelper.GetEvent(ev.FacebookId, ev.Creator.FacebookToken, "id,cover,description,end_time,is_date_only,location,name,owner,privacy,start_time,ticket_uri,timezone,updated_time,venue,parent_group");
                    model.Event.Name = fbev.Name;
                    model.Event.Description = fbev.Description;
                    model.Event.PhotoUrl = fbev.CoverPhoto.LargeSource;
                    model.Event.StartDate = fbev.StartTime;
                    model.Event.StartTime = fbev.StartTime;
                    model.Event.EndTime = fbev.EndTime;
                }

                //  Update Facebook Details

            }

            BuildEditModel(model);

            return View(model);
        }

        //[Authorize(Roles = "Owner,Promoter,Teacher")]
        //[HttpPost]
        //public ActionResult Create(EventCreateViewModel model)
        //{
        //    ModelState["Event.Place.Id"].Errors.Clear();
        //    ModelState["Event.Place.Name"].Errors.Clear();
        //    ModelState["Event.Place.Address"].Errors.Clear();
        //    ModelState["Event.Place.City"].Errors.Clear();
        //    ModelState["Event.Place.State"].Errors.Clear();
        //    ModelState["Event.Place.Zip"].Errors.Clear();

        //    if (model.Event.Place.Id != 0)
        //    {
        //        ModelState["NewPlace.Name"].Errors.Clear();
        //        ModelState["NewPlace.Address"].Errors.Clear();
        //        ModelState["NewPlace.City"].Errors.Clear();
        //        ModelState["NewPlace.State"].Errors.Clear();
        //        ModelState["NewPlace.Zip"].Errors.Clear();
        //    }

        //    //  Remove Validation for Socials
        //    if (model.EventType == EventType.Social)
        //    {
        //        ModelState["SkillLevel"].Errors.Clear();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        var userid = User.Identity.GetUserId();
        //        var user = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();

        //        //  New Event
        //        var evt = new Event();
        //        if (model.Event.Id == 0)
        //        {
        //            if (model.EventType == EventType.Class)
        //            {
        //                var cls = new Class();
        //                TryUpdateModel(cls);
        //                cls.ClassType = model.ClassType;
        //                cls.Teachers = new List<Teacher>();
        //                cls.Owners = new List<Owner>();

        //                if (model.Role == RoleName.Teacher)
        //                {
        //                    cls.Teachers.Add(DataContext.Teachers.Single(s => s.ApplicationUser.Id == userid));
        //                }
        //                if (model.Role == RoleName.Owner)
        //                {
        //                    cls.Owners.Add(DataContext.Owners.Single(s => s.ApplicationUser.Id == userid));
        //                }
        //                cls.SkillLevel = (int)model.SkillLevel;
        //                //  var cls = new Class() { ClassType = model.ClassType, Teachers = new List<Teacher>(), Owners = new List<Owner>(), Creator = user, Recurring = model.Event.Recurring, Interval };

        //                evt = cls;
        //            }
        //            else
        //            {
        //                var soc = new Social();
        //                TryUpdateModel(soc);
        //                soc.SocialType = model.SocialType;
        //                soc.Promoters = new List<Promoter>();
        //                soc.Owners = new List<Owner>();

        //                if (model.Role == RoleName.Promoter)
        //                {
        //                    soc.Promoters.Add(DataContext.Promoters.Single(s => s.ApplicationUser.Id == userid));
        //                }
        //                if (model.Role == RoleName.Owner)
        //                {
        //                    soc.Owners.Add(DataContext.Owners.Single(s => s.ApplicationUser.Id == userid));
        //                }

        //                evt = soc;
        //            }
        //            evt.DanceStyles = new List<DanceStyle>();
        //        }
        //        //evt.Name = model.Event.Name;
        //        //evt.StartDate = model.Event.StartDate;
        //        //evt.AllDay = model.Event.AllDay;
        //        //evt.StartTime = model.Event.StartTime;
        //        //evt.EndTime = model.Event.EndTime;
        //        //evt.EndDate = model.Event.EndDate;
        //        //evt.Description = model.Event.Description;
        //        //evt.Recurring = model.Event.Recurring;
        //        //evt.Frequency = model.Event.Frequency;
        //        //evt.Interval = model.Event.Interval;


        //        var place = new Place();
        //        if (model.Event.Place.Id == 0)
        //        {
        //            var address = Geolocation.ParseAddress(model.NewPlace.Address + " " + model.NewPlace.City + " " + model.NewPlace.State + " " + model.NewPlace.Zip);
        //            place = (Place)model.NewPlace;
        //            place.Latitude = address.Latitude;
        //            place.Longitude = address.Longitude;
        //            if (model.AddtoMyPlaces)
        //            {
        //                user.Places.Add(place);
        //                DataContext.Entry(user).State = EntityState.Modified;
        //            }
        //        }
        //        else
        //        {
        //            place = DataContext.Places.Where(p => p.Id == model.Event.Place.Id).FirstOrDefault();
        //        }


        //        //  Remove old place
        //        var oldplace = evt.Place;
        //        evt.Place = place;
        //        if (oldplace != null && oldplace.Id != place.Id && !oldplace.Public && oldplace.PlaceType == PlaceType.OtherPlace && user.Places.Where(p => p.Id == oldplace.Id).Count() == 0)
        //        {
        //            DataContext.Places.Remove(oldplace);
        //        }
        //        //  Remove old place

        //        if (model.PostedMonthDays != null)
        //        {
        //            evt.MonthDays = String.Join("-", model.PostedMonthDays) + (model.HiddenMonthDay != "" ? ("-" + model.HiddenMonthDay) : "");
        //        }
        //        else
        //        {
        //            evt.MonthDays = model.HiddenMonthDay;
        //        }
        //        //  Dance Styles
        //        evt.DanceStyles.Clear();
        //        var styles = DataContext.DanceStyles.Where(s => model.PostedStyles.DanceStyleIds.Contains(s.Id.ToString()));
        //        evt.DanceStyles = styles.ToList();
        //        //  Dance Styles

        //        DataContext.Events.Add(evt);
        //        DataContext.SaveChanges();
        //        model.Event.Id = evt.Id;

        //        return RedirectToAction("View", "Event", new { id = model.Event.Id, eventType = model.EventType });
        //    }
        //    else
        //    {
        //        return View(model);
        //    }
        //}

        [Authorize]
        [HttpPost]
        public ActionResult Edit(EventEditViewModel model)
        {
            ModelState["Event.Place.Id"].Errors.Clear();
            ModelState["Event.Place.Name"].Errors.Clear();
            ModelState["Event.Place.Address"].Errors.Clear();
            ModelState["Event.Place.City"].Errors.Clear();
            ModelState["Event.Place.State"].Errors.Clear();
            ModelState["Event.Place.Zip"].Errors.Clear();

            if (model.Event.Place.Id != 0)
            {
                ModelState["NewPlace.Name"].Errors.Clear();
                ModelState["NewPlace.Address"].Errors.Clear();
                ModelState["NewPlace.City"].Errors.Clear();
                ModelState["NewPlace.State"].Errors.Clear();
                ModelState["NewPlace.Zip"].Errors.Clear();
            }

            //  Remove Validation for Socials
            if (model.EventType == EventType.Social)
            {
                ModelState["SkillLevel"].Errors.Clear();
            }

            if (ModelState.IsValid)
            {
                var userid = User.Identity.GetUserId();
                var user = DataContext.Users.Where(u => u.Id == userid).Include("Places").FirstOrDefault();

                //  New Event
                var evt = new Event();
                if (model.Event.Id == 0)
                {
                    if (model.EventType == EventType.Class)
                    {
                        var cls = new Class() { ClassType = model.ClassType, Teachers = new List<Teacher>(), Owners = new List<Owner>(), Creator = user };

                        if (DataContext.Teachers.Where(t => t.ApplicationUser.Id == userid).Count() == 1)
                        {
                            cls.Teachers.Add(DataContext.Teachers.Where(t => t.ApplicationUser.Id == userid).FirstOrDefault());
                        }
                        else if (DataContext.Owners.Where(t => t.ApplicationUser.Id == userid).Count() == 1)
                        {
                            cls.Owners.Add(DataContext.Owners.Where(t => t.ApplicationUser.Id == userid).FirstOrDefault());
                        }
                        evt = cls;
                    }
                    else
                    {
                        var soc = new Social() { SocialType = model.SocialType, Promoters = new List<Promoter>(), Owners = new List<Owner>(), Creator = user };
                        if (DataContext.Promoters.Where(t => t.ApplicationUser.Id == userid).Count() == 1)
                        {
                            soc.Promoters.Add(DataContext.Promoters.Where(t => t.ApplicationUser.Id == userid).FirstOrDefault());
                        }
                        else if (DataContext.Owners.Where(t => t.ApplicationUser.Id == userid).Count() == 1)
                        {
                            soc.Owners.Add(DataContext.Owners.Where(t => t.ApplicationUser.Id == userid).FirstOrDefault());
                        }
                        evt = soc;
                    }
                    evt.DanceStyles = new List<DanceStyle>();
                }
                //  Existing Event
                else
                {
                    evt = new Event();

                    if (model.EventType == EventType.Class)
                    {
                        var cls = DataContext.Events.OfType<Class>().Where(c => c.Id == model.Event.Id).Include("DanceStyles").FirstOrDefault();
                        cls.ClassType = model.ClassType;
                        evt = cls;
                    }
                    else
                    {
                        var soc = DataContext.Events.OfType<Social>().Where(c => c.Id == model.Event.Id).Include("DanceStyles").FirstOrDefault();
                        soc.SocialType = model.SocialType;
                        evt = soc;
                    }
                }
                evt.Name = model.Event.Name;
                evt.StartDate = model.Event.StartDate;
                evt.AllDay = model.Event.AllDay;
                evt.StartTime = model.Event.StartTime;
                evt.EndTime = model.Event.EndTime;
                evt.EndDate = model.Event.EndDate;
                evt.Description = model.Event.Description;
                evt.Recurring = model.Event.Recurring;
                evt.Frequency = model.Event.Frequency;
                evt.Interval = model.Event.Interval;
                if (model.EventType == EventType.Class)
                {
                    ((Class)evt).SkillLevel = (int)model.SkillLevel;
                }


                var place = new Place();
                if (model.Event.Place.Id == 0)
                {
                    var address = Geolocation.ParseAddress(model.NewPlace.Address + " " + model.NewPlace.City + " " + model.NewPlace.State + " " + model.NewPlace.Zip);
                    place = (Place)model.NewPlace;
                    place.Latitude = address.Latitude;
                    place.Longitude = address.Longitude;
                    if (model.AddtoMyPlaces)
                    {
                        user.Places.Add(place);
                        DataContext.Entry(user).State = EntityState.Modified;
                    }
                }
                else
                {
                    place = DataContext.Places.Where(p => p.Id == model.Event.Place.Id).FirstOrDefault();
                }


                //  Remove old place
                var oldplace = evt.Place;
                evt.Place = place;
                if (oldplace != null && oldplace.Id != place.Id && !oldplace.Public && oldplace.PlaceType == PlaceType.OtherPlace && user.Places.Where(p => p.Id == oldplace.Id).Count() == 0)
                {
                    DataContext.Places.Remove(oldplace);
                }
                //  Remove old place

                if (model.PostedMonthDays != null)
                {
                    evt.MonthDays = String.Join("-", model.PostedMonthDays) + (model.HiddenMonthDay != "" ? ("-" + model.HiddenMonthDay) : "");
                }
                else
                {
                    evt.MonthDays = model.HiddenMonthDay;
                }
                //  Dance Styles
                evt.DanceStyles.Clear();
                var styles = DataContext.DanceStyles.Where(s => model.PostedStyles.DanceStyleIds.Contains(s.Id.ToString()));
                evt.DanceStyles = styles.ToList();
                //  Dance Styles

                if (model.Event.Id == 0)
                {
                    DataContext.Events.Add(evt);
                    DataContext.SaveChanges();
                    model.Event.Id = evt.Id;
                }
                else
                {
                    DataContext.Entry(evt).State = EntityState.Modified;
                    DataContext.SaveChanges();
                }

                return RedirectToAction("View", "Event", new { id = model.Event.Id, eventType = model.EventType });
            }
            else
            {
                BuildEditModel(model);
                return View(model);
            }
            //catch (DbEntityValidationException e)
            //{
            //    var msg = "";
            //    foreach (var eve in e.EntityValidationErrors)
            //    {
            //        msg = eve.Entry.Entity.GetType().Name + " " + eve.Entry.State;
            //        foreach (var ve in eve.ValidationErrors)
            //        {
            //            msg = ve.PropertyName + " " + ve.ErrorMessage;
            //        }
            //    }
            //    return View();
            //}
        }

        private void BuildEditModel(EventEditViewModel model)
        {
            var userid = User.Identity.GetUserId();
            var user = DataContext.Users.Where(u => u.Id == userid).Include("CurrentRole").Include("Places").FirstOrDefault();
            model.User = user;
            model.Places = new List<PlaceItem>();
            model.NewPlace = new Place() { Id = 0, Latitude = 0.0, Longitude = 0.0, Public = false, PlaceType = PlaceType.OtherPlace };

            var places = new List<Place>();
            //  Fill Places

            //  New Event
            if (model.Event == null)
            {
                if (model.EventType == EventType.Class)
                {
                    model.Event = new Class() { StartDate = DateTime.Today, Place = new Place() };
                }
                else
                {
                    model.Event = new Social() { StartDate = DateTime.Today, Place = new Place() };
                }
            }
            //  New Event

            int? placeid = model.Event.Place != null ? (int?)model.Event.Place.Id : null;
            //  Fill Places
            places = DataContext.Places.Where(p => p.Teachers.Any(t => t.ApplicationUser.Id == userid) || p.Owners.Any(o => o.ApplicationUser.Id == userid) || p.Promoters.Any(pr => pr.ApplicationUser.Id == userid) || p.Users.Any(u => u.Id == userid) || p.Id == model.Event.Place.Id).ToList();
            //if (user.CurrentRole != null)
            //{
            //    if (model.EventType == EventType.Class)
            //    {
            //        if (user.CurrentRole.Name == "Teacher")
            //        {
            //            places = DataContext.Places.Where(p => p.Teachers.Any(t => t.ApplicationUser.Id == userid) || p.Users.Any(u => u.Id == userid) || p.Id == placeid).ToList();
            //        }
            //        else if (user.CurrentRole.Name == "Owner")
            //        {
            //            places = DataContext.Places.Where(p => p.Owners.Any(t => t.ApplicationUser.Id == userid) || p.Users.Any(u => u.Id == userid) || p.Id == placeid).ToList();
            //        }
            //    }
            //    else
            //    {
            //        if (user.CurrentRole.Name == "Promoter")
            //        {
            //            places = DataContext.Places.Where(p => p.Promoters.Any(t => t.ApplicationUser.Id == userid) || p.Users.Any(u => u.Id == userid) || p.Id == placeid).ToList();
            //        }
            //        else if (user.CurrentRole.Name == "Owner")
            //        {
            //            places = DataContext.Places.Where(p => p.Owners.Any(t => t.ApplicationUser.Id == userid) || p.Users.Any(u => u.Id == userid) || p.Id == placeid).ToList();
            //        }
            //    }
            //}
            //else
            //{
            //    places = user.Places.ToList();
            //}

            model.Places = new List<PlaceItem>();
            model.Places.Add(new PlaceItem() { Id = 0, Latitude = 0.0, Longitude = 0.0 });

            foreach (var pl in places)
            {
                model.Places.Add(new PlaceItem() { Address = pl.Address, Address2 = pl.Address2, City = pl.City, Country = pl.Country, FacebookId = pl.FacebookId, FacebookLink = pl.FacebookLink, Filename = pl.Filename, Id = pl.Id, Latitude = pl.Latitude, Longitude = pl.Longitude, Name = pl.Name, PlaceType = pl.PlaceType, State = pl.State, ThumbnailFilename = pl.ThumbnailFilename, Website = pl.Website, Zip = pl.Zip, Selected = (model.Event.Place != null && model.Event.Place.Id == pl.Id) ? true : false });
            }
            //  Fill Places

            //  For Dance Styles Checkbox List
            var styles = new List<DanceStyleListItem>();
            foreach (DanceStyle s in DataContext.DanceStyles)
            {
                styles.Add(new DanceStyleListItem { Id = s.Id, Name = s.Name });
            }
            model.AvailableStyles = styles.OrderBy(x => x.Name);

            var selectedStyles = new List<DanceStyleListItem>();
            if (model.PostedStyles != null)
            {
                model.SelectedStyles = model.AvailableStyles.Where(s => model.PostedStyles.DanceStyleIds.Contains(s.Id.ToString()));
            }
            else
            {
                if (model.Event.DanceStyles != null)
                {
                    foreach (DanceStyle ss in model.Event.DanceStyles)
                    {
                        selectedStyles.Add(new DanceStyleListItem { Id = ss.Id, Name = ss.Name });
                    }
                }
                model.SelectedStyles = selectedStyles;
            }
            //  For Dance Styles Checkbox List

            //  For Month Days Checkbox List
            var selectedMonthDays = new List<SelectListItem>();
            string[] daysarray;
            model.SelectedMonthDays = new List<SelectListItem>();
            if (model.Event.MonthDays != null)
            {
                daysarray = model.Event.MonthDays.Split(new char[] { '-' });
                foreach (var day in daysarray)
                {
                    model.SelectedMonthDays.Add(new SelectListItem() { Value = day, Text = day });
                }
            }
            model.MonthDays = new List<SelectListItem>() { new SelectListItem() { Value = "1", Text = "1st" }, new SelectListItem() { Value = "2", Text = "2nd" }, new SelectListItem() { Value = "3", Text = "3rd" }, new SelectListItem() { Value = "4", Text = "4th" } };
            //  For Month Days Checkbox List

            //  Set Month day text
            var daysofmonth = new string[] { "blank", "1st", "2nd", "3rd", "4th", "5th", "6th", "7th", "8th", "9th", "10th", "11th", "12th", "13th", "14th", "15th", "16th", "17th", "18th", "19th", "20th", "21st", "22nd", "23rd", "24th", "25th", "26th", "27th", "28th", "29th", "30th", "31st" };
            model.MonthDay = daysofmonth[model.Event.StartDate.Day];
            //  Set Month day text
        }

        private EventEditViewModel BuildEditModel(int schoolId)
        {
            var model = new EventEditViewModel();
            model.Places = new List<PlaceItem>();
            model.NewPlace = new Place() { Id = 0, Latitude = 0.0, Longitude = 0.0, Public = false, PlaceType = PlaceType.OtherPlace };
            var userid = User.Identity.GetUserId();

            //  Fill Places
            var places = new List<Place>();
            int? placeid = model.Event.Place != null ? (int?)model.Event.Place.Id : null;
            //  Fill Places
            places = DataContext.Places.Where(p => p.Teachers.Any(t => t.ApplicationUser.Id == userid) || p.Owners.Any(o => o.ApplicationUser.Id == userid) || p.Promoters.Any(pr => pr.ApplicationUser.Id == userid) || p.Users.Any(u => u.Id == userid) || p.Id == model.Event.Place.Id).ToList();

            model.Places = new List<PlaceItem>();
            model.Places.Add(new PlaceItem() { Id = 0, Latitude = 0.0, Longitude = 0.0 });

            foreach (var pl in places)
            {
                model.Places.Add(new PlaceItem() { Address = pl.Address, Address2 = pl.Address2, City = pl.City, Country = pl.Country, FacebookId = pl.FacebookId, FacebookLink = pl.FacebookLink, Filename = pl.Filename, Id = pl.Id, Latitude = pl.Latitude, Longitude = pl.Longitude, Name = pl.Name, PlaceType = pl.PlaceType, State = pl.State, ThumbnailFilename = pl.ThumbnailFilename, Website = pl.Website, Zip = pl.Zip, Selected = (model.Event.Place != null && model.Event.Place.Id == pl.Id) ? true : false });
            }
            //  Fill Places

            //  For Dance Styles Checkbox List
            var styles = new List<DanceStyleListItem>();
            foreach (DanceStyle s in DataContext.DanceStyles)
            {
                styles.Add(new DanceStyleListItem { Id = s.Id, Name = s.Name });
            }
            model.AvailableStyles = styles.OrderBy(x => x.Name);

            var selectedStyles = new List<DanceStyleListItem>();
            if (model.PostedStyles != null)
            {
                model.SelectedStyles = model.AvailableStyles.Where(s => model.PostedStyles.DanceStyleIds.Contains(s.Id.ToString()));
            }
            else
            {
                if (model.Event.DanceStyles != null)
                {
                    foreach (DanceStyle ss in model.Event.DanceStyles)
                    {
                        selectedStyles.Add(new DanceStyleListItem { Id = ss.Id, Name = ss.Name });
                    }
                }
                model.SelectedStyles = selectedStyles;
            }
            //  For Dance Styles Checkbox List

            //  For Month Days Checkbox List
            var selectedMonthDays = new List<SelectListItem>();
            string[] daysarray;
            model.SelectedMonthDays = new List<SelectListItem>();
            if (model.Event.MonthDays != null)
            {
                daysarray = model.Event.MonthDays.Split(new char[] { '-' });
                foreach (var day in daysarray)
                {
                    model.SelectedMonthDays.Add(new SelectListItem() { Value = day, Text = day });
                }
            }
            model.MonthDays = new List<SelectListItem>() { new SelectListItem() { Value = "1", Text = "1st" }, new SelectListItem() { Value = "2", Text = "2nd" }, new SelectListItem() { Value = "3", Text = "3rd" }, new SelectListItem() { Value = "4", Text = "4th" } };
            //  For Month Days Checkbox List

            //  Set Month day text
            var daysofmonth = new string[] { "blank", "1st", "2nd", "3rd", "4th", "5th", "6th", "7th", "8th", "9th", "10th", "11th", "12th", "13th", "14th", "15th", "16th", "17th", "18th", "19th", "20th", "21st", "22nd", "23rd", "24th", "25th", "26th", "27th", "28th", "29th", "30th", "31st" };
            model.MonthDay = daysofmonth[model.Event.StartDate.Day];
            //  Set Month day text

            return model;
        }

        [Authorize]
        public ActionResult Delete(int id)
        {
            var evt = DataContext.Events.Where(e => e.Id == id)
                    .Include("Videos")
                    .Include("Pictures")
                    .Include("Playlists")
                    .Include("Reviews")
                    .Include("LinkedFacebookObjects")
                    .Include("EventMembers")
                    .Include("Feeds")
                    .Include("EventTickets")
                    .Include("EventInstances")
                    .FirstOrDefault();

            evt.Videos.Clear();
            evt.Pictures.Clear();
            evt.Playlists.Clear();
            DataContext.Reviews.RemoveRange(evt.Reviews);
            DataContext.LinkedFacebookObjects.RemoveRange(evt.LinkedFacebookObjects);
            DataContext.EventFeeds.RemoveRange(evt.Feeds);
            DataContext.EventMembers.RemoveRange(evt.EventMembers);
            DataContext.EventTickets.RemoveRange(evt.EventTickets);
            DataContext.EventInstances.RemoveRange(evt.EventInstances);
            DataContext.Events.Remove(evt);
            DataContext.SaveChanges();

            var userid = User.Identity.GetUserId();
            var user = UserManager.FindById(userid);

            //  Set Return
            if (user.CurrentRole != null)
            {
                if (user.CurrentRole.Name == "Owner")
                {
                    return Redirect(Url.Action("Home", "Owner", new { username = User.Identity.Name }));
                }
                else if (user.CurrentRole.Name == "Promoter")
                {
                    return Redirect(Url.Action("Home", "Promoter", new { username = User.Identity.Name }));
                }
                else if (user.CurrentRole.Name == "Teacher")
                {
                    return Redirect(Url.Action("Home", "Teacher", new { username = User.Identity.Name }));
                }
                else
                {
                    return Redirect(Url.Action("Home", "Dancer", new { username = User.Identity.Name }));
                }
            }
            else
            {
                return Redirect(Url.Action("Home", "Dancer", new { username = User.Identity.Name }));
            }
            //  Set Return
        }

        [Authorize]
        public ActionResult ImportFacebookEvent()
        {
            var userid = User.Identity.GetUserId();
            var user = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();

            if (user.FacebookToken != null)
            {
                var model = new ImportFacebookEventViewModel();

                model.FacebookEvents = FacebookHelper.GetEvents(user.FacebookToken, DateTime.Now).Where(fe => !DataContext.Events.Select(e => e.FacebookId).Contains(fe.Id)).ToList();

                return View(model);
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
        }

        protected string ParseFacebookLink(string facebookLink)
        {
            Uri fbUri = new Uri(facebookLink);

            var id = "";
            var val = "";
            foreach (var s in fbUri.Segments)
            {
                val = s.Replace("/", "").Replace("events", "").Replace("pages", "").Replace("groups", "");
                if (val != "")
                {
                    id = val;
                }
            }

            return id;
        }

        [Authorize]
        [HttpPost]
        public ActionResult ImportFacebookEvent(ImportFacebookEventViewModel model)
        {
            var userid = User.Identity.GetUserId();
            var user = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();

            if (user.FacebookToken != null)
            {
                if (model.FacebookLink != null)
                {
                    var id = ParseFacebookLink(model.FacebookLink);

                    var evt = DataContext.Events.Where(e => e.FacebookId == id).FirstOrDefault();
                    if (evt == null)
                    {
                        return RedirectToAction("ConfirmFacebookEvent", "Event", new { id = id, eventType = model.Type });
                    }
                    else
                    {
                        if (evt is Class)
                        {
                            return RedirectToAction("View", "Event", new { id = evt.Id, eventType = EventType.Class });
                        }
                        else
                        {
                            return RedirectToAction("View", "Event", new { id = evt.Id, eventType = EventType.Social });
                        }
                    }
                }
                else
                {
                    return View(model);
                }
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
        }

        //[Authorize(Roles = "Teacher,Owner")]
        //public ActionResult ImportClassFromFacebook(int? placeId)
        //{
        //    if (User.Identity.IsAuthenticated)
        //    {
        //        var userid = User.Identity.GetUserId();
        //        var user = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();

        //        if (placeId != null)
        //        {
        //            if (DataContext.Places.Where(p => p.Id == placeId).Where(p => p.Teachers.Any(t => t.ApplicationUser.Id == userid) || p.Owners.Any(o => o.ApplicationUser.Id == userid)).Count() == 0)
        //            {
        //                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //            }
        //        }

        //        var model = new ImportFacebookEventViewModel();
        //        model.User = user;
        //        model.EventType = EventType.Class;

        //        if (placeId != null)
        //        {
        //            model.PlaceId = (int)placeId;
        //        }

        //        if (user.FacebookToken != null)
        //        {
        //            if (Session["FacebookEvents"] == null)
        //            {
        //                Session["FacebookEvents"] = FacebookHelper.GetEvents(user.FacebookToken);
        //            }
        //            if (Session["FacebookEvents"] != null)
        //            {
        //                model.FacebookEvents = (List<FacebookEvent>)Session["FacebookEvents"];
        //            }
        //        }

        //        return View("ImportFacebookEvent", model);
        //    }
        //    else
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //}

        //[Authorize(Roles = "Owner,Promoter")]
        //public ActionResult ImportSocialFromFacebook(int? placeId)
        //{
        //    if (User.Identity.IsAuthenticated)
        //    {
        //        var userid = User.Identity.GetUserId();
        //        var user = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();

        //        if (placeId != null)
        //        {
        //            if (DataContext.Places.Where(p => p.Id == placeId).Where(p => p.Promoters.Any(t => t.ApplicationUser.Id == userid) || p.Owners.Any(o => o.ApplicationUser.Id == userid)).Count() == 0)
        //            {
        //                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //            }
        //        }

        //        var model = new ImportFacebookEventViewModel();
        //        model.User = user;
        //        model.EventType = EventType.Social;

        //        if (placeId != null)
        //        {
        //            model.PlaceId = (int)placeId;
        //        }

        //        if (user.FacebookToken != null)
        //        {
        //            if (Session["FacebookEvents"] == null)
        //            {
        //                Session["FacebookEvents"] = FacebookHelper.GetEvents(user.FacebookToken);
        //            }
        //            if (Session["FacebookEvents"] != null)
        //            {
        //                model.FacebookEvents = (List<FacebookEvent>)Session["FacebookEvents"];
        //            }
        //        }

        //        return View("ImportFacebookEvent", model);
        //    }
        //    else
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //}

        [Authorize(Roles = "Teacher,Owner")]
        public ActionResult CreateClass(int? placeId)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Edit", new { eventType = EventType.Class, placeId = placeId });
                //var userid = User.Identity.GetUserId();
                //var user = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();

                //if (placeId != null)
                //{
                //    if (DataContext.Places.Where(p => p.Id == placeId).Where(p => p.Teachers.Any(t => t.ApplicationUser.Id == userid) || p.Owners.Any(o => o.ApplicationUser.Id == userid)).Count() == 0)
                //    {
                //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                //    }
                //}
                
                //var model = new EventCreateViewModel();
                //model.EventType = EventType.Class;

                //LoadCreateModel(model);

                //if (placeId != null)
                //{
                //    model.PlaceId = (int)placeId;
                //}

                //if (user.FacebookToken != null)
                //{
                //    if (Session["FacebookEvents"] == null)
                //    {
                //        Session["FacebookEvents"] = FacebookHelper.GetEvents(user.FacebookToken);
                //    }
                //    if (Session["FacebookEvents"] != null)
                //    {
                //        model.FacebookEvents = (List<FacebookEvent>)Session["FacebookEvents"];
                //    }
                //}

                //return View("Create", model);
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
        }

        [Authorize(Roles = "Owner,Promoter")]
        public ActionResult CreateSocial(int? placeId)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Edit", new { eventType = EventType.Social, placeId = placeId });

                //var userid = User.Identity.GetUserId();
                //var user = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();

                //if (placeId != null)
                //{
                //    if (DataContext.Places.Where(p => p.Id == placeId).Where(p => p.Teachers.Any(t => t.ApplicationUser.Id == userid) || p.Owners.Any(o => o.ApplicationUser.Id == userid)).Count() == 0)
                //    {
                //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                //    }
                //}

                //var model = new EventCreateViewModel();
                //model.EventType = EventType.Social;

                //LoadCreateModel(model);

                //if (placeId != null)
                //{
                //    model.PlaceId = (int)placeId;
                //}

                //if (user.FacebookToken != null)
                //{
                //    if (Session["FacebookEvents"] == null)
                //    {
                //        Session["FacebookEvents"] = FacebookHelper.GetEvents(user.FacebookToken);
                //    }
                //    if (Session["FacebookEvents"] != null)
                //    {
                //        model.FacebookEvents = (List<FacebookEvent>)Session["FacebookEvents"];
                //    }
                //}

                //return View("Create", model);
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
        }

        //[Authorize]
        //public ActionResult Create(EventType eventType, int? placeId)
        //{
        //    if (User.Identity.IsAuthenticated)
        //    {
        //        var userid = User.Identity.GetUserId();
        //        var user = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();

        //        if (User.IsInRole("Teacher") || User.IsInRole("Owner"))
        //        {
        //            if (placeId != null)
        //            {
        //                if (DataContext.Places.Where(p => p.Id == placeId).Where(p => p.Teachers.Where(t => t.ApplicationUser.Id == userid).Count() != 0 || p.Owners.Where(o => o.ApplicationUser.Id == userid).Count() != 0).Count() == 0)
        //                {
        //                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //                }
        //            }
        //        }


        //        var model = new EventCreateViewModel();
        //        var id = User.Identity.GetUserId();
        //        model.EventType = eventType;

        //        LoadModel(model);

        //        if (placeId != null)
        //        {
        //            model.PlaceId = (int)placeId;
        //        }

        //        if (user.FacebookToken != null)
        //        {
        //            if (Session["FacebookEvents"] == null)
        //            {
        //                Session["FacebookEvents"] = FacebookHelper.GetEvents(user.FacebookToken);
        //            }
        //            if (Session["FacebookEvents"] != null)
        //            {
        //                model.FacebookEvents = (List<FacebookEvent>)Session["FacebookEvents"];
        //            }
        //        }

        //        return View(model);
        //    }
        //    else
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //}

        //[Authorize]
        //public ActionResult AddFacebookEvent(string id, RoleName role, EventType eventType)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            var userid = User.Identity.GetUserId();
        //            var user = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();
        //            int eventId;

        //            var fbevent = ((List<FacebookEvent>)Session["FacebookEvents"]).Where(x => x.Id == id).FirstOrDefault();

        //            Address ad = new Address();
        //            Place pl = new Place();
        //            if (fbevent.Address.FacebookId != null)
        //            {
        //                ad = Utilities.Geolocation.ParseAddress(fbevent.Address.Street + " " + fbevent.Address.City + ", " + fbevent.Address.State + " " + fbevent.Address.ZipCode);
        //                pl = new Place() { FacebookId = fbevent.Address.FacebookId, Name = fbevent.Location, PlaceType = Enums.PlaceType.OtherPlace, Zip = fbevent.Address.ZipCode, Address = fbevent.Address.Street, City = fbevent.Address.City, State = (Enums.State)System.Enum.Parse(typeof(Enums.State), fbevent.Address.State), Latitude = fbevent.Address.Latitude, Longitude = fbevent.Address.Longitude };
        //            }
        //            else
        //            {
        //                ad = Utilities.Geolocation.ParseAddress(user.ZipCode != null ? user.ZipCode : "90065");
        //                pl = new Place() { Name = "TBD", PlaceType = Enums.PlaceType.OtherPlace, Zip = user.ZipCode != null ? user.ZipCode : "90065", Address = ad.StreetNumber + " " + ad.StreetName, City = ad.City, State = (Enums.State)System.Enum.Parse(typeof(Enums.State), ad.State), Latitude = ad.Latitude, Longitude = ad.Longitude };
        //            }

        //            if (eventType == EventType.Class)
        //            {
        //                var cls = new Class() { Name = fbevent.Name, ClassType = Enums.ClassType.Class, Description = fbevent.Description, EndDate = fbevent.EndTime, FacebookId = id, StartDate = fbevent.StartTime, PhotoUrl = fbevent.CoverPhoto.LargeSource, Place = pl };
        //                if (role == RoleName.Teacher)
        //                {
        //                    var teacher = DataContext.Teachers.Where(x => x.ApplicationUser.Id == userid).Include("ApplicationUser").FirstOrDefault();
        //                    teacher.Classes.Add(cls);
        //                    DataContext.Entry(teacher).State = EntityState.Modified;
        //                    DataContext.SaveChanges();
        //                }
        //                else if (role == RoleName.Owner)
        //                {
        //                    var owner = DataContext.Owners.Where(x => x.ApplicationUser.Id == userid).Include("ApplicationUser").FirstOrDefault();
        //                    owner.Classes.Add(cls);
        //                    DataContext.Entry(owner).State = EntityState.Modified;
        //                    DataContext.SaveChanges();
        //                }
        //                eventId = cls.Id;
        //            }
        //            else
        //            {
        //                var social = new Social() { Name = fbevent.Name, SocialType = Enums.SocialType.Social, Description = fbevent.Description, EndDate = fbevent.EndTime, FacebookId = id, StartDate = fbevent.StartTime, PhotoUrl = fbevent.CoverPhoto.LargeSource, Place = pl };
        //                if (role == RoleName.Promoter)
        //                {
        //                    var promoter = DataContext.Promoters.Where(x => x.ApplicationUser.Id == userid).Include("ApplicationUser").FirstOrDefault();
        //                    promoter.Socials.Add(social);
        //                    DataContext.Entry(promoter).State = EntityState.Modified;
        //                    DataContext.SaveChanges();
        //                }
        //                else if (role == RoleName.Owner)
        //                {
        //                    var owner = DataContext.Owners.Where(x => x.ApplicationUser.Id == userid).Include("ApplicationUser").FirstOrDefault();
        //                    owner.Socials.Add(social);
        //                    DataContext.Entry(owner).State = EntityState.Modified;
        //                    DataContext.SaveChanges();
        //                }
        //                eventId = social.Id;
        //            }
        //            return RedirectToAction("View", "Event", new { id = eventId, eventType = eventType });
        //        }
        //        catch (DbEntityValidationException e)
        //        {
        //            var msg = "";
        //            foreach (var eve in e.EntityValidationErrors)
        //            {
        //                msg = eve.Entry.Entity.GetType().Name + " " + eve.Entry.State;
        //                foreach (var ve in eve.ValidationErrors)
        //                {
        //                    msg = ve.PropertyName + " " + ve.ErrorMessage;
        //                }
        //            }
        //            return View();
        //        }
        //    }
        //    return View();
        //}

        [Authorize]
        [HttpPost]
        public ActionResult ConfirmFacebookEvent(ConfirmFacebookEvent model)
        {
            if (model.Type == EventType.Social)
            {
                ModelState["SkillLevel"].Errors.Clear();
            }
            var userid = User.Identity.GetUserId();
            var user = DataContext.Users.Where(u => u.Id == userid).Include("CurrentRole").FirstOrDefault();

            //  Update Month Days
            if (model.PostedMonthDays != null)
            {
                model.Event.MonthDays = String.Join("-", model.PostedMonthDays) + (model.HiddenMonthDay != "" ? ("-" + model.HiddenMonthDay) : "");
            }
            else
            {
                model.Event.MonthDays = model.HiddenMonthDay;
            }
            //  Update Month Days

            //  Update Lon/Lat for Place
            if (model.Event.Place.Latitude == 0.0 || model.Event.Place.Longitude == 0.0)
            {
                var address = Geolocation.ParseAddress(model.Event.Place.Address + " " + model.Event.Place.City + " " + model.Event.Place.State + " " + model.Event.Place.Zip);
                model.Event.Place.Latitude = address.Latitude;
                model.Event.Place.Longitude = address.Longitude;
            }
            //  Update Lon/Lat for Place

            if (ModelState.IsValid)
            {
                var evt = model.Event;

                try
                {
                    //  Check if FB event exists
                    if (DataContext.Events.Where(e => e.FacebookId == evt.FacebookId).Count() == 0)
                    {
                        int eventId = 0;

                        if (model.PostedStyles == null)
                        {
                            model.PostedStyles = new PostedStyles();
                            model.PostedStyles.DanceStyleIds = new string[0];
                        }

                        //  Import Facebook Place
                        if (evt.Place.Id == 0)
                        {
                            DataContext.Places.Add(evt.Place);
                            DataContext.SaveChanges();

                            //  Add the place to user if it's not a Facebook Page
                            if (evt.Place.FacebookId == null)
                            {
                                user.Places.Add(evt.Place);
                                DataContext.SaveChanges();
                            }
                        }
                        else
                        {
                            DataContext.Places.Attach(evt.Place);
                        }

                        //Place pl = new Place();
                        //if (model.PlaceId == 0)
                        //{
                        //    if (model.NewPlace.Latitude == 0.0 || model.NewPlace.Longitude == 0.0)
                        //    {
                        //        var ad = Geolocation.ParseAddress(model.NewPlace.Address + " " + model.NewPlace.City + ", " + model.NewPlace.State + " " + model.NewPlace.Zip);
                        //        model.NewPlace.Longitude = ad.Longitude;
                        //        model.NewPlace.Latitude = ad.Latitude;
                        //    }
                        //    pl = model.NewPlace;
                        //    pl.PlaceType = Enums.PlaceType.OtherPlace;
                        //}
                        //else if (model.PlaceId > 0)
                        //{
                        //    pl = DataContext.Places.Where(p => p.Id == model.PlaceId).FirstOrDefault();
                        //}
                        //else
                        //{
                        //    Address ad = new Address();
                        //    ad = Utilities.Geolocation.ParseAddress(user.ZipCode != null ? user.ZipCode : "90065");
                        //    pl = new Place() { Name = "TBD", PlaceType = Enums.PlaceType.OtherPlace, Zip = user.ZipCode != null ? user.ZipCode : "90065", Address = ad.StreetNumber + " " + ad.StreetName, City = ad.City, State = (Enums.State)System.Enum.Parse(typeof(Enums.State), ad.State), Latitude = ad.Latitude, Longitude = ad.Longitude };
                        //}
                        //evt.Place = pl;

                        var obj = new LinkedFacebookObject() { MediaSource = MediaSource.Facebook, Name = evt.Name, FacebookId = evt.FacebookId, Url = evt.FacebookLink, ObjectType = FacebookObjectType.Event };
                        if (model.Type == EventType.Class)
                        {
                            var cls = new Class() { Name = evt.Name, Description = evt.Description, FacebookId = evt.FacebookId, PhotoUrl = evt.PhotoUrl, StartDate = evt.StartDate, EndDate = evt.EndDate, StartTime = evt.StartTime, EndTime = evt.EndTime, ClassType = model.ClassType != null ? (ClassType)Enum.Parse(typeof(ClassType), model.ClassType.ToString()) : ClassType.Class, Place = evt.Place, FacebookLink = evt.FacebookLink, Creator = user, DanceStyles = DataContext.DanceStyles.Where(s => model.PostedStyles.DanceStyleIds.Any(ps => ps == s.Id.ToString())).ToList(), IsAvailable = evt.IsAvailable, LinkedFacebookObjects = new List<LinkedFacebookObject>() { obj }, Recurring = evt.Recurring, Interval = evt.Interval, Frequency = evt.Frequency, MonthDays = evt.MonthDays, SkillLevel = model.SkillLevel, UpdatedDate = evt.UpdatedDate };
                            cls.EventMembers = new List<EventMember>();
                            cls.Teachers = new List<Teacher>();
                            cls.Owners = new List<Owner>();
                            cls.EventMembers.Add(new EventMember() { Event = cls, Member = user, Admin = true });
                            //  Get Feed
                            var feeds = FacebookHelper.GetFeed(evt.FacebookId, user.FacebookToken).Where(f => f.Link != null || f.Message != null).ToList();
                            cls.Feeds = feeds.Where(f => f.Message != null || f.Link != null).Select(f => new Feed() { Link = f.Link, Message = f.Message, UpdateTime = f.Updated_Time }).ToList();
                            //  Get Feed

                            //  Add Recurring Events
                            if (evt.Recurring)
                            {
                                var sdate = evt.StartDate;
                                var edate = evt.EndDate;
                                cls.EventInstances = new List<EventInstance>();
                                sdate = ApplicationUtility.GetNextDate(sdate, evt.Frequency, (int)evt.Interval, evt.Day, sdate, evt.MonthDays);

                                while (sdate <= edate)
                                {
                                    cls.EventInstances.Add(new EventInstance() { EventId = cls.Id, DateTime = sdate });
                                    sdate = ApplicationUtility.GetNextDate(sdate, evt.Frequency, (int)evt.Interval, evt.Day, sdate.AddDays(1), evt.MonthDays);
                                }
                            }
                            //  Add Recurring Events

                            DataContext.Events.Add(cls);
                            DataContext.SaveChanges();

                            if (user.CurrentRole != null)
                            {
                                if (user.CurrentRole.Name == "Teacher")
                                {
                                    var teacher = DataContext.Teachers.Where(x => x.ApplicationUser.Id == userid).Include("ApplicationUser").Include("Places").FirstOrDefault();
                                    DataContext.Teachers.Attach(teacher);
                                    cls.Teachers.Add(teacher);
                                    DataContext.Entry(teacher).State = EntityState.Modified;
                                    DataContext.SaveChanges();
                                }
                                else if (user.CurrentRole.Name == "Owner")
                                {
                                    var owner = DataContext.Owners.Where(x => x.ApplicationUser.Id == userid).Include("ApplicationUser").FirstOrDefault();
                                    DataContext.Owners.Attach(owner);
                                    cls.Owners.Add(owner);
                                    DataContext.Entry(owner).State = EntityState.Modified;
                                    DataContext.SaveChanges();
                                }
                            }
                            eventId = cls.Id;
                        }
                        else
                        {
                            var social = new Social() { Name = evt.Name, Description = evt.Description, FacebookId = evt.FacebookId, PhotoUrl = evt.PhotoUrl, StartDate = evt.StartDate, EndDate = evt.EndDate, StartTime = evt.StartTime, EndTime = evt.EndTime, SocialType = model.SocialType != null ? (SocialType)Enum.Parse(typeof(SocialType), model.SocialType.ToString()) : SocialType.Social, Place = evt.Place, FacebookLink = evt.FacebookLink, Creator = user, DanceStyles = DataContext.DanceStyles.Where(s => model.PostedStyles.DanceStyleIds.Any(ps => ps == s.Id.ToString())).ToList(), IsAvailable = evt.IsAvailable, LinkedFacebookObjects = new List<LinkedFacebookObject>() { obj }, Recurring = evt.Recurring, Interval = evt.Interval, Frequency = evt.Frequency, MonthDays = evt.MonthDays, UpdatedDate = evt.UpdatedDate };
                            social.EventMembers = new List<EventMember>();
                            social.Promoters = new List<Promoter>();
                            social.Owners = new List<Owner>();
                            social.EventMembers.Add(new EventMember() { Event = social, Member = user, Admin = true });
                            //  Get Feed
                            var feeds = FacebookHelper.GetFeed(evt.FacebookId, user.FacebookToken).Where(f => f.Link != null || f.Message != null).ToList();
                            social.Feeds = feeds.Where(f => f.Message != null || f.Link != null).Select(f => new Feed() { Link = f.Link, Message = f.Message, UpdateTime = f.Updated_Time }).ToList();
                            //  Get Feed

                            //  Add Recurring Events
                            if (evt.Recurring)
                            {
                                var sdate = evt.StartDate;
                                var edate = evt.EndDate;
                                social.EventInstances = new List<EventInstance>();
                                sdate = ApplicationUtility.GetNextDate(sdate, evt.Frequency, (int)evt.Interval, evt.Day, sdate, evt.MonthDays);

                                while (sdate <= edate)
                                {
                                    social.EventInstances.Add(new EventInstance() { EventId = social.Id, DateTime = sdate });
                                    sdate = ApplicationUtility.GetNextDate(sdate, evt.Frequency, (int)evt.Interval, evt.Day, sdate.AddDays(1), evt.MonthDays);
                                }
                            }
                            //  Add Recurring Events

                            DataContext.Events.Add(social);
                            DataContext.SaveChanges();

                            if (user.CurrentRole != null)
                            {
                                if (user.CurrentRole.Name == "Promoter")
                                {
                                    var promoter = DataContext.Promoters.Where(x => x.ApplicationUser.Id == userid).Include("ApplicationUser").FirstOrDefault();
                                    DataContext.Promoters.Attach(promoter);
                                    social.Promoters.Add(promoter);
                                    DataContext.Entry(social).State = EntityState.Modified;
                                    DataContext.SaveChanges();
                                }
                                else if (user.CurrentRole.Name == "Owner")
                                {
                                    var owner = DataContext.Owners.Where(x => x.ApplicationUser.Id == userid).Include("ApplicationUser").FirstOrDefault();
                                    DataContext.Owners.Attach(owner);
                                    social.Owners.Add(owner);
                                    DataContext.Entry(owner).State = EntityState.Modified;
                                    DataContext.SaveChanges();
                                }
                            }
                            eventId = social.Id;
                        }
                        return RedirectToAction("View", "Event", new { id = eventId, eventType = model.Type });
                    }
                    else 
                    {
                        return RedirectToAction("Home", user.CurrentRole != null ? user.CurrentRole.Name : "Dancer", new { username = User.Identity.Name });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    var msg = "";
                    foreach (var eve in e.EntityValidationErrors)
                    {
                        msg = eve.Entry.Entity.GetType().Name + " " + eve.Entry.State;
                        foreach (var ve in eve.ValidationErrors)
                        {
                            msg = ve.PropertyName + " " + ve.ErrorMessage;
                        }
                    }
                    return View();
                }
            }
            else
            {
                //  Load Dance Styles
                var styles = new List<DanceStyleListItem>();
                foreach (DanceStyle s in DataContext.DanceStyles)
                {
                    styles.Add(new DanceStyleListItem { Id = s.Id, Name = s.Name });
                }
                model.AvailableStyles = styles.OrderBy(x => x.Name);
                if (model.PostedStyles != null)
                {
                    model.SelectedStyles = model.AvailableStyles.Where(s => model.PostedStyles.DanceStyleIds.Contains(s.Id.ToString()));
                }
                //  Load Dance Styles

                //  For Month Days Checkbox List
                var selectedMonthDays = new List<SelectListItem>();
                string[] daysarray;
                model.SelectedMonthDays = new List<SelectListItem>();
                if (model.Event.MonthDays != null)
                {
                    daysarray = model.Event.MonthDays.Split(new char[] { '-' });
                    foreach (var day in daysarray)
                    {
                        model.SelectedMonthDays.Add(new SelectListItem() { Value = day, Text = day });
                    }
                }
                model.MonthDays = new List<SelectListItem>() { new SelectListItem() { Value = "1", Text = "1st" }, new SelectListItem() { Value = "2", Text = "2nd" }, new SelectListItem() { Value = "3", Text = "3rd" }, new SelectListItem() { Value = "4", Text = "4th" } };
                //  For Month Days Checkbox List

                //  Set Month day text
                var daysofmonth = new string[] { "blank", "1st", "2nd", "3rd", "4th", "5th", "6th", "7th", "8th", "9th", "10th", "11th", "12th", "13th", "14th", "15th", "16th", "17th", "18th", "19th", "20th", "21st", "22nd", "23rd", "24th", "25th", "26th", "27th", "28th", "29th", "30th", "31st" };
                model.MonthDay = daysofmonth[model.Event.StartDate.Day];
                //  Set Month day text

                ////  Load Places
                //model.Places = new List<PlaceItem>();
                //var blankPlace = new PlaceItem() { Id = 0 };
                //model.Places.Add(blankPlace);
                //if (model.EventType == EventType.Class)
                //{
                //    if (Session["MyRole"] != null)
                //    {
                //        if ((RoleName)Session["MyRole"] == RoleName.Teacher)
                //        {
                //            foreach (var pl in DataContext.Places.Where(p => p.Teachers.Any(t => t.ApplicationUser.Id == userid)))
                //            {
                //                model.Places.Add(new PlaceItem() { Address = pl.Address, Address2 = pl.Address2, City = pl.City, Country = pl.Country, FacebookId = pl.FacebookId, FacebookLink = pl.FacebookLink, Filename = pl.Filename, Id = pl.Id, Latitude = pl.Latitude, Longitude = pl.Longitude, Name = pl.Name, PlaceType = pl.PlaceType, State = pl.State, ThumbnailFilename = pl.ThumbnailFilename, Website = pl.Website, Zip = pl.Zip });
                //            }
                //        }
                //        else
                //        {
                //            foreach (var pl in DataContext.Places.Where(p => p.Owners.Any(o => o.ApplicationUser.Id == userid)))
                //            {
                //                model.Places.Add(new PlaceItem() { Address = pl.Address, Address2 = pl.Address2, City = pl.City, Country = pl.Country, FacebookId = pl.FacebookId, FacebookLink = pl.FacebookLink, Filename = pl.Filename, Id = pl.Id, Latitude = pl.Latitude, Longitude = pl.Longitude, Name = pl.Name, PlaceType = pl.PlaceType, State = pl.State, ThumbnailFilename = pl.ThumbnailFilename, Website = pl.Website, Zip = pl.Zip });
                //            }
                //        }
                //    }
                //}
                //else
                //{
                //    if (Session["MyRole"] != null)
                //    {
                //        if ((RoleName)Session["MyRole"] == RoleName.Promoter)
                //        {
                //            foreach (var pl in DataContext.Places.Where(p => p.Promoters.Any(pr => pr.ApplicationUser.Id == userid)))
                //            {
                //                model.Places.Add(new PlaceItem() { Address = pl.Address, Address2 = pl.Address2, City = pl.City, Country = pl.Country, FacebookId = pl.FacebookId, FacebookLink = pl.FacebookLink, Filename = pl.Filename, Id = pl.Id, Latitude = pl.Latitude, Longitude = pl.Longitude, Name = pl.Name, PlaceType = pl.PlaceType, State = pl.State, ThumbnailFilename = pl.ThumbnailFilename, Website = pl.Website, Zip = pl.Zip });
                //            }
                //        }
                //        else
                //        {
                //            foreach (var pl in DataContext.Places.Where(p => p.Owners.Any(o => o.ApplicationUser.Id == userid)))
                //            {
                //                model.Places.Add(new PlaceItem() { Address = pl.Address, Address2 = pl.Address2, City = pl.City, Country = pl.Country, FacebookId = pl.FacebookId, FacebookLink = pl.FacebookLink, Filename = pl.Filename, Id = pl.Id, Latitude = pl.Latitude, Longitude = pl.Longitude, Name = pl.Name, PlaceType = pl.PlaceType, State = pl.State, ThumbnailFilename = pl.ThumbnailFilename, Website = pl.Website, Zip = pl.Zip });
                //            }
                //        }
                //    }
                //}
                ////  Load Places

                return View(model);
            }
        }

        [Authorize]
        public ActionResult ConfirmFacebookEvent(string id, EventType? eventType)
        {
            var model = new ConfirmFacebookEvent();
            var userid = User.Identity.GetUserId();
            var user = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();
            model.User = user;

            //  Load Dance Styles
            var selectedStyles = new List<DanceStyleListItem>();
            model.SelectedStyles = selectedStyles;

            var styles = new List<DanceStyleListItem>();
            foreach (DanceStyle s in DataContext.DanceStyles)
            {
                styles.Add(new DanceStyleListItem { Id = s.Id, Name = s.Name });
            }
            model.AvailableStyles = styles.OrderBy(x => x.Name);
            //  Load Dance Styles

            model.Type = eventType;

            var fbevent = FacebookHelper.GetEvent(id, user.FacebookToken, "id,cover,description,end_time,is_date_only,location,name,owner,privacy,start_time,ticket_uri,timezone,updated_time,venue,parent_group");            //  ((List<FacebookEvent>)Session["FacebookEvents"]).Where(x => x.Id == id).FirstOrDefault();
            var available = (fbevent.Privacy == "OPEN" || fbevent.Privacy == "FRIENDS") ? true : false;
            model.Event = new Event() { Name = fbevent.Name, Description = fbevent.Description, StartDate = fbevent.StartTime, StartTime = fbevent.StartTime, EndDate = fbevent.EndTime, EndTime = fbevent.EndTime, PhotoUrl = fbevent.CoverPhoto.LargeSource, FacebookId = fbevent.Id, FacebookLink = fbevent.EventLink, Interval = 1, IsAvailable = available, UpdatedDate = fbevent.Updated };

            //  For Month Days Checkbox List
            var selectedMonthDays = new List<SelectListItem>();
            string[] daysarray;
            model.SelectedMonthDays = new List<SelectListItem>();
            if (model.Event.MonthDays != null)
            {
                daysarray = model.Event.MonthDays.Split(new char[] { '-' });
                foreach (var day in daysarray)
                {
                    model.SelectedMonthDays.Add(new SelectListItem() { Value = day, Text = day });
                }
            }
            model.MonthDays = new List<SelectListItem>() { new SelectListItem() { Value = "1", Text = "1st" }, new SelectListItem() { Value = "2", Text = "2nd" }, new SelectListItem() { Value = "3", Text = "3rd" }, new SelectListItem() { Value = "4", Text = "4th" } };
            //  For Month Days Checkbox List

            //  Set Month day text
            var daysofmonth = new string[] { "blank", "1st", "2nd", "3rd", "4th", "5th", "6th", "7th", "8th", "9th", "10th", "11th", "12th", "13th", "14th", "15th", "16th", "17th", "18th", "19th", "20th", "21st", "22nd", "23rd", "24th", "25th", "26th", "27th", "28th", "29th", "30th", "31st" };
            model.MonthDay = daysofmonth[model.Event.StartDate.Day];
            //  Set Month day text

            //  Extract Place from Facebook
            if (fbevent.Address != null)
            {
                //  Place is a Page in Facebook
                if (fbevent.Address.FacebookId != null)
                {
                    var place = DataContext.Places.Where(p => p.FacebookId == fbevent.Address.FacebookId).FirstOrDefault();

                    //  Existing Facebook place in the database
                    if (place != null)
                    {
                        model.Event.Place = place;
                    }
                    //  New Place from Facebook
                    else
                    {
                        //  Place has a Page in Facebook
                        if (fbevent.Address.FacebookId != null)
                        {
                            var fbplace = FacebookHelper.GetData(user.FacebookToken, fbevent.Address.FacebookId);

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

                            model.Event.Place = new Place() { Name = fbevent.Location, Address = fbevent.Address.Street, City = fbevent.Address.City, State = fbevent.Address.State != null ? (State)Enum.Parse(typeof(State), fbevent.Address.State) : State.CA, Zip = fbevent.Address.ZipCode, Country = fbevent.Address.Country, Latitude = fbevent.Address.Latitude, Longitude = fbevent.Address.Longitude, FacebookId = fbevent.Address.FacebookId, PlaceType = placetype, Public = true, Website = fbevent.Address.WebsiteUrl, FacebookLink = fbevent.Address.FacebookUrl, Filename = fbplace.cover != null ? fbplace.cover.source : null, ThumbnailFilename = fbplace.cover != null ? fbplace.cover.source : null };
                        }
                    }
                }
                //  Place does not have a Facebook Page
                else
                {
                    model.Event.Place = new Place() { Name = fbevent.Location, Address = fbevent.Address.Street, City = fbevent.Address.City, State = fbevent.Address.State != null ? (State)Enum.Parse(typeof(State), fbevent.Address.State) : State.CA, Zip = fbevent.Address.ZipCode, Country = fbevent.Address.Country, Latitude = fbevent.Address.Latitude, Longitude = fbevent.Address.Longitude, Public = false, Website = fbevent.Address.WebsiteUrl, FacebookLink = fbevent.Address.FacebookUrl };
                }
            }
            //  Place not in Facebook
            else
            {
                model.Event.Place = new Place() { PlaceType = PlaceType.OtherPlace, Public = false };
            }
            //  Extract Place

            //model.PlaceId = -1;
            //model.NewPlace = new PlaceItem() { Id = 0, Name = fbevent.Location, Address = fbevent.Address.Street, City = fbevent.Address.City, State = fbevent.Address.State != null ? (State)Enum.Parse(typeof(State), fbevent.Address.State) : State.CA, Zip = fbevent.Address.ZipCode, Latitude = fbevent.Address.Latitude, Longitude = fbevent.Address.Longitude, FacebookId = fbevent.Address.FacebookId };
            //model.Places = new List<PlaceItem>();
            //var blankPlace = new PlaceItem() { Id = 0 };
            //model.Places.Add(blankPlace);
            //if (eventType == EventType.Class)
            //{
            //    if (Session["MyRole"] != null)
            //    {
            //        if ((RoleName)Session["MyRole"] == RoleName.Teacher)
            //        {
            //            foreach (var pl in DataContext.Places.Where(p => p.Teachers.Any(t => t.ApplicationUser.Id == userid)))
            //            {
            //                model.Places.Add(new PlaceItem() { Address = pl.Address, Address2 = pl.Address2, City = pl.City, Country = pl.Country, FacebookId = pl.FacebookId, FacebookLink = pl.FacebookLink, Filename = pl.Filename, Id = pl.Id, Latitude = pl.Latitude, Longitude = pl.Longitude, Name = pl.Name, PlaceType = pl.PlaceType, State = pl.State, ThumbnailFilename = pl.ThumbnailFilename, Website = pl.Website, Zip = pl.Zip });
            //            }
            //        }
            //        else
            //        {
            //            foreach (var pl in DataContext.Places.Where(p => p.Owners.Any(o => o.ApplicationUser.Id == userid)))
            //            {
            //                model.Places.Add(new PlaceItem() { Address = pl.Address, Address2 = pl.Address2, City = pl.City, Country = pl.Country, FacebookId = pl.FacebookId, FacebookLink = pl.FacebookLink, Filename = pl.Filename, Id = pl.Id, Latitude = pl.Latitude, Longitude = pl.Longitude, Name = pl.Name, PlaceType = pl.PlaceType, State = pl.State, ThumbnailFilename = pl.ThumbnailFilename, Website = pl.Website, Zip = pl.Zip });
            //            }
            //        }
            //    }
            //}
            //else
            //{
            //    if (Session["MyRole"] != null)
            //    {
            //        if ((RoleName)Session["MyRole"] == RoleName.Promoter)
            //        {
            //            foreach (var pl in DataContext.Places.Where(p => p.Promoters.Any(pr => pr.ApplicationUser.Id == userid)))
            //            {
            //                model.Places.Add(new PlaceItem() { Address = pl.Address, Address2 = pl.Address2, City = pl.City, Country = pl.Country, FacebookId = pl.FacebookId, FacebookLink = pl.FacebookLink, Filename = pl.Filename, Id = pl.Id, Latitude = pl.Latitude, Longitude = pl.Longitude, Name = pl.Name, PlaceType = pl.PlaceType, State = pl.State, ThumbnailFilename = pl.ThumbnailFilename, Website = pl.Website, Zip = pl.Zip });
            //            }
            //        }
            //        else
            //        {
            //            foreach (var pl in DataContext.Places.Where(p => p.Owners.Any(o => o.ApplicationUser.Id == userid)))
            //            {
            //                model.Places.Add(new PlaceItem() { Address = pl.Address, Address2 = pl.Address2, City = pl.City, Country = pl.Country, FacebookId = pl.FacebookId, FacebookLink = pl.FacebookLink, Filename = pl.Filename, Id = pl.Id, Latitude = pl.Latitude, Longitude = pl.Longitude, Name = pl.Name, PlaceType = pl.PlaceType, State = pl.State, ThumbnailFilename = pl.ThumbnailFilename, Website = pl.Website, Zip = pl.Zip });
            //            }
            //        }
            //    }
            //}

            //if (fbevent.Address.Street != null && fbevent.Address.City != null && fbevent.Address.State != null && fbevent.Address.ZipCode != null)
            //{
            //    var fbeventAddress = Geolocation.ParseAddress(fbevent.Address.Street + " " + fbevent.Address.City + ", " + fbevent.Address.State + " " + fbevent.Address.ZipCode);
            //    var pl = model.Places.Where(p => p.Address == fbeventAddress.Street && p.City == fbeventAddress.City && p.State.ToString() == fbeventAddress.State).FirstOrDefault();
            //    if (pl != null)
            //    {
            //        pl.Selected = true;
            //    }
            //    else
            //    {
            //        blankPlace.Selected = true;
            //    }
            //}
            //else
            //{
            //    blankPlace.Selected = true;
            //}

            return View(model);
        }

        //private void LoadCreateModel(EventCreateViewModel model)
        //{
        //    var id = User.Identity.GetUserId();
        //    var user = UserManager.FindById(id);
        //    //  model.User = user;

        //    var selectedStyles = new List<DanceStyleListItem>();
        //    model.SelectedStyles = selectedStyles;

        //    var styles = new List<DanceStyleListItem>();
        //    foreach (DanceStyle s in DataContext.DanceStyles)
        //    {
        //        styles.Add(new DanceStyleListItem { Id = s.Id, Name = s.Name });
        //    }
        //    model.AvailableStyles = styles.OrderBy(x => x.Name);

        //    if (user.CurrentRole.Name == "Teacher")
        //    {
        //        model.PlaceList = DataContext.Places.Where(x => x.Teachers.Any(t => t.ApplicationUser.Id == id)).Select(p => new SelectListItem() { Text = p.Name, Value = p.Id.ToString() }).ToList();
        //    }
        //    else if (user.CurrentRole.Name == "Owner")
        //    {
        //        model.PlaceList = DataContext.Places.Where(x => x.Owners.Any(t => t.ApplicationUser.Id == id)).Select(p => new SelectListItem() { Text = p.Name, Value = p.Id.ToString() }).ToList();
        //    }
        //    else if (user.CurrentRole.Name == "Promoter")
        //    {
        //        model.PlaceList = DataContext.Places.Where(x => x.Promoters.Any(t => t.ApplicationUser.Id == id)).Select(p => new SelectListItem() { Text = p.Name, Value = p.Id.ToString() }).ToList();
        //    }
        //}

        //[Authorize]
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Create(EventCreateViewModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var event1 = model.Event;
        //        event1.StartDate = event1.StartDate.AddHours(model.Time.Hour).AddMinutes(model.Time.Minute);
        //        event1.Place = DataContext.Places.Find(model.PlaceId);
        //        var id = User.Identity.GetUserId();

        //        event1.DanceStyles = new List<DanceStyle>();
        //        var styles = DataContext.DanceStyles.Where(x => model.PostedStyles.DanceStyleIds.Contains(x.Id.ToString())).ToList();

        //        event1.DanceStyles.Clear();
        //        int newId = 0;

        //        foreach (DanceStyle s in styles)
        //        {
        //            event1.DanceStyles.Add(s);
        //        }

        //        if (model.EventType == EventType.Class)
        //        {
        //            var class1 = ConvertToClass(event1);
        //            if (model.Role == RoleName.Teacher)
        //            {
        //                var teacher = DataContext.Teachers.Include("ApplicationUser").Where(x => x.ApplicationUser.Id == id).FirstOrDefault();
        //                class1.Teachers.Add(teacher);
        //            }
        //            DataContext.Events.Add(class1);
        //            newId = class1.Id;
        //        }
        //        else if (model.EventType == EventType.Social)
        //        {
        //            var social = ConvertToSocial(event1);
        //            if (model.Role == RoleName.Promoter)
        //            {
        //                var promoter = DataContext.Promoters.Include("ApplicationUser").Where(x => x.ApplicationUser.Id == id).FirstOrDefault();
        //                social.Promoters.Add(promoter);
        //            }
        //            DataContext.Events.Add(social);
        //            newId = social.Id;
        //        }
        //        DataContext.SaveChanges();
        //        return RedirectToAction("View", "Event", new { id = newId, eventType = model.EventType });
        //        //promoter.ContactEmail = model.Promoter.ContactEmail;
        //        //promoter.Website = model.Promoter.Website;
        //        //promoter.Facebook = model.Promoter.Facebook;

        //        //DataContext.Entry(promoter).State = EntityState.Modified;
        //        //DataContext.SaveChanges();
        //    }
        //    else
        //    {
        //        var allErrors = ModelState.Values.SelectMany(v => v.Errors);
        //        LoadCreateModel(model);
        //        return View(model);
        //    }
        //}

        [Authorize]
        public PartialViewResult PostReview(EventViewModel model)
        {
            var userid = User.Identity.GetUserId();
            if (DataContext.Reviews.Where(r => r.Event.Id == model.Event.Id && r.Author.Id == userid).Count() == 0)
            {
                var auth = DataContext.Users.Where(x => x.Id == userid).FirstOrDefault();
                var ev = DataContext.Events.Where(e => e.Id == model.Event.Id).Include("Reviews").FirstOrDefault();
                ev.Reviews.Add(new Review() { ReviewText = model.Reviews.NewReview.ReviewText, ReviewDate = DateTime.Now, Like = model.Reviews.NewReview.Like, Author = auth });
                DataContext.SaveChanges();

                var reviews = DataContext.Reviews.Where(x => x.Event.Id == model.Event.Id);
                return PartialView("~/Views/Shared/Events/_ReviewsPartial.cshtml", reviews);
            }
            else
            {
                return PartialView();
            }
            
            //if (ModelState.IsValid)
            //{
            //    return RedirectToAction("Class", "Event", new { id = model.Event.Id});
            //}
            //else
            //{
            //    var allErrors = ModelState.Values.SelectMany(v => v.Errors);
            //}
            //return View(model);
        }

        [Authorize]
        public ActionResult AddFacebookLink(int id, EventType eventType)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var model = LoadEvent(id, eventType);
            model.EventType = eventType;

            if (model.Event == null)
            {
                return HttpNotFound();
            }

            return View(model);
        }

        [Authorize]
        public ActionResult GetAvailableFacebookEvents(int id, EventType eventType)
        {
            var model = new EventLinkedFacebookEventContainer();
            model.Type = eventType;
            var userid = User.Identity.GetUserId();
            var user = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();
            var evt = DataContext.Events.Where(e => e.Id == id).FirstOrDefault();
            model.Event = evt;

            var facebookToken = user.FacebookToken;
            if (facebookToken != null)
            {
                //  Get Facebook Events
                var eventIds = DataContext.Events.Where(e => e.Id == id).Include("LinkedFacebookObjects").FirstOrDefault().LinkedFacebookObjects.Where(fo => fo.ObjectType == FacebookObjectType.Event).Select(ee => ee.FacebookId).ToArray();
                if (Session["ExternalFacebookEvents"] == null)
                {
                    Session["ExternalFacebookEvents"] = FacebookHelper.GetEvents(facebookToken).Where(f => !eventIds.Any(e => e.Contains(f.Id))).ToList();
                }
                else
                {
                    Session["ExternalFacebookEvents"] = ((List<FacebookEvent>)Session["ExternalFacebookEvents"]).Where(f => !eventIds.Any(e => e.Contains(f.Id))).ToList();
                }
                model.FacebookEvents = (List<FacebookEvent>)Session["ExternalFacebookEvents"];
                //  Get Facebook Events
            }

            return PartialView("~/Views/Shared/Events/_AddLinkedFacebookEventsPartial.cshtml", model);
        }

        [Authorize]
        public ActionResult GetAvailableFacebookGroups(int id, EventType eventType)
        {
            var model = new EventLinkedFacebookGroupContainer();
            model.Type = eventType;
            var userid = User.Identity.GetUserId();
            var user = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();
            var evt = DataContext.Events.Where(e => e.Id == id).FirstOrDefault();
            model.Event = evt;

            var facebookToken = user.FacebookToken;
            if (facebookToken != null)
            {
                //  Get Facebook Groups
                var groupIds = DataContext.Events.Where(e => e.Id == id).Include("LinkedFacebookObjects").FirstOrDefault().LinkedFacebookObjects.Where(fo => fo.ObjectType == FacebookObjectType.Group).Select(ee => ee.FacebookId).ToArray();
                if (Session["ExternalFacebookGroups"] == null)
                {
                    Session["ExternalFacebookGroups"] = FacebookHelper.GetGroups(facebookToken).Where(f => !groupIds.Any(e => e.Contains(f.Id))).ToList();
                }
                else
                {
                    Session["ExternalFacebookGroups"] = ((List<FacebookGroup>)Session["ExternalFacebookGroups"]).Where(f => !groupIds.Any(e => e.Contains(f.Id))).ToList();
                }
                model.FacebookGroups = (List<FacebookGroup>)Session["ExternalFacebookGroups"];
                //  Get Facebook Groups
            }

            return PartialView("~/Views/Shared/Events/_AddLinkedFacebookGroupsPartial.cshtml", model);
        }

        [Authorize]
        public ActionResult RemoveLinkedFacebookObject(int id, string returnUrl)
        {
            DataContext.LinkedFacebookObjects.Remove(DataContext.LinkedFacebookObjects.Where(o => o.Id == id).FirstOrDefault());
            DataContext.SaveChanges();
            return Redirect(returnUrl);
        }

        [Authorize]
        public ActionResult LinkFacebookEvent(int id, string fbId, string facebookLink)
        {
            var userid = User.Identity.GetUserId();
            var user = UserManager.FindById(userid);

            if (fbId != null)
            {
                var fbEvent = ((List<FacebookEvent>)Session["ExternalFacebookEvents"]).Where(f => f.Id == fbId).FirstOrDefault();
                DataContext.Events.Where(e => e.Id == id).Include("LinkedFacebookObjects").FirstOrDefault().LinkedFacebookObjects.Add(new LinkedFacebookObject { FacebookId = fbId, MediaSource = MediaSource.Facebook, Name = fbEvent.Name, Url = fbEvent.EventLink, ObjectType = FacebookObjectType.Event });
                DataContext.SaveChanges();
            }
            else if (facebookLink != null)
            {
                fbId = ParseFacebookLink(facebookLink);
                if (fbId != "")
                {
                    var fbEvent = FacebookHelper.GetEvent(fbId, user.FacebookToken);
                    DataContext.Events.Where(e => e.Id == id).Include("LinkedFacebookObjects").FirstOrDefault().LinkedFacebookObjects.Add(new LinkedFacebookObject { FacebookId = fbId, MediaSource = MediaSource.Facebook, Name = fbEvent.Name, Url = fbEvent.EventLink, ObjectType = FacebookObjectType.Event });
                    DataContext.SaveChanges();
                }
            }
            var evt = DataContext.Events.Where(e => e.Id == id).FirstOrDefault();
            return RedirectToAction("View", "Event", new { id = id, eventType = evt is Class ? EventType.Class : EventType.Social });
        }

        [Authorize]
        public ActionResult LinkFacebookGroup(int id, string fbId, string facebookLink)
        {
            var userid = User.Identity.GetUserId();
            var user = UserManager.FindById(userid);

            if (fbId != null)
            {
                var fbGroup = ((List<FacebookGroup>)Session["ExternalFacebookGroups"]).Where(f => f.Id == fbId).FirstOrDefault();
                DataContext.Events.Where(e => e.Id == id).Include("LinkedFacebookObjects").FirstOrDefault().LinkedFacebookObjects.Add(new LinkedFacebookObject { FacebookId = fbId, MediaSource = MediaSource.Facebook, Name = fbGroup.Name, Url = "https://www.facebook.com/groups/" + fbGroup.Id, ObjectType = FacebookObjectType.Group });
                DataContext.SaveChanges();
            }
            else if (facebookLink != null)
            {
                fbId = ParseFacebookLink(facebookLink);
                if (fbId != "")
                {
                    var fbGroup = FacebookHelper.GetGroup(fbId, user.FacebookToken);
                    DataContext.Events.Where(e => e.Id == id).Include("LinkedFacebookObjects").FirstOrDefault().LinkedFacebookObjects.Add(new LinkedFacebookObject { FacebookId = fbId, MediaSource = MediaSource.Facebook, Name = fbGroup.Name, Url = "https://www.facebook.com/groups/" + fbGroup.Id, ObjectType = FacebookObjectType.Group });
                    DataContext.SaveChanges();
                }
            }
            var evt = DataContext.Events.Where(e => e.Id == id).FirstOrDefault();
            return RedirectToAction("View", "Event", new { id = id, eventType = evt is Class ? EventType.Class : EventType.Social });
        }

        //[Authorize]
        //public ActionResult LinkFacebookPage(int id, string facebookLink)
        //{
        //    var userid = User.Identity.GetUserId();
        //    var user = UserManager.FindById(userid);

        //    if (facebookLink != null)
        //    {
        //        fbId = ParseFacebookLink(facebookLink);
        //        if (fbId != "")
        //        {
        //            var fbGroup = FacebookHelper.GetGroup(fbId, user.FacebookToken);
        //            DataContext.Events.Where(e => e.Id == id).Include("LinkedFacebookObjects").FirstOrDefault().LinkedFacebookObjects.Add(new LinkedFacebookObject { FacebookId = fbId, MediaSource = MediaSource.Facebook, Name = fbGroup.Name, Url = "https://www.facebook.com/groups/" + fbGroup.Id, ObjectType = FacebookObjectType.Group });
        //            DataContext.SaveChanges();
        //        }
        //    }
        //    var evt = DataContext.Events.Where(e => e.Id == id).FirstOrDefault();
        //    return RedirectToAction("View", "Event", new { id = id, eventType = evt is Class ? EventType.Class : EventType.Social });
        //}

        public ActionResult GetRelatedEvents(int id, EventType eventType)
        {
            //  Get Events
            var model = new RelatedEvents();
            model.EventType = eventType;

            if (eventType == EventType.Class)
            {
                model.Events = DataContext.Events.OfType<Class>().Include("Place").Where(c => (c.Teachers.Any(t => t.Classes.Any(cl => cl.Id == id)) || c.Owners.Any(t => t.Classes.Any(cl => cl.Id == id))) && c.Id != id).Cast<Event>();
            }
            else
            {
                model.Events = DataContext.Events.OfType<Social>().Include("Place").Where(c => (c.Promoters.Any(t => t.Socials.Any(cl => cl.Id == id)) || c.Owners.Any(t => t.Socials.Any(cl => cl.Id == id))) && c.Id != id).Cast<Event>();
            }

            return PartialView("~/Views/Shared/Events/_RelatedEventsPartial.cshtml", model);
            //  Get Events
        }

        [Authorize]
        public ActionResult AddTeacher(int id)
        {
            var model = new AddTeacherViewModel();
            var clss = DataContext.Events.OfType<Class>().Where(e => e.Id == id).FirstOrDefault();
            model.Class = clss;

            //  Load Dance Styles
            var styles = new List<DanceStyleListItem>();
            foreach (DanceStyle s in DataContext.DanceStyles)
            {
                styles.Add(new DanceStyleListItem { Id = s.Id, Name = s.Name });
            }
            model.AvailableStyles = styles.OrderBy(x => x.Name);
            //  Load Dance Styles

            model.Teachers = DataContext.Teachers.Include("ApplicationUser").Where(t => !t.Classes.Any(c => c.Id == model.Class.Id)).ToList();

            //  Search based on location
            if (model.Location != null)
            {
                var address = Geolocation.ParseAddress(model.Location);
                var NELat = address.Latitude + .5;
                var SWLat = address.Latitude - .5;
                var NELng = address.Longitude + .5;
                var SWLng = address.Longitude - .5;
                model.Teachers = model.Teachers.Where(t => t.ApplicationUser.Longitude >= SWLng && t.ApplicationUser.Longitude <= NELng && t.ApplicationUser.Latitude >= SWLat && t.ApplicationUser.Latitude <= NELat).ToList();
            }

            //  Search By Last Name
            if (model.LastName != null)
            {
                model.Teachers = model.Teachers.Where(t => t.ApplicationUser.LastName.Contains(model.LastName)).ToList();
            }

            //  Search By First Name
            if (model.LastName != null)
            {
                model.Teachers = model.Teachers.Where(t => t.ApplicationUser.FirstName.Contains(model.FirstName)).ToList();
            }

            return View(model);
        }

        [Authorize]
        [HttpPost]
        public ActionResult AddTeacher(AddTeacherViewModel model)
        {
            var clss = DataContext.Events.OfType<Class>().Where(e => e.Id == model.Class.Id).FirstOrDefault();
            model.Class = clss;
            //  Load Dance Styles
            var styles = new List<DanceStyleListItem>();
            foreach (DanceStyle s in DataContext.DanceStyles)
            {
                styles.Add(new DanceStyleListItem { Id = s.Id, Name = s.Name });
            }
            model.AvailableStyles = styles.OrderBy(x => x.Name);
            if (model.PostedStyles != null)
            {
                model.SelectedStyles = model.AvailableStyles.Where(s => model.PostedStyles.DanceStyleIds.Contains(s.Id.ToString()));
            }
            //  Load Dance Styles

            model.Teachers = DataContext.Teachers.Include("ApplicationUser").Where(t => !t.Classes.Any(c => c.Id == model.Class.Id)).ToList();

            //  Search based on location
            if (model.Location != null)
            {
                var address = Geolocation.ParseAddress(model.Location);
                var NELat = address.Latitude + .5;
                var SWLat = address.Latitude - .5;
                var NELng = address.Longitude + .5;
                var SWLng = address.Longitude - .5;
                model.Teachers = model.Teachers.Where(t => t.ApplicationUser.Longitude >= SWLng && t.ApplicationUser.Longitude <= NELng && t.ApplicationUser.Latitude >= SWLat && t.ApplicationUser.Latitude <= NELat).ToList();
            }

            //  Search By First Name
            if (model.FirstName != null)
            {
                model.Teachers = model.Teachers.Where(t => t.ApplicationUser.FirstName.IndexOf(model.FirstName, StringComparison.CurrentCultureIgnoreCase) != -1).ToList();
            }

            //  Search By Last Name
            if (model.LastName != null)
            {
                model.Teachers = model.Teachers.Where(t => t.ApplicationUser.LastName.IndexOf(model.LastName, StringComparison.CurrentCultureIgnoreCase) != -1).ToList();
            }

            //  Search By Dance Styles
            if (model.PostedStyles != null)
            {
                model.Teachers = model.Teachers.Where(t => t.DanceStyles.Any(s => model.PostedStyles.DanceStyleIds.Contains(s.Id.ToString()))).ToList();
            }

            return View(model);
        }

        [Authorize]
        public ActionResult SaveTeacher(int id, int teacherId)
        {
            var clss = DataContext.Events.OfType<Class>().Where(c => c.Id == id).Include("Teachers").FirstOrDefault();

            if (clss != null)
            {
                if (!clss.Teachers.Any(t => t.Id == teacherId))
                {
                    clss.Teachers.Add(DataContext.Teachers.Where(t => t.Id == teacherId).FirstOrDefault());
                    DataContext.SaveChanges();
                }
            }

            return RedirectToAction("View", "Event", new { id = id, eventType = EventType.Class });
        }

        [Authorize]
        public ActionResult RemoveFromTeachers(int id)
        {
            var userid = User.Identity.GetUserId();
            var clss = DataContext.Events.OfType<Class>().Where(c => c.Id == id).Include("Teachers").FirstOrDefault();

            if (clss != null)
            {
                clss.Teachers.Remove(DataContext.Teachers.Where(t => t.ApplicationUser.Id == userid).FirstOrDefault());
                DataContext.SaveChanges();
            }

            return RedirectToAction("View", "Event", new { id = id, eventType = EventType.Class });
        }

        [Authorize]
        public ActionResult EditFees(int id)
        {
            var model = new EventFeeViewModel();
            model.Event = DataContext.Events.Where(e => e.Id == id).Include("EventTickets").Include("EventTickets.Ticket").FirstOrDefault();
            model.SchoolId = DataContext.Classes.Where(c => c.Id == id).FirstOrDefault().SchoolId;

            return View(model);
        }

        [Authorize]
        [HttpPost]
        public ActionResult EditFees(Event evnt)
        {
            return RedirectToAction("View", "Event", new { id = evnt.Id, eventType = (evnt is Class ? EventType.Class : EventType.Social) });
        }

        //
        // POST: /Checkout/AddressAndPayment
        [HttpPost]
        public ActionResult AddFee(FormCollection values)
        {
            var ticket = new Ticket();

            var eid = Convert.ToInt32(values["Event.Id"]);
            var evnt = DataContext.Events.Where(e => e.Id == eid).Include("EventTickets").FirstOrDefault();

            try
            {
                ticket.Price = Convert.ToDecimal(values["Fee"]);
                ticket.Quantity = Convert.ToDecimal(values["Quantity"]);
                ticket.SchoolId = Convert.ToInt32(values["SchoolId"]);

                evnt.EventTickets.Add(new EventTicket() { Ticket = ticket });

                DataContext.Entry(evnt).State = EntityState.Modified;
                DataContext.SaveChanges();

                return RedirectToAction("EditFees", "Event", new { id = evnt.Id, eventType = (evnt is Class ? EventType.Class : EventType.Social) });
            }
            catch
            {
                //Invalid - redisplay with errors
                return View(ticket);
            }
        }
        
        //public Class ConvertToClass(Event event1)
        //{
        //    var class1 = new Class()
        //    {
        //        Name = event1.Name,
        //        Description = event1.Description,
        //        FacebookLink = event1.FacebookLink,
        //        StartDate = event1.StartDate,
        //        EndDate = event1.EndDate,
        //        Price = event1.Price,
        //        IsAvailable = event1.IsAvailable,
        //        Recurring = event1.Recurring,
        //        Frequency = event1.Frequency,
        //        Interval = event1.Interval,
        //        Duration = event1.Duration,
        //        Place = event1.Place,
        //        Teachers = new List<Teacher>(),
        //        DanceStyles = event1.DanceStyles
        //    };

        //    return class1;
        //}

        //public Social ConvertToSocial(Event event1)
        //{
        //    var social = new Social()
        //    {
        //        Name = event1.Name,
        //        Description = event1.Description,
        //        FacebookLink = event1.FacebookLink,
        //        StartDate = event1.StartDate,
        //        EndDate = event1.EndDate,
        //        Price = event1.Price,
        //        IsAvailable = event1.IsAvailable,
        //        Recurring = event1.Recurring,
        //        Frequency = event1.Frequency,
        //        Interval = event1.Interval,
        //        Duration = event1.Duration,
        //        Place = event1.Place,
        //        DanceStyles = event1.DanceStyles,
        //        Promoters = new List<Promoter>()
        //    };

        //    return social;
        //}

        //public Rehearsal ConvertToRehearsal(Event event1)
        //{
        //    var rehearsal = new Rehearsal()
        //    {
        //        Name = event1.Name,
        //        Description = event1.Description,
        //        FacebookLink = event1.FacebookLink,
        //        StartDate = event1.StartDate,
        //        EndDate = event1.EndDate,
        //        Price = event1.Price,
        //        IsAvailable = event1.IsAvailable,
        //        Recurring = event1.Recurring,
        //        Frequency = event1.Frequency,
        //        Interval = event1.Interval,
        //        Duration = event1.Duration,
        //        Place = event1.Place,
        //        DanceStyles = event1.DanceStyles
        //    };

        //    return rehearsal;
        //}
    }
}
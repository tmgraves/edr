﻿using EDR.Models.ViewModels;
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
using System.Globalization;
using System.IO;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace EDR.Controllers
{
    public class EventController : BaseController
    {
        //[Route("Classes/{Location}")]
        //[Route("Classes")]
        public ActionResult Classes(LearnViewModel model)
        {
            SearchClasses(model);
            model.Styles = DataContext.DanceStyles.Select(s => s.Name).ToArray();
            if (HttpContext.Request.Browser.IsMobileDevice)
            {
                return View("Mobile/Classes", model);
            }
            else
            {
                return View(model);
            }
        }

        private void SearchClasses(LearnViewModel model)
        {
            model.Classes = DataContext.Classes
                                .Include("Teachers.ApplicationUser")
                                .Include("DanceStyles")
                                .Include("Place")
                                .Include("EventMembers.Member")
                                .Include("Reviews")
                                .Include("EventInstances")
                                .Where(c => c.EventInstances.Any(i => i.DateTime >= DateTime.Today))
                                .AsQueryable();
            if (model.Style != null)
            {
                var style = DataContext.DanceStyles.Where(s => s.Name == model.Style).FirstOrDefault();
                if (style != null)
                {
                    model.DanceStyleId = style.Id;
                }
            }

            if (model.DanceStyleId != null)
            {
                model.Classes = model.Classes.Where(c => c.DanceStyles.Select(st => st.Id).Contains((int)model.DanceStyleId));
            }
            if (model.TeacherId != null && model.TeacherId != "")
            {
                model.Classes = model.Classes.Where(c => c.Teachers.Select(t => t.ApplicationUser.Id).Contains(model.TeacherId));
            }
            if (model.Location != null)
            {
                var add = Geolocation.ParseAddress(model.Location);
                model.Classes = model.Classes.Where(e => (e.Place.Longitude >= add.Longitude - .5 && e.Place.Longitude <= add.Longitude + .5) && (e.Place.Latitude >= add.Latitude - .5 && e.Place.Latitude <= add.Latitude + .5)).ToList();
            }
            if (model.NELat != null && model.SWLng != null)
            {
                model.Classes = model.Classes.Where(c => c.Place.Longitude >= model.SWLng && c.Place.Longitude <= model.NELng && c.Place.Latitude >= model.SWLat && c.Place.Latitude <= model.NELat);
            }
            if (model.SkillLevel != null)
            {
                model.Classes = model.Classes.Where(x => model.SkillLevel.Contains((int)x.SkillLevel));
            }
            if (model.Days != null)
            {
                model.Classes = model.Classes.Where(x => model.Days.Contains(x.Day));
            }
            if (model.SchoolId != null)
            {
                model.Classes = model.Classes.Where(c => c.SchoolId == model.SchoolId);
            }

            model.Classes = model.Classes.ToList().Take(100);
        }

        //[Route("Events")]
        //[Route("Events/{Location}")]
        public ActionResult Socials(SocialViewModel model)
        {
            SearchSocials(model);
            model.Styles = DataContext.DanceStyles.Select(s => s.Name).ToArray();
            if (HttpContext.Request.Browser.IsMobileDevice)
            {
                return View("Mobile/Socials", model);
            }
            else
            {
                return View(model);
            }
        }

        private void SearchSocials(SocialViewModel model)
        {
            model.Socials = DataContext.Socials
                                .Include("DanceStyles")
                                .Include("Place")
                                .Include("EventMembers.Member")
                                .Include("Reviews")
                                .Include("EventInstances")
                                .Where(c => c.EventInstances.Any(i => i.DateTime >= DateTime.Today))
                                .AsQueryable();
            if (model.Style != null)
            {
                var style = DataContext.DanceStyles.Where(s => s.Name == model.Style).FirstOrDefault();
                if (style != null)
                {
                    model.DanceStyleId = style.Id;
                }
            }

            if (model.DanceStyleId != null)
            {
                model.Socials = model.Socials.Where(c => c.DanceStyles.Select(st => st.Id).Contains((int)model.DanceStyleId));
            }

            if (model.NELat != null && model.SWLng != null)
            {
                model.Socials = model.Socials.Where(c => c.Place.Longitude >= model.SWLng && c.Place.Longitude <= model.NELng && c.Place.Latitude >= model.SWLat && c.Place.Latitude <= model.NELat);
            }
            if (model.Days != null)
            {
                model.Socials = model.Socials.Where(x => model.Days.Contains(x.Day));
            }

            model.Socials = model.Socials.ToList().Take(100);
        }

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

        [Route("Class/{id:int}/{eventname?}/{location?}")]
        public ActionResult Class(int? id, int? instanceId)
        {
            if (!id.HasValue)
            {
                return RedirectToAction("Classes", "Event", null);
            }
            var userid = User.Identity.GetUserId();

            var model = LoadEvent((int)id, EventType.Class, instanceId);

            if (model.Event == null)
            {
                return RedirectToAction("Classes", "Event", null);
            }

            if (User.Identity.IsAuthenticated)
            {
                model.Review = model.Event.Reviews.Where(r => r.Author.Id == userid).FirstOrDefault();
            }
            else
            {
                model.Review = new Review();
            }

            if (model.Event == null)
            {
                return HttpNotFound();
            }

            if (HttpContext.Request.Browser.IsMobileDevice)
            {
                return View("Mobile/View", model);
            }
            else
            {
                return View("View", model);
            }
        }

        [Route("Social/{id:int}/{eventname?}/{location?}")]
        public ActionResult Social(int? id, int? instanceId)
        {
            if (!id.HasValue)
            {
                return RedirectToAction("Socials", "Event", null);
            }
            var userid = User.Identity.GetUserId();

            var model = LoadEvent((int)id, EventType.Social, instanceId);

            if (model.Event == null)
            {
                return RedirectToAction("Socials", "Event", null);
            }

            if (User.Identity.IsAuthenticated)
            {
                model.Review = model.Event.Reviews.Where(r => r.Author.Id == userid).FirstOrDefault();
            }
            else
            {
                model.Review = new Review();
            }

            if (model.Event == null)
            {
                return HttpNotFound();
            }

            if (HttpContext.Request.Browser.IsMobileDevice)
            {
                return View("Mobile/View", model);
            }
            else
            {
                return View("View", model);
            }
        }

        public ActionResult View(int? id, EventType eventType, int? instanceId)
        {
            if (!id.HasValue)
            {
                return RedirectToAction("Learn", "Home", null);
            }
            var userid = User.Identity.GetUserId();

            var model = LoadEvent((int)id, eventType, instanceId);

            if (model.Event == null)
            {
                return RedirectToAction("Learn", "Home", null);
            }

            if (User.Identity.IsAuthenticated)
            {
                model.Review = model.Event.Reviews.Where(r => r.Author.Id == userid).FirstOrDefault();
            }
            else
            {
                model.Review = new Review();
            }

            model.EventType = eventType;

            ////  Update Facebook Event with Current Info
            //if (model.Event.FacebookId != null && model.Event.UpdatedDate < DateTime.Now.AddDays(-7))
            //{
            //    UpdateFacebookEvent(model.Event);
            //}
            ////  Get Current Facebook Picture/Video

            if (model.Event == null)
            {
                return HttpNotFound();
            }

            if (HttpContext.Request.Browser.IsMobileDevice)
            {
                return View("Mobile/View", model);
            }
            else
            {
                return View(model);
            }
        }

        [Authorize]
        public ActionResult Manage(int? id, EventType eventType)
        {
            if (!id.HasValue)
            {
                ViewBag.errorMessage = "Invalid Id";
                return View("Error");
            }

            var userid = User.Identity.GetUserId();
            var user = DataContext.Users.Single(u => u.Id == userid);
            var admin = User.IsInRole("Admin");
            var model = new EventManageViewModel();
            if (eventType == EventType.Class)
            {
                var cls =
                    DataContext.Classes
                    .Include("Tickets.UserTickets.EventRegistrations")
                    .Include("EventInstances.EventRegistrations.User")
                    .Include("Pictures.PostedBy")
                    .Include("Albums.PostedBy")
                    .Include("Videos.Author")
                    .Include("Playlists.Author")
                    .Include("DanceStyles")
                    .Include("Place")
                    .Include("LinkedMedia")
                    .SingleOrDefault(e => e.Id == id && (e.Teachers.Any(t => t.ApplicationUser.Id == userid) || e.Owners.Any(t => t.ApplicationUser.Id == userid) || e.School.Members.Any(m => m.UserId == userid && m.Admin) || admin));
                model.Event = cls;
                model.SkillLevel = cls.SkillLevel;
                model.ClassType = cls.ClassType;
            }
            else
            {
                var soc =
                DataContext.Socials
                        .Include("Tickets.UserTickets.EventRegistrations")
                        .Include("EventInstances.EventRegistrations.User")
                        .Include("Pictures.PostedBy")
                        .Include("Albums.PostedBy")
                        .Include("Videos.Author")
                        .Include("Playlists.Author")
                        .Include("DanceStyles")
                        .Include("Place")
                        .Include("LinkedMedia")
                        .Include("PromoterGroup")
                        .SingleOrDefault(e => e.Id == id && (e.Promoters.Any(t => t.ApplicationUser.Id == userid) || e.Owners.Any(t => t.ApplicationUser.Id == userid) || e.PromoterGroup.Members.Any(m => m.UserId == userid && m.Admin) || admin));
                model.Event = soc;
                model.MusicType = soc.MusicType;
                model.PromoterGroups = DataContext.PromoterGroups.Where(g => g.Promoters.Any(p => p.ApplicationUser.Id == userid)).ToList();
                model.SocialType = soc.SocialType;
            }

            if (model.Event == null)
            {
                ViewBag.errorMessage = "Not Authorized";
                return View("Error");
            }

            model.NewPlace = new Place();
            model.NewPlace.PlaceType = PlaceType.OtherPlace;

            //  Load Facebook Albums - Use for adding an album
            if (user.FacebookToken != null)
            {
                Session["FacebookAlbums"] = FacebookHelper.GetAlbums(user.FacebookToken);
            }
            //  Load Facebook Albums

            if (model.Event is Class)
            {
                model.SchoolId = DataContext.Classes.Single(c => c.Id == model.Event.Id).SchoolId;
            }
            else if (model.Event is Social)
            {
                model.PromoterGroupId = DataContext.Socials.Single(s => s.Id == model.Event.Id).PromoterGroupId;
            }

            model.EventType = eventType;

            if (model.Event == null)
            {
                return HttpNotFound();
            }

            return View(model);
        }

        [Authorize]
        public ActionResult GetRegistrants(int id)
        {
            var registrants = DataContext.EventRegistrations.Where(r => r.EventInstanceId == id).ToList();

            return PartialView("~/Views/Shared/_EventRegistrationsPartial.cshtml", registrants);
        }

        [Authorize(Roles = "Owner,Promoter,Teacher")]
        public ActionResult ManageInstance(int id)
        {
            var model = new EventInstanceManageViewModel();
            model.Instance = DataContext.EventInstances
                                    .Include("Event")
                                    .Include("EventRegistrations.User")
                                    .Single(s => s.Id == id);
            model.NewPlace = new Place();
            model.NewPlace.PlaceType = PlaceType.OtherPlace;

            if (model.Instance.Event == null)
            {
                return HttpNotFound();
            }

            return View(model);
        }

        [Authorize(Roles = "Owner,Promoter,Teacher")]
        public ActionResult ConfirmInstance(int id)
        {
            DataContext.EventInstances.Where(i => i.Id == id).FirstOrDefault().Confirmed = DateTime.Now;
            DataContext.SaveChanges();
            return RedirectToAction("ManageInstance", new { id = id });
        }

        [Authorize(Roles = "Owner,Promoter,Teacher")]
        public ActionResult CancelInstance(int id)
        {
            var ins = DataContext.EventInstances.Where(i => i.Id == id).FirstOrDefault();
            DataContext.EventInstances.Remove(DataContext.EventInstances.Where(i => i.Id == id).FirstOrDefault());
            DataContext.SaveChanges();
            return RedirectToAction("Manage", new { id = ins.EventId, eventType = ins.Event is Class ? EventType.Class : EventType.Social });
        }

        public JsonResult Search(string searchString)
        {
            var today = DateTime.Today;
            var events = DataContext.Events.Where(e => e is Class || e is Social).Where(e => e.EventInstances.Any(i => i.DateTime >= today) && (e.Name + " " + e.Description).Contains(searchString)).Select(s => new { Id = s.Id, NextDate = s.EventInstances.Where(i => i.DateTime >= today).Min(i => i.DateTime), Name = s.Name + " - " + s.Place.City + ", " + s.Place.State }).ToList();
            return Json(events.OrderBy(e => e.NextDate).Select(e => new { Id = e.Id, Name = e.NextDate.ToShortDateString() + ": " + e.Name }), JsonRequestBehavior.AllowGet);
        }

        private List<EventVideo> GetVideos(int id)
        {
            var evt = DataContext.Events
                            .Include("Videos.Author")
                            .Include("Playlists.Author")
                            .Single(e => e.Id == id);
            var videos = new List<EventVideo>();
            videos = evt.Videos.ToList();

            //  Extract YouTube Playlists
            if (evt.Playlists != null)
            {
                foreach (var lst in evt.Playlists)
                {
                    var ytids = videos.Select(v => v.YoutubeId).ToArray();
                    videos.AddRange(YouTubeHelper.GetPlaylistVideos(lst.YouTubeId).Where(v => !ytids.Contains(v.Id)).Select(v => new EventVideo() { Author = lst.Author, MediaSource = MediaSource.YouTube, YoutubeId = v.Id, PhotoUrl = v.Thumbnail.ToString(), PublishDate = v.PubDate, Title = v.Title, VideoUrl = v.VideoLink.ToString(), YouTubePlaylistTitle = lst.Title, YouTubePlaylistUrl = lst.YouTubeUrl, YoutubeThumbnail = v.Thumbnail.ToString(), YoutubeUrl = v.VideoLink.ToString() }));
                    //  lstMedia.AddRange(videos.Select(v => new EventMedia() { Event = evt, SourceName = v.Title, SourceLink = v.VideoLink.ToString(), MediaDate = v.PubDate, MediaType = Enums.MediaType.Video, PhotoUrl = v.Thumbnail.ToString(), MediaUrl = v.VideoLink.ToString(), Title = v.Title, MediaSource = MediaSource.YouTube, Target = target, Playlist = lst, Author = lst.Author }).ToList());
                }
            }

            return videos;
        }

        private List<EventPicture> GetPictures(int id)
        {
            var evt = DataContext.Events
                            .Include("Pictures.PostedBy")
                            .Include("Albums.PostedBy")
                            .Single(e => e.Id == id);
            var pictures = new List<EventPicture>();
            pictures = evt.Pictures.ToList();

            //  Extract Facebook Album Pictures
            if (evt.Albums != null)
            {
                foreach (var album in evt.Albums)
                {
                    var fbids = pictures.Select(p => p.FacebookId).ToArray();
                    if (album.PostedBy.FacebookToken != null)
                    {
                        pictures.AddRange(FacebookHelper.GetAlbumPhotos(album.PostedBy.FacebookToken, album.FacebookId).Where(p => !fbids.Contains(p.Id)).Select(p => new EventPicture() { Album = album, Title = p.Name, PhotoDate = p.PhotoDate, Filename = p.LargeSource, ThumbnailFilename = p.Source, MediaSource = MediaSource.Facebook }).ToList());
                    }
                }
            }

            return pictures;
        }

        public JsonResult GetPicturesJSON(int id)
        {
            //  Get Linked Objects Pictures
            //  Get Facebook Group/Event/Page Feed Pictures

            //  Get Instagram Post Pictures
            var pictures = GetPictures(id);
            return Json(pictures.Select(p => new { FileName = p.Filename, Title = p.Title, ThumbnailFilename = p.ThumbnailFilename }), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetSchoolJSON(int id)
        {
            //  Get Instagram Post Pictures
            var school = DataContext.Schools.Include("Tickets").Single(s => s.Id == id);
            return Json(new { school.Id, school.Name, HasTickets=school.Tickets.Count() > 0 }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetVideosJSON(int id)
        {
            //  Get Linked Objects Videos
            //  Get Facebook Group/Event/Page Feed Videos

            //  Get YouTube Playlist Videos
            //  Add Videos
            var videos = GetVideos(id);
            return Json(videos.OrderByDescending(v => v.PublishDate).Select(v => new { PhotoUrl = v.PhotoUrl, VideoUrl = v.VideoUrl, Title = v.Title, Id = v.YoutubeId }), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetFeedJSON(int id)
        {
            var evt = DataContext.Events.Include("LinkedMedia").Single(e => e.Id == id);
            var evtfeed = new List<Feed>();

            foreach (var o in evt.LinkedMedia)
            {
                var feed = FacebookHelper.GetFeed(o.FacebookId, FacebookHelper.GetGlobalToken());
                if (feed != null)
                {
                    evtfeed.AddRange(feed.Select(ff => new Feed() { Link = ff.Link, Message = ff.Message, PhotoUrl = ff.Picture, UpdateTime = ff.Updated_Time, Type = (ff.Type == "video" ? MediaType.Video : (ff.Type == "picture" ? MediaType.Picture : MediaType.Comment)) }).ToList());
                }
            }

            //  model.LinkedFacebookObjects = DataContext.Events.Where(e => e.Id == id).Include("LinkedFacebookObjects").FirstOrDefault().LinkedFacebookObjects;

            return Json(evtfeed.Where(f => f.Link != null || f.Message != null).OrderByDescending(d => d.UpdateTime).Select(f => new { UpdateTime = f.UpdateTime.ToShortDateString(), Link = f.Link, Message = f.Message, PhotoUrl = f.PhotoUrl, FeedType = f.Type }), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetReviewsJSON(int id)
        {
            var evt = DataContext.Events.Include("Reviews").Single(e => e.Id == id);
            var evtreviews = new List<Review>();

            //  model.LinkedFacebookObjects = DataContext.Events.Where(e => e.Id == id).Include("LinkedFacebookObjects").FirstOrDefault().LinkedFacebookObjects;

            return Json(evt.Reviews.OrderByDescending(r => r.ReviewDate).Select(r => new { ReviewDate = r.ReviewDate.ToShortDateString(), ReviewText = r.ReviewText, FirstName = r.Author.FirstName, Rating = r.Rating }), JsonRequestBehavior.AllowGet);
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

        [Authorize]
        [HttpPost]
        public JsonResult UploadImageAsync(string imageData, int id)
        {
            var newFile = new UploadFile();
            if (string.IsNullOrEmpty(imageData))
                newFile.UploadStatus = "Failed";

            Match imageMatch = Regex.Match(imageData, @"^data:(?<mimetype>[^;]+);base64,(?<data>.+)$");
            if (!imageMatch.Success)
                newFile.UploadStatus = "Failed";

            string mimeType = imageMatch.Groups["mimetype"].Value;
            Match imageType = Regex.Match(mimeType, @"^[^/]+/(?<type>.+?)$");
            if (!imageType.Success)
                newFile.UploadStatus = "Failed";

            string fileExtension = imageType.Groups["type"].Value;
            byte[] data2 = Convert.FromBase64String(imageMatch.Groups["data"].Value);

            if (newFile.UploadStatus != "Failed")
            {
                newFile = EDR.Utilities.ApplicationUtility.UploadFromPath(imageData);
                if (newFile.UploadStatus == "Success")
                {
                    var evt = DataContext.Events.Single(s => s.Id == id);
                    EDR.Utilities.ApplicationUtility.DeletePicture(new Picture() { Filename = evt.PhotoUrl });
                    evt.PhotoUrl = newFile.FilePath;
                    DataContext.SaveChanges();
                }
            }
            var objUpload = new { FilePath = Url.Content(newFile.FilePath), UploadStatus = newFile.UploadStatus };
            return Json(objUpload, JsonRequestBehavior.AllowGet);
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

        private EventViewModel LoadEvent(int id, EventType eventType, int? instanceId = null)
        {
            var userid = User.Identity.GetUserId();
            var model = new EventViewModel();
            model.EventType = eventType;
            //  var evq = DataContext.Set<Event>().SqlQuery("usp_Events_Search @Id = {0}, @UserId = {1}", id, userid);
            //var eins = 
            //        (from e in (from e in DataContext.Events
            //        join i in DataContext.EventInstances
            //        on e.Id equals i.EventId
            //        join r in DataContext.EventRegistrations
            //         on new { InstanceId = i.Id, UserId = userid } equals new { InstanceId = r.EventInstanceId, r.UserId } into res
            //        from reg in res.DefaultIfEmpty()
            //        join s in DataContext.DanceStyles
            //        on new { id = 0 } equals new { id = 0 }
            //        where e.Id == id
            //        && e.DanceStyles.Select(s => s.Id).Contains(s.Id)
            //        select new { Event = e, EventInstances = i, EventRegsitrations = reg, DanceStyles = s })
            //        select e.Event)
            //        .Include("Place");

            //var qr = (from e in DataContext.Events
            //        join i in eins
            //        on e.Id equals i.EventId
            //        where e.Id == id
            //        select e)
            //        .Include("Place")
            //        .Include("DanceStyles")
            //        .Include("Reviews")
            //        .Include("EventMembers")
            //        .Include("EventMembers.Member")
            //        .Include("Creator")
            //        .Include("Tickets")
            //        .FirstOrDefault();

            //model.Event =
            //        (from e in DataContext.Events
            //         join ei in DataContext.EventInstances
            //         on e.Id equals ei.Event.Id
            //         join er in DataContext.EventRegistrations
            //         on new { EventInstanceId = ei.Id, UserId = userid } equals new { EventInstanceId = er.EventInstanceId, UserId = er.UserId }
            //         into _events
            //         from ev in _events.DefaultIfEmpty()
            //         where e.Id == id
            //         select e)
            //        .Include("Place")
            //        .Include("DanceStyles")
            //        .Include("Reviews")
            //        .Include("EventMembers")
            //        .Include("EventMembers.Member")
            //        .Include("Creator")
            //        .Include("Tickets")
            //        .FirstOrDefault();
            //DataContext.Events.Where(x => x.Id == id)
            //.Include("Place")
            //.Include("DanceStyles")
            //.Include("Reviews")
            //.Include("EventMembers")
            //.Include("EventMembers.Member")
            //.Include("EventInstances")
            //.Include("Creator")
            //.Include("Tickets")
            //.Select(e => new Event
            //{
            //    Name = e.Name,
            //    Description = e.Description,

            //    EventInstances = e.EventInstances
            //                        .Select(i =>
            //                            new EventInstance()
            //                            {
            //                                DateTime = i.DateTime,
            //                                EventId = i.EventId,
            //                                EventRegistrations = i.EventRegistrations.Where(r => r.UserId == userid).ToList()
            //                            }
            //                            ).ToList()

            //})
            //.FirstOrDefault();

            if (eventType == EventType.Class)
            {
                model.Event =
                DataContext.Classes.Where(x => x.Id == id)
                .Include("DanceStyles")
                .Include("Reviews")
                .Include("EventMembers")
                .Include("EventMembers.Member")
                .Include("EventInstances")
                .Include("EventInstances.EventRegistrations")
                //.Include("EventInstances.EventRegistrations.User")
                //.Include("EventInstances.EventRegistrations.UserTicket.Ticket")
                .Include("Tickets")
                .Include("LinkedMedia")
                .Include("School.Members")
                .FirstOrDefault();
            }
            else
            {
                model.Event =
                DataContext.Socials.Where(x => x.Id == id)
                .Include("DanceStyles")
                .Include("Reviews")
                .Include("EventMembers")
                .Include("EventMembers.Member")
                .Include("EventInstances")
                .Include("EventInstances.EventRegistrations")
                //.Include("EventInstances.EventRegistrations.User")
                //.Include("EventInstances.EventRegistrations.UserTicket.Ticket")
                .Include("Tickets")
                .Include("LinkedMedia")
                .Include("PromoterGroup.Members")
                .FirstOrDefault();
            }
            if (instanceId != null)
            {
                model.CurrentInstance = model.Event.EventInstances.Where(i => i.Id == instanceId).FirstOrDefault();
            }

            if (model.CurrentInstance == null)
            {
                model.CurrentInstance = model.Event.EventInstances.Where(i => i.DateTime >= DateTime.Today).OrderBy(i => i.DateTime).FirstOrDefault();
                if (model.CurrentInstance == null)
                {
                    model.CurrentInstance = model.Event.EventInstances.OrderByDescending(i => i.DateTime).Take(1).FirstOrDefault();
                }
            }

            ////  Get Tickets
            //if (model.Event.Tickets.Count() != 0)
            //{
            //    model.Tickets = model.Event.Tickets;
            //}
            //else
            //{
            //    model.Tickets = 
            //            from t in DataContext.Tickets
            //            join c in DataContext.Classes
            //            on t.SchoolId equals c.SchoolId
            //            where c.Id == id
            //            select t;
            //}

            if (!model.Event.Free)
            {
                model.Tickets = new List<Ticket>();
                if (model.CurrentInstance != null)
                {
                    //  Event Tickets
                    if (model.Event.Tickets.Count() != 0)
                    {
                        //  Fill Tickets
                        model.Tickets.AddRange(model.Event.Tickets);

                        var tix = DataContext.UserTickets
                                .Include("EventRegistrations")
                                .Where(ut =>
                                ut.UserId == userid
                                && ut.Ticket.EventId == model.CurrentInstance.EventId
                                && ut.Quantity * ut.Ticket.Quantity > ut.EventRegistrations.Count()
                                && ut.Quantity > ut.EventRegistrations.Where(r => r.EventInstanceId == model.CurrentInstance.Id).Count())
                                .Select(ut =>
                                new
                                {
                                    ut.Id,
                                    ut.TicketId,
                                    AmountPurchased =
                                            ut.Quantity,
                                    NumInstances =
                                            ut.Ticket.Quantity,
                                    TotalInstances =
                                            ut.Quantity * ut.Ticket.Quantity,
                                    EventAvail =
                                            ut.Ticket.Quantity * ut.Quantity - ut.EventRegistrations.Count(),
                                    EventUsed =
                                            ut.EventRegistrations.Count(),
                                    ut.Quantity,
                                    InstancesUsed =
                                            ut.EventRegistrations.Where(r => r.EventInstanceId == model.CurrentInstance.Id).Count(),
                                    InstanceAvail =
                                            ut.Quantity - ut.EventRegistrations.Where(r => r.EventInstanceId == model.CurrentInstance.Id).Count()
                                }).ToList();

                        if (tix != null)
                        {
                            model.AvailableTickets = tix.Sum(ut => (ut.InstanceAvail < ut.EventAvail ? ut.InstanceAvail : ut.EventAvail));
                        }

                        //var tix1 =
                        //    from i in DataContext.EventInstances
                        //    join t in DataContext.Tickets
                        //    on i.EventId equals t.EventId
                        //    join ut in (from ut in DataContext.UserTickets
                        //                where ut.Quantity > ut.EventRegistrations.Where(r => r.EventInstanceId == model.CurrentInstance.Id).Count()
                        //                && ut.UserId == userid
                        //                && ut.Ticket.Quantity > ut.EventRegistrations.Count()
                        //                select new
                        //                {
                        //                    ut.Id,
                        //                    ut.TicketId,
                        //                    AmountPurchased =
                        //                    ut.Quantity,
                        //                    NumInstances =
                        //                    ut.Ticket.Quantity,
                        //                    TotalInstances =
                        //                    ut.Quantity * ut.Ticket.Quantity,
                        //                    EventAvail =
                        //                    ut.Ticket.Quantity * ut.Quantity - ut.EventRegistrations.Count(),
                        //                    EventUsed =
                        //                    ut.EventRegistrations.Count(),
                        //                    ut.Quantity,
                        //                    InstanceAvail =
                        //                    ut.Quantity - ut.EventRegistrations.Where(r => r.EventInstanceId == model.CurrentInstance.Id).Count()
                        //                })
                        //    on t.Id equals ut.TicketId
                        //    where i.Id == model.CurrentInstance.Id
                        //    select new
                        //    {
                        //        i.Id,
                        //        i.DateTime,
                        //        TicketId = t.Id,
                        //        t.Description,
                        //        UserTickets =
                        //        ut
                        //    };

                        //var usertickets =
                        //        from ut in DataContext.UserTickets
                        //        join t in DataContext.Tickets
                        //        on ut.TicketId equals t.Id
                        //        where t.EventId == id
                        //        && ut.UserId == userid
                        //        select ut.Quantity * t.Quantity;

                        //var used =
                        //        (from evi in DataContext.EventInstances
                        //         join er in DataContext.EventRegistrations
                        //         on evi.Id equals er.EventInstanceId
                        //         where er.UserId == userid
                        //         && evi.EventId == id
                        //         select er.Id).Distinct();

                        //  model.AvailableTickets = Convert.ToInt32((usertickets.Count() != 0 ? usertickets.Sum() : 0) - (used.Count() != 0 ? used.Count() : 0));
                        //  model.AvailableTickets = tix.Any(t => t.UserTickets != null) ? Convert.ToInt32(tix.Sum(t => t.UserTickets.InstanceAvail)) : 0;
                    }
                    else
                    {
                        //  Fill Tickets
                        model.Tickets.AddRange((model.Event as Class).School.Tickets);

                        var tix = DataContext.UserTickets
                                .Include("EventRegistrations")
                                .Where(ut =>
                                ut.UserId == userid
                                && ut.Ticket.SchoolId == DataContext.Classes.Where(c => c.Id == model.CurrentInstance.EventId).FirstOrDefault().SchoolId
                                && ut.Quantity * ut.Ticket.Quantity > ut.EventRegistrations.Count()
                                && ut.Quantity > ut.EventRegistrations.Where(r => r.EventInstanceId == model.CurrentInstance.Id).Count())
                                .Select(ut =>
                                new
                                {
                                    ut.Id,
                                    ut.TicketId,
                                    AmountPurchased =
                                            ut.Quantity,
                                    NumInstances =
                                            ut.Ticket.Quantity,
                                    TotalInstances =
                                            ut.Quantity * ut.Ticket.Quantity,
                                    EventAvail =
                                            ut.Ticket.Quantity * ut.Quantity - ut.EventRegistrations.Count(),
                                    EventUsed =
                                            ut.EventRegistrations.Count(),
                                    ut.Quantity,
                                    InstancesUsed =
                                            ut.EventRegistrations.Where(r => r.EventInstanceId == model.CurrentInstance.Id).Count(),
                                    InstanceAvail =
                                            ut.Quantity - ut.EventRegistrations.Where(r => r.EventInstanceId == model.CurrentInstance.Id).Count()
                                }).ToList();

                        if (tix != null)
                        {
                            model.AvailableTickets = tix.Sum(ut => (ut.InstanceAvail < ut.EventAvail ? ut.InstanceAvail : ut.EventAvail));
                        }
                        //  model.AvailableTickets = tix.Sum(ut => ut.Quantity - ut.EventRegistrations.Where(r => r.EventInstanceId == model.CurrentInstance.Id).Count());

                        //var tix =
                        //    from i in DataContext.EventInstances
                        //    join c in DataContext.Classes
                        //    on i.EventId equals c.Id
                        //    join t in DataContext.Tickets
                        //    on c.SchoolId equals t.SchoolId
                        //    join ut in (from ut in DataContext.UserTickets
                        //                where ut.Quantity > ut.EventRegistrations.Where(r => r.EventInstanceId == model.CurrentInstance.Id).Count()
                        //                && ut.UserId == userid
                        //                && ut.Ticket.Quantity > ut.EventRegistrations.Count()
                        //                select new
                        //                {
                        //                    ut.Id,
                        //                    ut.TicketId,
                        //                    AmountPurchased =
                        //                    ut.Quantity,
                        //                    NumInstances =
                        //                    ut.Ticket.Quantity,
                        //                    TotalInstances =
                        //                    ut.Quantity * ut.Ticket.Quantity,
                        //                    EventAvail =
                        //                    ut.Ticket.Quantity * ut.Quantity - ut.EventRegistrations.Count(),
                        //                    EventUsed =
                        //                    ut.EventRegistrations.Count(),
                        //                    ut.Quantity,
                        //                    InstanceAvail =
                        //                    ut.Quantity - ut.EventRegistrations.Where(r => r.EventInstanceId == model.CurrentInstance.Id).Count()
                        //                })
                        //    on t.Id equals ut.TicketId
                        //    where i.Id == model.CurrentInstance.Id
                        //    select new
                        //    {
                        //        i.Id,
                        //        i.DateTime,
                        //        TicketId = t.Id,
                        //        t.Description,
                        //        UserTickets =
                        //        ut
                        //    };

                        //model.AvailableTickets = tix.Any(t => t.UserTickets != null) ? Convert.ToInt32(tix.Sum(t => t.UserTickets.InstanceAvail)) : 0;

                        //var usertickets =
                        //        from ut in DataContext.UserTickets
                        //        join t in DataContext.Tickets
                        //        on ut.TicketId equals t.Id
                        //        where t.EventId == id
                        //        && ut.UserId == userid
                        //        select ut.Quantity * t.Quantity;

                        //var used =
                        //        (from evi in DataContext.EventInstances
                        //         join er in DataContext.EventRegistrations
                        //         on evi.Id equals er.EventInstanceId
                        //         where er.UserId == userid
                        //         && evi.EventId == id
                        //         select er.Id).Distinct();

                        //  model.AvailableTickets = Convert.ToInt32((usertickets.Count() != 0 ? usertickets.Sum() : 0) - (used.Count() != 0 ? used.Count() : 0));

                        //var usertickets =
                        //        from ut in DataContext.UserTickets
                        //        join t in DataContext.Tickets
                        //        on ut.TicketId equals t.Id
                        //        join c in DataContext.Classes
                        //        on t.SchoolId equals c.SchoolId
                        //        where c.Id == id
                        //        && ut.UserId == userid
                        //        select ut.Quantity * t.Quantity;

                        //var used =
                        //        (from er in DataContext.EventRegistrations
                        //         join ei in DataContext.EventInstances
                        //         on er.EventInstanceId equals ei.Id
                        //         join ut in DataContext.UserTickets
                        //         on new { UserTicketId = (int)er.UserTicketId, er.UserId } equals new { UserTicketId = ut.Id, UserId = ut.UserId }
                        //         join t in DataContext.Tickets
                        //         on ut.TicketId equals t.Id
                        //         join c in DataContext.Classes
                        //         on t.SchoolId equals c.SchoolId
                        //         where ut.UserId == userid
                        //         && c.Id == id
                        //         select er.Id).Distinct();

                        //model.AvailableTickets = Convert.ToInt32((usertickets.Count() != 0 ? usertickets.Sum() : 0) - (used.Count() != 0 ? used.Count() : 0));
                    }
                }
            }

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

            ////  Add Videos
            //model.Event.Videos = GetVideos(model.Event.Id);

            ////  Add Pictures
            //model.Event.Pictures = GetPictures(model.Event.Id);
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

        //[Authorize]
        //public ActionResult DeleteAlbum(int albumId, string returnUrl)
        //{
        //    var album = DataContext.Albums.Find(albumId);
        //    DataContext.Albums.Remove(album);
        //    //  DataContext.Entry(album).State = EntityState.Deleted;
        //    DataContext.SaveChanges();
        //    return Redirect(returnUrl);
        //}

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
            var ePlaylist = new EventPlaylist() { Title = ytPlaylist.Name, PublishDate = ytPlaylist.PubDate, YouTubeId = ytPlaylist.Id, Author = auth, YouTubeUrl = ytPlaylist.Url.ToString(), CoverPhoto = ytPlaylist.ThumbnailUrl.ToString(), MediaSource = MediaSource.YouTube };

            ev.Playlists.Add(ePlaylist);
            DataContext.Entry(ev).State = EntityState.Modified;
            DataContext.SaveChanges();
            ViewBag.Message = "Playlist was imported";
            return Redirect(returnUrl);
        }
        //[Authorize]
        //public ActionResult ImportYouTubePlaylistLink(string playlistUrl, int eventId, string returnUrl)
        //{
        //    var ytPlaylist = YouTubeHelper.GetPlaylist(playlistUrl);
        //    var userid = User.Identity.GetUserId();
        //    var auth = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();
        //    var ev = DataContext.Events.Where(e => e.Id == eventId).Include("Playlists").FirstOrDefault();
        //    var ePlaylist = new EventPlaylist() { Title = ytPlaylist.Name, PublishDate = ytPlaylist.PubDate, YouTubeId = ytPlaylist.Id, Author = auth, YouTubeUrl = ytPlaylist.Url, CoverPhoto = ytPlaylist.ThumbnailUrl, MediaSource = MediaSource.YouTube };

        //    ev.Playlists.Add(ePlaylist);
        //    DataContext.Entry(ev).State = EntityState.Modified;
        //    DataContext.SaveChanges();
        //    ViewBag.Message = "Playlist was imported";
        //    return Redirect(returnUrl);
        //}
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

        //[Authorize]
        //public ActionResult DeleteVideo(int videoId, string returnUrl)
        //{
        //    DataContext.Videos.Remove(DataContext.Videos.Where(v => v.Id == videoId).FirstOrDefault());
        //    DataContext.SaveChanges();
        //    ViewBag.Message = "Video was deleted";
        //    return Redirect(returnUrl);
        //}
        //[Authorize]
        //public ActionResult DeletePlaylist(int listId, string returnUrl)
        //{
        //    DataContext.Playlists.Remove(DataContext.Playlists.Where(l => l.Id == listId).FirstOrDefault());
        //    DataContext.SaveChanges();
        //    ViewBag.Message = "Playlist was removed";
        //    return Redirect(returnUrl);
        //}
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
        public JsonResult RegisterJSON(int id)
        {
            var userid = User.Identity.GetUserId();
            var einstance = DataContext.EventInstances.Where(i => i.Id == id).FirstOrDefault();

            //  Event Tickets
            if (einstance.Event.Free)
            {
                try
                {
                    RegisterDancer(userid, id, null, "", "");
                    //DataContext.EventRegistrations.Add(new EventRegistration() { UserId = userid, EventInstanceId = id });
                    //DataContext.SaveChanges();
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
                return Json("sucess", JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("failure", JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize]
        public JsonResult UnRegisterJSON(int id)
        {
            var userid = User.Identity.GetUserId();
            var reg = DataContext.EventRegistrations.Include("Instance.Event").Where(r => r.EventInstanceId == id && r.UserId == userid).FirstOrDefault();

            //  Event Tickets
            if (reg.Instance.Event.Free)
            {
                try
                {
                    DataContext.EventRegistrations.Remove(reg);
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
                return Json("sucess", JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("failure", JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Register(int id)
        {
            var userid = "";
            if (User.Identity.IsAuthenticated)
            {
                userid = User.Identity.GetUserId();
            }

            //  CHECK TO REMOVE SINCE ATTENDEES GETS CALLED FROM EVENTS VIEW
            var einstance = DataContext.EventInstances.Where(i => i.Id == id).FirstOrDefault();

            //  Pay Event
            if (!einstance.Event.Free)
            {
                var instance =
                        (from i in DataContext.EventInstances
                         join e in DataContext.Events
                         on i.EventId equals e.Id
                         join c in DataContext.Classes
                         on i.EventId equals c.Id
                         into res
                         from cls in res.DefaultIfEmpty()
                         join tix in DataContext.Tickets
                         on e.Id equals tix.EventId
                         into tickets
                         from tics in tickets.DefaultIfEmpty()
                         join stix in DataContext.Tickets
                         on cls.SchoolId equals stix.SchoolId
                         into stickets
                         from stics in stickets.DefaultIfEmpty()
                         where i.Id == id
                         select new { EventInstance = i, Class = cls, Tickets = tics, SchoolTics = stics }).FirstOrDefault();

                var utix = new List<UserTicket>();
                //  Event Tickets
                if (instance.Tickets != null)
                {
                    utix = DataContext.UserTickets
                                .Include("EventRegistrations")
                                .Where(t => t.UserId == userid 
                                    && t.Ticket.EventId == instance.EventInstance.EventId
                                    && t.Quantity > t.EventRegistrations.Where(r => r.EventInstanceId == id).Count()
                                    && (t.Quantity * t.Ticket.Quantity) > t.EventRegistrations.Count())
                                .ToList();
                }
                //  School Tickets
                else
                {
                    utix = DataContext.UserTickets
                                .Include("EventRegistrations")
                                .Where(t => t.UserId == userid 
                                    && t.Ticket.SchoolId == instance.Class.SchoolId 
                                    && t.Quantity > t.EventRegistrations.Where(r => r.EventInstanceId == id).Count()
                                    && (t.Quantity * t.Ticket.Quantity) > t.EventRegistrations.Count())
                                .ToList();
                }

                //  Does user have tickets?
                if (utix.Count() != 0)
                {
                    var user = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();
                    RegisterDancer(userid, id, null, user.FirstName, user.LastName);
                    //DataContext.EventRegistrations.Add(new EventRegistration() { UserId = userid, EventInstanceId = id, UserTicketId = utix.FirstOrDefault().Id, FirstName = user.FirstName, LastName = user.LastName });
                    //DataContext.SaveChanges();

                    return RedirectToAction("View", new { id = instance.EventInstance.EventId, eventType = instance.EventInstance.Event is Class ? EventType.Class : EventType.Social, instanceId = id });
                }
                //  CHECK TO REMOVE SINCE ATTENDEES GETS CALLED FROM EVENTS VIEW
                //  User Needs to buy tickets
                else
                {
                    return RedirectToAction("BuyTicket", "Store", new { instanceId = id });
                }
            }
            // Free Event
            else
            {
                try
                {
                    if (User.Identity.IsAuthenticated)
                    {
                        var user = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();
                        RegisterDancer(userid, id, null, user.FirstName, user.LastName);
                    }
                    //DataContext.EventRegistrations.Add(new EventRegistration() { UserId = userid, EventInstanceId = id, FirstName = user.FirstName, LastName = user.LastName });
                    //DataContext.SaveChanges();
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
                return RedirectToAction("View", new { id = einstance.EventId, eventType = einstance.Event is Class ? EventType.Class : EventType.Social, instanceId = id });
            }
        }

        [Authorize]
        public void RegisterDancer(string userid, int instanceid, int? userTicketId, string firstName, string lastName)
        {
            var registration = new EventRegistration() { UserId = userid, UserTicketId = userTicketId, EventInstanceId = instanceid, FirstName = firstName, LastName = lastName };
            DataContext.EventRegistrations.Add(registration);
            DataContext.SaveChanges();
            EmailProcess.NewEventRegistration(registration.Id);
        }

        //else
        //{
        //    var credits = DataContext.UserTickets.Where(t => t.Ticket.SchoolId == instance.Class.SchoolId && t.UserId == userid).Include("Ticket").Include("EventRegistrations").ToList();

        //    if (credits != null)
        //    {
        //        //  User Has Remaining Credit
        //        if (credits.Sum(c => c.Quantity * c.Ticket.Quantity) > credits.Sum(c => c.EventRegistrations.Count()))
        //        {
        //            //  Get Available Tickets
        //            var ticket =
        //                    (from ut in DataContext.UserTickets
        //                     join t in DataContext.Tickets
        //                     on ut.TicketId equals t.Id
        //                     where ut.UserId == userid
        //                     && t.SchoolId == instance.Class.SchoolId
        //                     && ut.EventRegistrations.Count() < ut.Quantity * t.Quantity
        //                     select ut).FirstOrDefault();
        //            DataContext.EventRegistrations.Add(new EventRegistration() { UserId = userid, EventInstanceId = id, UserTicketId = ticket.Id });
        //            DataContext.SaveChanges();
        //            return RedirectToAction("View", new { id = instance.EventInstance.EventId, eventType = instance.EventInstance.Event is Class ? EventType.Class : EventType.Social });
        //        }
        //        //  User needs to purchase tickets
        //        else
        //        {
        //            return RedirectToAction("BuyTicket", "Store", new { instanceId = id });
        //        }
        //    }
        //var credits = DataContext.UserTickets.Where(t => t.Ticket.EventId == instance.EventInstance.EventId && t.UserId == userid).Include("Ticket").Include("EventRegistrations").ToList();
        //if (credits != null)
        //{
        //    //  User Has Remaining Credit
        //    if (credits.Sum(c => c.Quantity * c.Ticket.Quantity) > credits.Sum(c => c.EventRegistrations.Count()))
        //    {
        //        //  Get Available Tickets
        //        var ticket =
        //                (from ut in DataContext.UserTickets
        //                 join t in DataContext.Tickets
        //                 on ut.TicketId equals t.Id
        //                 where ut.UserId == userid
        //                 && t.EventId == instance.EventInstance.EventId
        //                 && ut.EventRegistrations.Count() < ut.Quantity * t.Quantity
        //                 select ut).FirstOrDefault();
        //    }
        //    //  User needs to purchase tickets
        //    else
        //    {
        //        return RedirectToAction("BuyTicket", "Store", new { instanceId = id });
        //    }
        //}

        [Authorize]
        public ActionResult UnRegister(int id, int? regId)
        {
            var userid = User.Identity.GetUserId();
            var instance = DataContext.EventInstances.Where(i => i.Id == id).Include("Event").FirstOrDefault();
            if (regId != null)
            {
                DataContext.EventRegistrations.RemoveRange(DataContext.EventRegistrations.Where(s => s.UserId == userid && s.EventInstanceId == id && s.Id == regId));
            }
            else
            {
                DataContext.EventRegistrations.RemoveRange(DataContext.EventRegistrations.Where(s => s.UserId == userid && s.EventInstanceId == id));
            }
            DataContext.SaveChanges();
            return RedirectToAction("View", new { id = instance.EventId, eventType = instance.Event is Class ? EventType.Class : EventType.Social, instanceId = id });
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

//          [Route("{eventType}/Create")]
        [Authorize(Roles = "Owner,Promoter,Teacher")]
        public ActionResult Create(EventType eventType, int? schoolId, int? promotergroupId, RoleName role, string fbId)
        {
            var model = new EventCreateViewModel(eventType, schoolId, promotergroupId, role);
            var userid = User.Identity.GetUserId();
            var user = DataContext.Users.Single(u => u.Id == userid);

            if (schoolId != null && eventType == EventType.Class)
            {
                model.School = DataContext.Schools.Single(s => s.Id == schoolId);
            }
            //  Pick a Facebook Event
            if (fbId != null)
            {
                model.FacebookId = fbId;
                model.CreateAction = "Facebook";
                var f = FacebookHelper.GetEvent(model.FacebookId, user.FacebookToken);
                model.Event = new Event() { Description = f.Description, Name = f.Name, StartDate = Convert.ToDateTime(f.StartTime.ToShortDateString()), StartTime = f.StartTime, EndDate = f.EndTime != null ? (DateTime?)Convert.ToDateTime(((DateTime)f.EndTime).ToShortDateString()) : Convert.ToDateTime(f.StartTime.ToShortDateString()), EndTime = f.EndTime, FacebookId = f.Id, FacebookLink = f.EventLink, PhotoUrl = f.CoverPhoto.LargeSource, Place = new Place() { Name = f.Address.Location, Address = f.Address.Street, City = f.Address.City, StateName = f.Address.State ?? "CA", Zip = f.Address.ZipCode, Country = f.Address.Country, Latitude = f.Address.Latitude, Longitude = f.Address.Longitude, FacebookId = f.Address.FacebookId, PlaceType = FacebookHelper.ParsePlaceType(f.Address.Categories), Public = true, Website = Uri.IsWellFormedUriString(f.Address.WebsiteUrl, UriKind.RelativeOrAbsolute) ? f.Address.WebsiteUrl : null, FacebookLink = f.Address.FacebookUrl, Filename = f.Address.CoverPhotoUrl, ThumbnailFilename = f.Address.ThumbnailUrl, GooglePlaceId = f.Address.GooglePlaceId } };
                model.FacebookId = null;

                ModelState.Clear();
                LoadCreateModel(model);
                return View(model);
            }
            //  Pick a Facebook Event
            else
            {
                LoadCreateModel(model);
                return View(model);
            }
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

            ////  Fill Places
            //var places = new List<Place>();
            //int? placeid = model.Event.Place != null ? (int?)model.Event.Place.Id : null;
            ////  Fill Places
            //places = DataContext.Places.Where(p => p.Teachers.Any(t => t.ApplicationUser.Id == userid) || p.Owners.Any(o => o.ApplicationUser.Id == userid) || p.Promoters.Any(pr => pr.ApplicationUser.Id == userid) || p.Users.Any(u => u.Id == userid) || p.Id == model.Event.Place.Id).ToList();
            //model.Places = new List<PlaceItem>();
            //model.Places.Add(new PlaceItem() { Id = 0, Latitude = 0.0, Longitude = 0.0 });
            //model.Places.AddRange(DataContext.Places.Where(p => p.Teachers.Any(t => t.ApplicationUser.Id == userid) || p.Owners.Any(o => o.ApplicationUser.Id == userid) || p.Promoters.Any(pr => pr.ApplicationUser.Id == userid) || p.Users.Any(u => u.Id == userid) || p.Id == model.Event.Place.Id).AsEnumerable().Select(p => new PlaceItem() { Address = p.Address, Address2 = p.Address2, City = p.City, Country = p.Country, FacebookId = p.FacebookId, FacebookLink = p.FacebookLink, Filename = p.Filename, Id = p.Id, Latitude = p.Latitude, Longitude = p.Longitude, Name = p.Name, PlaceType = p.PlaceType, State = p.State, ThumbnailFilename = p.ThumbnailFilename, Website = p.Website, Zip = p.Zip, Selected = (model.Event.Place != null && model.Event.Place.Id == p.Id) ? true : false }));
            ////  Fill Places

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

            //  Load School
            if (model.SchoolId != null && model.EventType == EventType.Class)
            {
                model.School = DataContext.Schools.Single(s => s.Id == model.SchoolId);
            }
            //  Load School
            //  Load Promoter Groups
            model.Schools = DataContext.Schools.Where(g => g.Teachers.Any(p => p.ApplicationUser.Id == userid) || g.Owners.Any(p => p.ApplicationUser.Id == userid) || g.Members.Any(p => p.UserId == userid)).ToList();
            //  Load Promoter Groups

            //  Load Promoter Groups
            model.PromoterGroups = DataContext.PromoterGroups.Where(g => g.Promoters.Any(p => p.ApplicationUser.Id == userid)).ToList();
            //  Load Promoter Groups


            ////  Set Tikcets
            //model.Tickets = DataContext.Tickets.Where(t => t.SchoolId == model.SchoolId).ToList();
            ////  Set Tickets
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
                model.CreateAction = "Facebook";
                var f = FacebookHelper.GetEvent(model.FacebookId, user.FacebookToken);
                model.Event = new Event() { Description = f.Description, Name = f.Name, StartDate = Convert.ToDateTime(f.StartTime.ToShortDateString()), StartTime = f.StartTime, EndDate = f.EndTime != null ? (DateTime?)Convert.ToDateTime(((DateTime)f.EndTime).ToShortDateString()) : Convert.ToDateTime(f.StartTime.ToShortDateString()), EndTime = f.EndTime, FacebookId = f.Id, FacebookLink = f.EventLink, PhotoUrl = f.CoverPhoto.LargeSource, Place = new Place() { Name = f.Address.Location, Address = f.Address.Street, City = f.Address.City, StateName = f.Address.State ?? "CA", Zip = f.Address.ZipCode, Country = f.Address.Country, Latitude = f.Address.Latitude, Longitude = f.Address.Longitude, FacebookId = f.Address.FacebookId, PlaceType = FacebookHelper.ParsePlaceType(f.Address.Categories), Public = true, Website = Uri.IsWellFormedUriString(f.Address.WebsiteUrl, UriKind.RelativeOrAbsolute) ? f.Address.WebsiteUrl : null, FacebookLink = f.Address.FacebookUrl, Filename = f.Address.CoverPhotoUrl, ThumbnailFilename = f.Address.ThumbnailUrl, GooglePlaceId = f.Address.GooglePlaceId } };
                model.FacebookId = null;

                ModelState.Clear();
                LoadCreateModel(model);
                return View(model);
            }
            //  Pick a Facebook Event

            //  Create the Place
            var place = DataContext.Places.Where(p => p.GooglePlaceId == model.Event.Place.GooglePlaceId && model.Event.Place.GooglePlaceId != null).FirstOrDefault();
            if (place == null)
            {
                place = DataContext.Places.Add(model.Event.Place);
                DataContext.SaveChanges();
            }
            model.Event.Place = null;
            model.Event.PlaceId = place.Id;
            //  Create the Place

            if (model.EventType == EventType.Social)
            {
                ModelState["SkillLevel"].Errors.Clear();
            }

            //if (model.Event.PlaceId >= 0)
            //{
            //    ModelState["Event.Place.Name"].Errors.Clear();
            //    ModelState["Event.Place.Address"].Errors.Clear();
            //    ModelState["Event.Place.City"].Errors.Clear();
            //    ModelState["Event.Place.State"].Errors.Clear();
            //    ModelState["Event.Place.Zip"].Errors.Clear();
            //}

            if (ModelState.IsValid)
            {
                try
                {
                    var evnt = new Event();
                    if (model.EventType == EventType.Class)
                    {
                        evnt = new Class();
                        ((Class)evnt).SchoolId = (int)model.SchoolId;
                        ((Class)evnt).SkillLevel = (SkillLevel)model.SkillLevel;
                        //  Add Teachers for Class
                        if (model.Role == RoleName.Teacher)
                        {
                            ((Class)evnt).Teachers = new List<Teacher>() { DataContext.Teachers.Single(t => t.ApplicationUser.Id == userid) };
                        }
                        else
                        {
                            ((Class)evnt).Owners = new List<Owner>() { DataContext.Owners.Single(o => o.ApplicationUser.Id == userid) };
                        }
                        //  Add Teachers for Class
                    }
                    else
                    {
                        evnt = new Social();
                        ((Social)evnt).PromoterGroupId = model.PromoterGroupId;
                        if (model.Role == RoleName.Promoter)
                        {
                            ((Social)evnt).Promoters = new List<Promoter>() { DataContext.Promoters.Single(t => t.ApplicationUser.Id == userid) };
                        }
                        else
                        {
                            ((Social)evnt).Owners = new List<Owner>() { DataContext.Owners.Single(o => o.ApplicationUser.Id == userid) };
                        }
                    }

                    //  Build Class/Social
                    evnt.Creator = user;
                    evnt.EventInstances = new List<EventInstance>();
                    TryUpdateModel(evnt, "Event");
                    TryUpdateModel(evnt);
                    evnt.Place = null;
                    evnt.PlaceId = place.Id;
                    //  Build Class/Social

                    //  Add Admin
                    evnt.EventMembers = new List<EventMember>() { new EventMember() { Admin = true, UserId = user.Id } };
                    //  Add Admin

                    //  Update Month Days
                    if (model.MonthDays.PostedItems != null)
                    {
                        evnt.MonthDays = String.Join("-", model.MonthDays.PostedItems) + (model.HiddenMonthDay != "" ? ("-" + model.HiddenMonthDay) : "");
                    }
                    else
                    {
                        evnt.MonthDays = model.HiddenMonthDay;
                    }
                    //  Update Month Days

                    //  Add Recurring Events
                    model.Event.EventInstances = new List<EventInstance>();
                    if (model.Event.Recurring)
                    {
                        var sdate = evnt.StartDate;
                        var edate = evnt.EndDate;
                        var daylength = (Convert.ToDateTime(evnt.EndDate) - evnt.StartDate).TotalDays;
                        sdate = ApplicationUtility.GetNextDate(sdate, evnt.Frequency, (int)evnt.Interval, evnt.Day, sdate, evnt.MonthDays);

                        for (int i = 1; i <= model.EventCount; i++)
                        {
                            evnt.EventInstances.Add(new EventInstance() { DateTime = sdate, EndDate = sdate.AddDays(daylength), PlaceId = evnt.PlaceId, StartTime = Convert.ToDateTime(sdate.ToShortDateString() + " " + ((DateTime)evnt.StartTime).ToShortTimeString()), EndTime = Convert.ToDateTime(sdate.AddDays(daylength).ToShortDateString() + " " + ((DateTime)evnt.EndTime).ToShortTimeString()) });
                            sdate = ApplicationUtility.GetNextDate(sdate, evnt.Frequency, (int)evnt.Interval, evnt.Day, sdate.AddDays(1), evnt.MonthDays);
                        }
                    }
                    else
                    {
                        evnt.EventInstances.Add(new EventInstance() { DateTime = evnt.StartDate, EndDate = Convert.ToDateTime(evnt.EndDate), PlaceId = evnt.PlaceId, StartTime = evnt.StartTime, EndTime = evnt.EndTime });
                    }
                    //  Add Recurring Events

                    //  Add Linked Object
                    if (evnt.FacebookId != null)
                    {
                        evnt.LinkedMedia = new List<LinkedMedia>() { new LinkedMedia() { FacebookId = evnt.FacebookId, MediaSource = MediaSource.Facebook, Name = model.FacebookEventName, ObjectType = "Event", Url = evnt.FacebookLink, Default = true } };
                    }
                    //  Add Linked Object

                    //  Add Dance Styles
                    var styleids = model.StyleIds.Split('-');
                    if (styleids.Length != 0)
                    {
                        evnt.DanceStyles = DataContext.DanceStyles.Where(x => styleids.Contains(x.Id.ToString())).ToList();
                        DataContext.SaveChanges();
                    }
                    //  Add Dance Styles

                    //  Add Tickets
                    if (!model.Event.Free)
                    {
                        if (!model.UseSchoolTickets)
                        {
                            evnt.Tickets = new List<Ticket>();
                            evnt.Tickets.Add(new Ticket() { Price = (decimal)model.TicketPrice, Quantity = (int)model.TicketQuantity });
                            //  DataContext.SaveChanges();
                        }
                    }
                    //  Add Tickets

                    //  Save Event
                    DataContext.Events.Add(evnt);
                    DataContext.SaveChanges();
                    //  Save Event

                    return RedirectToAction("View", new { id = evnt.Id, eventType = model.EventType });
                }
                catch (DbEntityValidationException e)
                {
                    foreach (var eve in e.EntityValidationErrors)
                    {
                        Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                            eve.Entry.Entity.GetType().Name, eve.Entry.State);
                        foreach (var ve in eve.ValidationErrors)
                        {
                            Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                                ve.PropertyName, ve.ErrorMessage);
                        }
                    }
                    throw;
                }

                //if (model.Event.PlaceId == 0)
                //{
                //    var add = Geolocation.ParseAddress(model.Event.Place.Address + ", " + model.Event.Place.City + ", " + model.Event.Place.State + ", " + model.Event.Place.Zip);
                //    model.Event.Place.Latitude = add.Latitude;
                //    model.Event.Place.Longitude = add.Longitude;
                //}

            }
            else
            {
                LoadCreateModel(model);

                return View(model);
            }
        }

        [Authorize(Roles = "Owner,Promoter,Teacher")]
        public ActionResult DeleteTicket(int id, int ticketId, EventType eventType)
        {
            var ticket = DataContext.Tickets.Include("UserTickets").Where(t => t.Id == ticketId).FirstOrDefault();
            if (ticket.UserTickets.Count() == 0)
            {
                DataContext.Tickets.Remove(ticket);
                DataContext.SaveChanges();
            }

            return RedirectToAction("Manage", new { id = id, eventType = eventType });
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

        //[Authorize]
        //public ActionResult Edit(int? id, EventType eventType, int? placeId)
        //{
        //    var model = new EventEditViewModel();
        //    model.EventType = eventType;

        //    if (id != null)
        //    {
        //        var ev = DataContext.Events.Where(e => e.Id == id).Include("Creator").FirstOrDefault();

        //        if (ev is Class)
        //        {
        //            var cls = DataContext.Events.OfType<Class>().Where(e => e.Id == id).Include("DanceStyles").Include("Place").FirstOrDefault();
        //            model.Event = cls;
        //            model.ClassType = cls.ClassType;
        //            model.SkillLevel = cls.SkillLevel;
        //        }
        //        else
        //        {
        //            var soc = DataContext.Events.OfType<Social>().Where(e => e.Id == id).Include("DanceStyles").Include("Place").FirstOrDefault();
        //            model.Event = soc;
        //            model.SocialType = soc.SocialType;
        //        }

        //        //  Update Facebook Details
        //        if (ev.FacebookId != null)
        //        {
        //            var fbev = FacebookHelper.GetEvent(ev.FacebookId, ev.Creator.FacebookToken, "id,cover,description,end_time,is_date_only,location,name,owner,privacy,start_time,ticket_uri,timezone,updated_time,venue,parent_group");
        //            model.Event.Name = fbev.Name;
        //            model.Event.Description = fbev.Description;
        //            model.Event.PhotoUrl = fbev.CoverPhoto.LargeSource;
        //            model.Event.StartDate = fbev.StartTime;
        //            model.Event.StartTime = fbev.StartTime;
        //            model.Event.EndTime = fbev.EndTime;
        //        }

        //        //  Update Facebook Details

        //    }

        //    BuildEditModel(model);

        //    return View(model);
        //}

        [Authorize(Roles = "Owner,Promoter,Teacher")]
        public PartialViewResult AddStyle(EventManageViewModel model)
        {
            var evnt = DataContext.Events.Include("DanceStyles").Single(e => e.Id == model.Event.Id);
            if (evnt.DanceStyles.Where(s => s.Id == model.NewStyleId).Count() == 0)
            {
                evnt.DanceStyles.Add(DataContext.DanceStyles.Single(s => s.Id == model.NewStyleId));
                DataContext.SaveChanges();
            }
            return PartialView("~/Views/Event/Partial/_DanceStylesPartial.cshtml", new EventDanceStylesPartialViewModel() { DanceStyles = evnt.DanceStyles, EventId = evnt.Id, EventType = evnt is Class ? EventType.Class : EventType.Social } );
        }

        [Authorize(Roles = "Owner,Promoter,Teacher")]
        public PartialViewResult AddYouTubePlaylist(EventManageViewModel model)
        {
            var ytPlaylist = YouTubeHelper.GetPlaylist(model.NewYoutubePlayList.ToString());
            var userid = User.Identity.GetUserId();
            var auth = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();
            var ev = DataContext.Events.Where(e => e.Id == model.Event.Id).Include("Playlists").FirstOrDefault();
            //  var ePlaylist = new EventPlaylist() { Title = ytPlaylist.Name, PublishDate = ytPlaylist.PubDate, YouTubeId = ytPlaylist.Id, Author = auth, YouTubeUrl = ytPlaylist.Url, CoverPhoto = ytPlaylist.ThumbnailUrl, MediaSource = MediaSource.YouTube };

            if (ev.Playlists.Where(p => p.YouTubeId == ytPlaylist.Id).Count() == 0)
            {
                ev.Playlists.Add(new EventPlaylist() { Title = ytPlaylist.Name, PublishDate = ytPlaylist.PubDate, YouTubeId = ytPlaylist.Id, Author = auth, YouTubeUrl = ytPlaylist.Url.ToString(), CoverPhoto = ytPlaylist.ThumbnailUrl.ToString(), MediaSource = MediaSource.YouTube, UpdatedDate = ytPlaylist.PubDate, VideoCount = ytPlaylist.VideoCount });
                DataContext.Entry(ev).State = EntityState.Modified;
                DataContext.SaveChanges();
                ViewBag.Message = "Playlist was imported";
            }
            return PartialView("~/Views/Shared/Events/_ManagePlaylistsPartial.cshtml", ev.Playlists);
        }

        [Authorize(Roles = "Owner,Promoter,Teacher")]
        public PartialViewResult DeletePlaylist(int id, int playListId)
        {
            var playlists = DataContext.Events.Include("Playlists").Single(e => e.Id == id).Playlists;
            playlists.Remove(playlists.Single(l => l.Id == playListId));
            DataContext.SaveChanges();
            ViewBag.Message = "Playlist was removed";
            return PartialView("~/Views/Shared/Events/_ManagePlaylistsPartial.cshtml", playlists);
        }

        [Authorize(Roles = "Owner,Promoter,Teacher")]
        public PartialViewResult AddYouTubeVideo(EventManageViewModel model)
        {
            var ytVideo = YouTubeHelper.GetVideo(model.NewYouTubeVideo.ToString());
            var userid = User.Identity.GetUserId();
            var auth = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();
            var ev = DataContext.Events.Where(e => e.Id == model.Event.Id).Include("Videos").FirstOrDefault();
            //  var ePlaylist = new EventPlaylist() { Title = ytPlaylist.Name, PublishDate = ytPlaylist.PubDate, YouTubeId = ytPlaylist.Id, Author = auth, YouTubeUrl = ytPlaylist.Url, CoverPhoto = ytPlaylist.ThumbnailUrl, MediaSource = MediaSource.YouTube };

            if (ev.Videos.Where(p => p.YoutubeId == ytVideo.Id).Count() == 0)
            {
                ev.Videos.Add(new EventVideo() { Title = ytVideo.Title, PublishDate = ytVideo.PubDate, YoutubeId = ytVideo.Id, Author = auth, YoutubeUrl = ytVideo.VideoLink.ToString(), PhotoUrl = ytVideo.Thumbnail.ToString(), MediaSource = MediaSource.YouTube });
                DataContext.Entry(ev).State = EntityState.Modified;
                DataContext.SaveChanges();
                ViewBag.Message = "Video was imported";
            }
            return PartialView("~/Views/Shared/Events/_ManageVideosPartial.cshtml", ev.Videos);
        }

        [Authorize(Roles = "Owner,Promoter,Teacher")]
        public PartialViewResult DeleteVideo(int id, int videoId)
        {
            var videos = DataContext.Events.Include("Videos").Single(e => e.Id == id).Videos;
            videos.Remove(videos.Single(l => l.Id == videoId));
            DataContext.SaveChanges();
            ViewBag.Message = "Video was removed";
            return PartialView("~/Views/Shared/Events/_ManageVideosPartial.cshtml", videos);
        }

        [Authorize(Roles = "Owner,Promoter,Teacher")]
        public PartialViewResult AddFacebookAlbum(int id, string albumId)
        {
            var userid = User.Identity.GetUserId();
            var auth = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();
            var ev = DataContext.Events.Where(e => e.Id == id).Include("Albums").FirstOrDefault();
            //  var ePlaylist = new EventPlaylist() { Title = ytPlaylist.Name, PublishDate = ytPlaylist.PubDate, YouTubeId = ytPlaylist.Id, Author = auth, YouTubeUrl = ytPlaylist.Url, CoverPhoto = ytPlaylist.ThumbnailUrl, MediaSource = MediaSource.YouTube };

            if (ev.Albums.Where(a => a.FacebookId == albumId).Count() == 0)
            {
                var fbalbum = ((IEnumerable<EDR.Models.FacebookAlbum>)Session["FacebookAlbums"]).Single(a => a.Id == albumId);
                ev.Albums.Add(new EventAlbum() { Name = fbalbum.Name, AlbumDate = fbalbum.Updated_Time, FacebookId = albumId, PostedBy = auth, CoverPhoto = fbalbum.Cover_Photo,  MediaSource = MediaSource.Facebook, Description = fbalbum.Description, SourceLink = fbalbum.Link, PhotoCount = Convert.ToInt32(fbalbum.Count), CoverThumbnail = fbalbum.Cover_Photo });
                DataContext.Entry(ev).State = EntityState.Modified;
                DataContext.SaveChanges();
                ViewBag.Message = "Album was imported";
            }
            return PartialView("~/Views/Shared/Events/_ManageAlbumsPartial.cshtml", ev.Albums);
        }

        [Authorize(Roles = "Owner,Promoter,Teacher")]
        public PartialViewResult DeleteAlbum(int id, int albumId)
        {
            var albums = DataContext.Events.Include("Albums").Single(e => e.Id == id).Albums;
            albums.Remove(albums.Single(l => l.Id == albumId));
            DataContext.SaveChanges();
            ViewBag.Message = "Album was removed";
            return PartialView("~/Views/Shared/Events/_ManageAlbumsPartial.cshtml", albums);
        }

        [Authorize(Roles = "Owner,Promoter,Teacher")]
        public ActionResult DeleteStyle(int id, int styleId, EventType eventType)
        {
            DataContext.Events.Include("DanceStyles").Single(e => e.Id == id).DanceStyles.Remove(DataContext.DanceStyles.Single(s => s.Id == styleId));
            DataContext.SaveChanges();
            return RedirectToAction("Manage", new { id = id, eventType = eventType });
        }

        [Authorize(Roles = "Owner,Promoter,Teacher")]
        public PartialViewResult AddLinkedMedia(EventManageViewModel model)
        {
            var evt = DataContext.Events.Include("LinkedMedia").Single(e => e.Id == model.Event.Id);
            var source = model.NewLinkedMedia.Host.Contains("facebook.com") ? MediaSource.Facebook : MediaSource.None;
            if (source == MediaSource.Facebook)
            {
                var type = model.NewLinkedMedia.Segments[1].Replace("/", "");
                var id = model.NewLinkedMedia.Segments[2].Replace("/", "");
                long fbid;
                dynamic obj;

                if (long.TryParse(id, out fbid))
                {
                    var fb = new Facebook.FacebookClient(FacebookHelper.GetToken());
                    obj = fb.Get(id);
                }
                else
                {
                    var fb = new Facebook.FacebookClient(FacebookHelper.GetGlobalToken());
                    obj = fb.Get("/search?q=" + id + "&type=group");
                    obj = obj.data[0];
                }

                if (evt.LinkedMedia.Where(m => m.FacebookId == obj.id).Count() == 0)
                {
                    evt.LinkedMedia.Add(new LinkedMedia() { MediaSource = source, FacebookId = obj.id, Default = false, Name = obj.name, Url = model.NewLinkedMedia.ToString(), ObjectType = model.NewLinkedMedia.Segments[1].Replace("/", "") });
                    DataContext.SaveChanges();
                }
            }

            ViewBag.Message = "Link was added";
            return PartialView("~/Views/Shared/Events/_ManageLinkedMediaPartial.cshtml", evt.LinkedMedia);
        }

        [Authorize(Roles = "Owner,Promoter,Teacher")]
        public PartialViewResult DeleteLinkedMedia(int id, int linkId)
        {
            var media = DataContext.Events.Include("LinkedMedia").Single(e => e.Id == id).LinkedMedia;
            media.Remove(media.Single(m => m.Id == linkId));
            DataContext.SaveChanges();
            ViewBag.Message = "Linked Media was removed";
            return PartialView("~/Views/Shared/Events/_ManageLinkedMediaPartial.cshtml", media);
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

        [Authorize(Roles = "Owner,Promoter,Teacher")]
        [HttpPost]
        public ActionResult Save(EventManageViewModel model)
        {
            //model.Event.StartTime = Convert.ToDateTime(model.Event.StartDate.ToShortDateString() + " " + model.StartHour.ToString() + ":" + model.StartMinute.ToString() + " " + model.StartAMPM);
            //model.Event.EndTime = Convert.ToDateTime(((DateTime)model.Event.StartDate).ToShortDateString() + " " + model.EndHour.ToString() + ":" + model.EndMinute.ToString() + " " + model.EndAMPM);

            if (ModelState.IsValid)
            {
                try
                {
                    var evnt = new Event();
                    if (model.EventType == EventType.Class)
                    {
                        evnt = DataContext.Classes
                                        .Include("Place")
                                        .Include("EventInstances")
                                        .Single(s => s.Id == model.Event.Id);
                    }
                    else
                    {
                        evnt = DataContext.Socials
                                        .Include("Place")
                                        .Include("EventInstances")
                                        .Single(s => s.Id == model.Event.Id);
                    }
                    evnt.Name = model.Event.Name;
                    evnt.Description = model.Event.Description;
                    evnt.StartDate = model.Event.StartDate;
                    evnt.StartTime = model.Event.StartTime;
                    evnt.EndDate = model.Event.EndDate;
                    evnt.EndTime = model.Event.EndTime;
                    evnt.FacebookLink = model.Event.FacebookLink;
                    evnt.Gender = model.Event.Gender;
                    if (evnt is Social)
                    {
                        ((Social)evnt).MusicType = model.MusicType;
                        ((Social)evnt).PromoterGroupId = model.PromoterGroupId;
                        ((Social)evnt).SocialType = model.SocialType;
                    }
                    else
                    {
                        ((Class)evnt).ClassType = model.ClassType;
                        ((Class)evnt).SkillLevel = model.SkillLevel;
                    }

                    if (model.NewPlace.GooglePlaceId != null)
                    {
                        var place = DataContext.Places.Where(p => p.GooglePlaceId == model.NewPlace.GooglePlaceId).FirstOrDefault();
                        if (place == null)
                        {
                            place = DataContext.Places.Add(model.NewPlace);
                            DataContext.SaveChanges();
                        }
                        evnt.PlaceId = place.Id;
                        evnt.EventInstances.ToList().ForEach(i => i.PlaceId = place.Id);
                    }

                    DataContext.Entry(evnt).State = EntityState.Modified; 
                    DataContext.SaveChanges();

                    return RedirectToAction("Manage", new { id = model.Event.Id, eventType = model.EventType });
                }
                catch (DbEntityValidationException e)
                {
                    foreach (var eve in e.EntityValidationErrors)
                    {
                        Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                            eve.Entry.Entity.GetType().Name, eve.Entry.State);
                        foreach (var ve in eve.ValidationErrors)
                        {
                            Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                                ve.PropertyName, ve.ErrorMessage);
                        }
                    }
                    throw;
                }
            }
            else
            {
                return View(model);
            }
        }

        [Authorize(Roles = "Owner,Promoter,Teacher")]
        [HttpPost]
        public ActionResult AddInstances(int id, int eventCount)
        {
            var evnt = DataContext.Events.Include("EventInstances").Single(e => e.Id == id);

            try
            {
                //  Add Recurring Events
                if (evnt.Recurring)
                {
                    if (DataContext.EventInstances.Where(i => i.EventId == id && i.DateTime >= DateTime.Today).Count() < 20)
                    {
                        var sdate = DataContext.EventInstances.Where(i => i.EventId == id).Max(i => i.DateTime).AddDays(1);
                        var daylength = (Convert.ToDateTime(evnt.EndDate) - evnt.StartDate).TotalDays;
                        sdate = ApplicationUtility.GetNextDate(evnt.StartDate, evnt.Frequency, (int)evnt.Interval, evnt.Day, sdate, evnt.MonthDays);

                        for (int i = 1; i <= eventCount; i++)
                        {
                            evnt.EventInstances.Add(new EventInstance() { DateTime = sdate, EndDate = sdate.AddDays(daylength), PlaceId = evnt.PlaceId, StartTime = Convert.ToDateTime(sdate.ToShortDateString() + " " + ((DateTime)evnt.StartTime).ToShortTimeString()), EndTime = Convert.ToDateTime(sdate.AddDays(daylength).ToShortDateString() + " " + ((DateTime)evnt.EndTime).ToShortTimeString()) });
                            sdate = ApplicationUtility.GetNextDate(sdate, evnt.Frequency, (int)evnt.Interval, evnt.Day, sdate.AddDays(1), evnt.MonthDays);
                        }
                        DataContext.SaveChanges();
                    }
                }

                return RedirectToAction("Manage", new { id = id, eventType = evnt is Class ? EventType.Class : EventType.Social });
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

                return RedirectToAction("Manage", new { id = id, eventType = evnt is Class ? EventType.Class : EventType.Social });
            }
        }

        [Authorize(Roles = "Owner,Promoter,Teacher")]
        [HttpPost]
        public ActionResult SaveInstance(EventInstanceManageViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (model.NewPlace.GooglePlaceId != null)
                    {
                        var place = DataContext.Places.Where(p => p.GooglePlaceId == model.NewPlace.GooglePlaceId).FirstOrDefault();
                        if (place == null)
                        {
                            place = DataContext.Places.Add(model.NewPlace);
                            DataContext.SaveChanges();
                        }
                        model.Instance.PlaceId = place.Id;
                    }

                    DataContext.Entry(model.Instance).State = EntityState.Modified;
                    DataContext.SaveChanges();

                    return RedirectToAction("ManageInstance", new { id = model.Instance.Id });
                }
                catch (DbEntityValidationException e)
                {
                    foreach (var eve in e.EntityValidationErrors)
                    {
                        Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                            eve.Entry.Entity.GetType().Name, eve.Entry.State);
                        foreach (var ve in eve.ValidationErrors)
                        {
                            Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                                ve.PropertyName, ve.ErrorMessage);
                        }
                    }
                    throw;
                }
            }
            else
            {
                return View(model);
            }
        }

        //[Authorize]
        //[HttpPost]
        //public ActionResult Edit(EventEditViewModel model)
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
        //        var user = DataContext.Users.Where(u => u.Id == userid).Include("Places").FirstOrDefault();

        //        //  New Event
        //        var evt = new Event();
        //        if (model.Event.Id == 0)
        //        {
        //            if (model.EventType == EventType.Class)
        //            {
        //                var cls = new Class() { ClassType = model.ClassType, Teachers = new List<Teacher>(), Owners = new List<Owner>(), Creator = user };

        //                if (DataContext.Teachers.Where(t => t.ApplicationUser.Id == userid).Count() == 1)
        //                {
        //                    cls.Teachers.Add(DataContext.Teachers.Where(t => t.ApplicationUser.Id == userid).FirstOrDefault());
        //                }
        //                else if (DataContext.Owners.Where(t => t.ApplicationUser.Id == userid).Count() == 1)
        //                {
        //                    cls.Owners.Add(DataContext.Owners.Where(t => t.ApplicationUser.Id == userid).FirstOrDefault());
        //                }
        //                evt = cls;
        //            }
        //            else
        //            {
        //                var soc = new Social() { SocialType = model.SocialType, Promoters = new List<Promoter>(), Owners = new List<Owner>(), Creator = user };
        //                if (DataContext.Promoters.Where(t => t.ApplicationUser.Id == userid).Count() == 1)
        //                {
        //                    soc.Promoters.Add(DataContext.Promoters.Where(t => t.ApplicationUser.Id == userid).FirstOrDefault());
        //                }
        //                else if (DataContext.Owners.Where(t => t.ApplicationUser.Id == userid).Count() == 1)
        //                {
        //                    soc.Owners.Add(DataContext.Owners.Where(t => t.ApplicationUser.Id == userid).FirstOrDefault());
        //                }
        //                evt = soc;
        //            }
        //            evt.DanceStyles = new List<DanceStyle>();
        //        }
        //        //  Existing Event
        //        else
        //        {
        //            evt = new Event();

        //            if (model.EventType == EventType.Class)
        //            {
        //                var cls = DataContext.Events.OfType<Class>().Where(c => c.Id == model.Event.Id).Include("DanceStyles").FirstOrDefault();
        //                cls.ClassType = model.ClassType;
        //                evt = cls;
        //            }
        //            else
        //            {
        //                var soc = DataContext.Events.OfType<Social>().Where(c => c.Id == model.Event.Id).Include("DanceStyles").FirstOrDefault();
        //                soc.SocialType = model.SocialType;
        //                evt = soc;
        //            }
        //        }
        //        evt.Name = model.Event.Name;
        //        evt.StartDate = model.Event.StartDate;
        //        evt.AllDay = model.Event.AllDay;
        //        evt.StartTime = model.Event.StartTime;
        //        evt.EndTime = model.Event.EndTime;
        //        evt.EndDate = model.Event.EndDate;
        //        evt.Description = model.Event.Description;
        //        evt.Recurring = model.Event.Recurring;
        //        evt.Frequency = model.Event.Frequency;
        //        evt.Interval = model.Event.Interval;
        //        if (model.EventType == EventType.Class)
        //        {
        //            ((Class)evt).SkillLevel = (int)model.SkillLevel;
        //        }


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

        //        if (model.Event.Id == 0)
        //        {
        //            DataContext.Events.Add(evt);
        //            DataContext.SaveChanges();
        //            model.Event.Id = evt.Id;
        //        }
        //        else
        //        {
        //            DataContext.Entry(evt).State = EntityState.Modified;
        //            DataContext.SaveChanges();
        //        }

        //        return RedirectToAction("View", "Event", new { id = model.Event.Id, eventType = model.EventType });
        //    }
        //    else
        //    {
        //        BuildEditModel(model);
        //        return View(model);
        //    }
        //    //catch (DbEntityValidationException e)
        //    //{
        //    //    var msg = "";
        //    //    foreach (var eve in e.EntityValidationErrors)
        //    //    {
        //    //        msg = eve.Entry.Entity.GetType().Name + " " + eve.Entry.State;
        //    //        foreach (var ve in eve.ValidationErrors)
        //    //        {
        //    //            msg = ve.PropertyName + " " + ve.ErrorMessage;
        //    //        }
        //    //    }
        //    //    return View();
        //    //}
        //}

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
            var userid = User.Identity.GetUserId();
            var user = UserManager.FindById(userid);

            var evt = DataContext.Events.Where(e => e.Id == id)
                    .Include("Videos")
                    .Include("Pictures")
                    .Include("Playlists")
                    .Include("Reviews")
                    .Include("LinkedFacebookObjects")
                    .Include("LinkedMedia")
                    .Include("EventMembers")
                    .Include("Feeds")
                    .Include("Tickets")
                    .Include("EventInstances")
                    .FirstOrDefault();

            var returnAction = "";
            if (evt is Class)
            {
                returnAction = Url.Action("Manage", "School", new { id = DataContext.Schools.Where(s => s.Classes.Any(c => c.Id == id)).FirstOrDefault().Id });
            }
            else
            {
                returnAction = Url.Action("Manage", "Promoter", new { username = User.Identity.Name });
            }

            evt.Videos.Clear();
            evt.Pictures.Clear();
            evt.Playlists.Clear();
            DataContext.Reviews.RemoveRange(evt.Reviews);
            DataContext.LinkedFacebookObjects.RemoveRange(evt.LinkedFacebookObjects);
            DataContext.Feeds.RemoveRange(evt.Feeds);
            DataContext.EventMembers.RemoveRange(evt.EventMembers);
            DataContext.Tickets.RemoveRange(evt.Tickets);
            DataContext.EventInstances.RemoveRange(evt.EventInstances);
            DataContext.LinkedMedia.RemoveRange(evt.LinkedMedia);
            DataContext.Events.Remove(evt);
            DataContext.SaveChanges();

            return Redirect(returnAction);
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

        //[Authorize]
        //[HttpPost]
        //public ActionResult ImportFacebookEvent(ImportFacebookEventViewModel model)
        //{
        //    var userid = User.Identity.GetUserId();
        //    var user = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();

        //    if (user.FacebookToken != null)
        //    {
        //        if (model.FacebookLink != null)
        //        {
        //            var id = ParseFacebookLink(model.FacebookLink);

        //            var evt = DataContext.Events.Where(e => e.FacebookId == id).FirstOrDefault();
        //            if (evt == null)
        //            {
        //                return RedirectToAction("ConfirmFacebookEvent", "Event", new { id = id, eventType = model.Type });
        //            }
        //            else
        //            {
        //                if (evt is Class)
        //                {
        //                    return RedirectToAction("View", "Event", new { id = evt.Id, eventType = EventType.Class });
        //                }
        //                else
        //                {
        //                    return RedirectToAction("View", "Event", new { id = evt.Id, eventType = EventType.Social });
        //                }
        //            }
        //        }
        //        else
        //        {
        //            return View(model);
        //        }
        //    }
        //    else
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //}

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

        //[Authorize(Roles = "Teacher,Owner")]
        //public ActionResult CreateClass(int? placeId)
        //{
        //    if (User.Identity.IsAuthenticated)
        //    {
        //        return RedirectToAction("Edit", new { eventType = EventType.Class, placeId = placeId });
        //        //var userid = User.Identity.GetUserId();
        //        //var user = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();

        //        //if (placeId != null)
        //        //{
        //        //    if (DataContext.Places.Where(p => p.Id == placeId).Where(p => p.Teachers.Any(t => t.ApplicationUser.Id == userid) || p.Owners.Any(o => o.ApplicationUser.Id == userid)).Count() == 0)
        //        //    {
        //        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //        //    }
        //        //}
                
        //        //var model = new EventCreateViewModel();
        //        //model.EventType = EventType.Class;

        //        //LoadCreateModel(model);

        //        //if (placeId != null)
        //        //{
        //        //    model.PlaceId = (int)placeId;
        //        //}

        //        //if (user.FacebookToken != null)
        //        //{
        //        //    if (Session["FacebookEvents"] == null)
        //        //    {
        //        //        Session["FacebookEvents"] = FacebookHelper.GetEvents(user.FacebookToken);
        //        //    }
        //        //    if (Session["FacebookEvents"] != null)
        //        //    {
        //        //        model.FacebookEvents = (List<FacebookEvent>)Session["FacebookEvents"];
        //        //    }
        //        //}

        //        //return View("Create", model);
        //    }
        //    else
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //}

        //[Authorize(Roles = "Owner,Promoter")]
        //public ActionResult CreateSocial(int? placeId)
        //{
        //    if (User.Identity.IsAuthenticated)
        //    {
        //        return RedirectToAction("Edit", new { eventType = EventType.Social, placeId = placeId });

        //        //var userid = User.Identity.GetUserId();
        //        //var user = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();

        //        //if (placeId != null)
        //        //{
        //        //    if (DataContext.Places.Where(p => p.Id == placeId).Where(p => p.Teachers.Any(t => t.ApplicationUser.Id == userid) || p.Owners.Any(o => o.ApplicationUser.Id == userid)).Count() == 0)
        //        //    {
        //        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //        //    }
        //        //}

        //        //var model = new EventCreateViewModel();
        //        //model.EventType = EventType.Social;

        //        //LoadCreateModel(model);

        //        //if (placeId != null)
        //        //{
        //        //    model.PlaceId = (int)placeId;
        //        //}

        //        //if (user.FacebookToken != null)
        //        //{
        //        //    if (Session["FacebookEvents"] == null)
        //        //    {
        //        //        Session["FacebookEvents"] = FacebookHelper.GetEvents(user.FacebookToken);
        //        //    }
        //        //    if (Session["FacebookEvents"] != null)
        //        //    {
        //        //        model.FacebookEvents = (List<FacebookEvent>)Session["FacebookEvents"];
        //        //    }
        //        //}

        //        //return View("Create", model);
        //    }
        //    else
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //}

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

        //[Authorize]
        //[HttpPost]
        //public ActionResult ConfirmFacebookEvent(ConfirmFacebookEvent model)
        //{
        //    if (model.Type == EventType.Social)
        //    {
        //        ModelState["SkillLevel"].Errors.Clear();
        //    }
        //    var userid = User.Identity.GetUserId();
        //    var user = DataContext.Users.Where(u => u.Id == userid).Include("CurrentRole").FirstOrDefault();

        //    //  Update Month Days
        //    if (model.PostedMonthDays != null)
        //    {
        //        model.Event.MonthDays = String.Join("-", model.PostedMonthDays) + (model.HiddenMonthDay != "" ? ("-" + model.HiddenMonthDay) : "");
        //    }
        //    else
        //    {
        //        model.Event.MonthDays = model.HiddenMonthDay;
        //    }
        //    //  Update Month Days

        //    //  Update Lon/Lat for Place
        //    if (model.Event.Place.Latitude == 0.0 || model.Event.Place.Longitude == 0.0)
        //    {
        //        var address = Geolocation.ParseAddress(model.Event.Place.Address + " " + model.Event.Place.City + " " + model.Event.Place.State + " " + model.Event.Place.Zip);
        //        model.Event.Place.Latitude = address.Latitude;
        //        model.Event.Place.Longitude = address.Longitude;
        //    }
        //    //  Update Lon/Lat for Place

        //    if (ModelState.IsValid)
        //    {
        //        var evt = model.Event;

        //        try
        //        {
        //            //  Check if FB event exists
        //            if (DataContext.Events.Where(e => e.FacebookId == evt.FacebookId).Count() == 0)
        //            {
        //                int eventId = 0;

        //                if (model.PostedStyles == null)
        //                {
        //                    model.PostedStyles = new PostedStyles();
        //                    model.PostedStyles.DanceStyleIds = new string[0];
        //                }

        //                //  Import Facebook Place
        //                if (evt.Place.Id == 0)
        //                {
        //                    DataContext.Places.Add(evt.Place);
        //                    DataContext.SaveChanges();

        //                    //  Add the place to user if it's not a Facebook Page
        //                    if (evt.Place.FacebookId == null)
        //                    {
        //                        user.Places.Add(evt.Place);
        //                        DataContext.SaveChanges();
        //                    }
        //                }
        //                else
        //                {
        //                    DataContext.Places.Attach(evt.Place);
        //                }

        //                //Place pl = new Place();
        //                //if (model.PlaceId == 0)
        //                //{
        //                //    if (model.NewPlace.Latitude == 0.0 || model.NewPlace.Longitude == 0.0)
        //                //    {
        //                //        var ad = Geolocation.ParseAddress(model.NewPlace.Address + " " + model.NewPlace.City + ", " + model.NewPlace.State + " " + model.NewPlace.Zip);
        //                //        model.NewPlace.Longitude = ad.Longitude;
        //                //        model.NewPlace.Latitude = ad.Latitude;
        //                //    }
        //                //    pl = model.NewPlace;
        //                //    pl.PlaceType = Enums.PlaceType.OtherPlace;
        //                //}
        //                //else if (model.PlaceId > 0)
        //                //{
        //                //    pl = DataContext.Places.Where(p => p.Id == model.PlaceId).FirstOrDefault();
        //                //}
        //                //else
        //                //{
        //                //    Address ad = new Address();
        //                //    ad = Utilities.Geolocation.ParseAddress(user.ZipCode != null ? user.ZipCode : "90065");
        //                //    pl = new Place() { Name = "TBD", PlaceType = Enums.PlaceType.OtherPlace, Zip = user.ZipCode != null ? user.ZipCode : "90065", Address = ad.StreetNumber + " " + ad.StreetName, City = ad.City, State = (Enums.State)System.Enum.Parse(typeof(Enums.State), ad.State), Latitude = ad.Latitude, Longitude = ad.Longitude };
        //                //}
        //                //evt.Place = pl;

        //                var obj = new LinkedFacebookObject() { MediaSource = MediaSource.Facebook, Name = evt.Name, FacebookId = evt.FacebookId, Url = evt.FacebookLink, ObjectType = FacebookObjectType.Event };
        //                if (model.Type == EventType.Class)
        //                {
        //                    var cls = new Class() { Name = evt.Name, Description = evt.Description, FacebookId = evt.FacebookId, PhotoUrl = evt.PhotoUrl, StartDate = evt.StartDate, EndDate = evt.EndDate, StartTime = evt.StartTime, EndTime = evt.EndTime, ClassType = model.ClassType != null ? (ClassType)Enum.Parse(typeof(ClassType), model.ClassType.ToString()) : ClassType.Class, Place = evt.Place, FacebookLink = evt.FacebookLink, Creator = user, DanceStyles = DataContext.DanceStyles.Where(s => model.PostedStyles.DanceStyleIds.Any(ps => ps == s.Id.ToString())).ToList(), IsAvailable = evt.IsAvailable, LinkedFacebookObjects = new List<LinkedFacebookObject>() { obj }, Recurring = evt.Recurring, Interval = evt.Interval, Frequency = evt.Frequency, MonthDays = evt.MonthDays, SkillLevel = model.SkillLevel, UpdatedDate = evt.UpdatedDate };
        //                    cls.EventMembers = new List<EventMember>();
        //                    cls.Teachers = new List<Teacher>();
        //                    cls.Owners = new List<Owner>();
        //                    cls.EventMembers.Add(new EventMember() { Event = cls, Member = user, Admin = true });
        //                    //  Get Feed
        //                    var feeds = FacebookHelper.GetFeed(evt.FacebookId, user.FacebookToken).Where(f => f.Link != null || f.Message != null).ToList();
        //                    cls.Feeds = feeds.Where(f => f.Message != null || f.Link != null).Select(f => new Feed() { Link = f.Link, Message = f.Message, UpdateTime = f.Updated_Time }).ToList();
        //                    //  Get Feed

        //                    //  Add Recurring Events
        //                    if (evt.Recurring)
        //                    {
        //                        var sdate = evt.StartDate;
        //                        var edate = evt.EndDate;
        //                        cls.EventInstances = new List<EventInstance>();
        //                        sdate = ApplicationUtility.GetNextDate(sdate, evt.Frequency, (int)evt.Interval, evt.Day, sdate, evt.MonthDays);

        //                        while (sdate <= edate)
        //                        {
        //                            cls.EventInstances.Add(new EventInstance() { EventId = cls.Id, DateTime = sdate });
        //                            sdate = ApplicationUtility.GetNextDate(sdate, evt.Frequency, (int)evt.Interval, evt.Day, sdate.AddDays(1), evt.MonthDays);
        //                        }
        //                    }
        //                    //  Add Recurring Events

        //                    DataContext.Events.Add(cls);
        //                    DataContext.SaveChanges();

        //                    if (user.CurrentRole != null)
        //                    {
        //                        if (user.CurrentRole.Name == "Teacher")
        //                        {
        //                            var teacher = DataContext.Teachers.Where(x => x.ApplicationUser.Id == userid).Include("ApplicationUser").Include("Places").FirstOrDefault();
        //                            DataContext.Teachers.Attach(teacher);
        //                            cls.Teachers.Add(teacher);
        //                            DataContext.Entry(teacher).State = EntityState.Modified;
        //                            DataContext.SaveChanges();
        //                        }
        //                        else if (user.CurrentRole.Name == "Owner")
        //                        {
        //                            var owner = DataContext.Owners.Where(x => x.ApplicationUser.Id == userid).Include("ApplicationUser").FirstOrDefault();
        //                            DataContext.Owners.Attach(owner);
        //                            cls.Owners.Add(owner);
        //                            DataContext.Entry(owner).State = EntityState.Modified;
        //                            DataContext.SaveChanges();
        //                        }
        //                    }
        //                    eventId = cls.Id;
        //                }
        //                else
        //                {
        //                    var social = new Social() { Name = evt.Name, Description = evt.Description, FacebookId = evt.FacebookId, PhotoUrl = evt.PhotoUrl, StartDate = evt.StartDate, EndDate = evt.EndDate, StartTime = evt.StartTime, EndTime = evt.EndTime, SocialType = model.SocialType != null ? (SocialType)Enum.Parse(typeof(SocialType), model.SocialType.ToString()) : SocialType.Social, Place = evt.Place, FacebookLink = evt.FacebookLink, Creator = user, DanceStyles = DataContext.DanceStyles.Where(s => model.PostedStyles.DanceStyleIds.Any(ps => ps == s.Id.ToString())).ToList(), IsAvailable = evt.IsAvailable, LinkedFacebookObjects = new List<LinkedFacebookObject>() { obj }, Recurring = evt.Recurring, Interval = evt.Interval, Frequency = evt.Frequency, MonthDays = evt.MonthDays, UpdatedDate = evt.UpdatedDate };
        //                    social.EventMembers = new List<EventMember>();
        //                    social.Promoters = new List<Promoter>();
        //                    social.Owners = new List<Owner>();
        //                    social.EventMembers.Add(new EventMember() { Event = social, Member = user, Admin = true });
        //                    //  Get Feed
        //                    var feeds = FacebookHelper.GetFeed(evt.FacebookId, user.FacebookToken).Where(f => f.Link != null || f.Message != null).ToList();
        //                    social.Feeds = feeds.Where(f => f.Message != null || f.Link != null).Select(f => new Feed() { Link = f.Link, Message = f.Message, UpdateTime = f.Updated_Time }).ToList();
        //                    //  Get Feed

        //                    //  Add Recurring Events
        //                    if (evt.Recurring)
        //                    {
        //                        var sdate = evt.StartDate;
        //                        var edate = evt.EndDate;
        //                        social.EventInstances = new List<EventInstance>();
        //                        sdate = ApplicationUtility.GetNextDate(sdate, evt.Frequency, (int)evt.Interval, evt.Day, sdate, evt.MonthDays);

        //                        while (sdate <= edate)
        //                        {
        //                            social.EventInstances.Add(new EventInstance() { EventId = social.Id, DateTime = sdate });
        //                            sdate = ApplicationUtility.GetNextDate(sdate, evt.Frequency, (int)evt.Interval, evt.Day, sdate.AddDays(1), evt.MonthDays);
        //                        }
        //                    }
        //                    //  Add Recurring Events

        //                    DataContext.Events.Add(social);
        //                    DataContext.SaveChanges();

        //                    if (user.CurrentRole != null)
        //                    {
        //                        if (user.CurrentRole.Name == "Promoter")
        //                        {
        //                            var promoter = DataContext.Promoters.Where(x => x.ApplicationUser.Id == userid).Include("ApplicationUser").FirstOrDefault();
        //                            DataContext.Promoters.Attach(promoter);
        //                            social.Promoters.Add(promoter);
        //                            DataContext.Entry(social).State = EntityState.Modified;
        //                            DataContext.SaveChanges();
        //                        }
        //                        else if (user.CurrentRole.Name == "Owner")
        //                        {
        //                            var owner = DataContext.Owners.Where(x => x.ApplicationUser.Id == userid).Include("ApplicationUser").FirstOrDefault();
        //                            DataContext.Owners.Attach(owner);
        //                            social.Owners.Add(owner);
        //                            DataContext.Entry(owner).State = EntityState.Modified;
        //                            DataContext.SaveChanges();
        //                        }
        //                    }
        //                    eventId = social.Id;
        //                }
        //                return RedirectToAction("View", "Event", new { id = eventId, eventType = model.Type });
        //            }
        //            else 
        //            {
        //                return RedirectToAction("Home", user.CurrentRole != null ? user.CurrentRole.Name : "Dancer", new { username = User.Identity.Name });
        //            }
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
        //    else
        //    {
        //        //  Load Dance Styles
        //        var styles = new List<DanceStyleListItem>();
        //        foreach (DanceStyle s in DataContext.DanceStyles)
        //        {
        //            styles.Add(new DanceStyleListItem { Id = s.Id, Name = s.Name });
        //        }
        //        model.AvailableStyles = styles.OrderBy(x => x.Name);
        //        if (model.PostedStyles != null)
        //        {
        //            model.SelectedStyles = model.AvailableStyles.Where(s => model.PostedStyles.DanceStyleIds.Contains(s.Id.ToString()));
        //        }
        //        //  Load Dance Styles

        //        //  For Month Days Checkbox List
        //        var selectedMonthDays = new List<SelectListItem>();
        //        string[] daysarray;
        //        model.SelectedMonthDays = new List<SelectListItem>();
        //        if (model.Event.MonthDays != null)
        //        {
        //            daysarray = model.Event.MonthDays.Split(new char[] { '-' });
        //            foreach (var day in daysarray)
        //            {
        //                model.SelectedMonthDays.Add(new SelectListItem() { Value = day, Text = day });
        //            }
        //        }
        //        model.MonthDays = new List<SelectListItem>() { new SelectListItem() { Value = "1", Text = "1st" }, new SelectListItem() { Value = "2", Text = "2nd" }, new SelectListItem() { Value = "3", Text = "3rd" }, new SelectListItem() { Value = "4", Text = "4th" } };
        //        //  For Month Days Checkbox List

        //        //  Set Month day text
        //        var daysofmonth = new string[] { "blank", "1st", "2nd", "3rd", "4th", "5th", "6th", "7th", "8th", "9th", "10th", "11th", "12th", "13th", "14th", "15th", "16th", "17th", "18th", "19th", "20th", "21st", "22nd", "23rd", "24th", "25th", "26th", "27th", "28th", "29th", "30th", "31st" };
        //        model.MonthDay = daysofmonth[model.Event.StartDate.Day];
        //        //  Set Month day text

        //        ////  Load Places
        //        //model.Places = new List<PlaceItem>();
        //        //var blankPlace = new PlaceItem() { Id = 0 };
        //        //model.Places.Add(blankPlace);
        //        //if (model.EventType == EventType.Class)
        //        //{
        //        //    if (Session["MyRole"] != null)
        //        //    {
        //        //        if ((RoleName)Session["MyRole"] == RoleName.Teacher)
        //        //        {
        //        //            foreach (var pl in DataContext.Places.Where(p => p.Teachers.Any(t => t.ApplicationUser.Id == userid)))
        //        //            {
        //        //                model.Places.Add(new PlaceItem() { Address = pl.Address, Address2 = pl.Address2, City = pl.City, Country = pl.Country, FacebookId = pl.FacebookId, FacebookLink = pl.FacebookLink, Filename = pl.Filename, Id = pl.Id, Latitude = pl.Latitude, Longitude = pl.Longitude, Name = pl.Name, PlaceType = pl.PlaceType, State = pl.State, ThumbnailFilename = pl.ThumbnailFilename, Website = pl.Website, Zip = pl.Zip });
        //        //            }
        //        //        }
        //        //        else
        //        //        {
        //        //            foreach (var pl in DataContext.Places.Where(p => p.Owners.Any(o => o.ApplicationUser.Id == userid)))
        //        //            {
        //        //                model.Places.Add(new PlaceItem() { Address = pl.Address, Address2 = pl.Address2, City = pl.City, Country = pl.Country, FacebookId = pl.FacebookId, FacebookLink = pl.FacebookLink, Filename = pl.Filename, Id = pl.Id, Latitude = pl.Latitude, Longitude = pl.Longitude, Name = pl.Name, PlaceType = pl.PlaceType, State = pl.State, ThumbnailFilename = pl.ThumbnailFilename, Website = pl.Website, Zip = pl.Zip });
        //        //            }
        //        //        }
        //        //    }
        //        //}
        //        //else
        //        //{
        //        //    if (Session["MyRole"] != null)
        //        //    {
        //        //        if ((RoleName)Session["MyRole"] == RoleName.Promoter)
        //        //        {
        //        //            foreach (var pl in DataContext.Places.Where(p => p.Promoters.Any(pr => pr.ApplicationUser.Id == userid)))
        //        //            {
        //        //                model.Places.Add(new PlaceItem() { Address = pl.Address, Address2 = pl.Address2, City = pl.City, Country = pl.Country, FacebookId = pl.FacebookId, FacebookLink = pl.FacebookLink, Filename = pl.Filename, Id = pl.Id, Latitude = pl.Latitude, Longitude = pl.Longitude, Name = pl.Name, PlaceType = pl.PlaceType, State = pl.State, ThumbnailFilename = pl.ThumbnailFilename, Website = pl.Website, Zip = pl.Zip });
        //        //            }
        //        //        }
        //        //        else
        //        //        {
        //        //            foreach (var pl in DataContext.Places.Where(p => p.Owners.Any(o => o.ApplicationUser.Id == userid)))
        //        //            {
        //        //                model.Places.Add(new PlaceItem() { Address = pl.Address, Address2 = pl.Address2, City = pl.City, Country = pl.Country, FacebookId = pl.FacebookId, FacebookLink = pl.FacebookLink, Filename = pl.Filename, Id = pl.Id, Latitude = pl.Latitude, Longitude = pl.Longitude, Name = pl.Name, PlaceType = pl.PlaceType, State = pl.State, ThumbnailFilename = pl.ThumbnailFilename, Website = pl.Website, Zip = pl.Zip });
        //        //            }
        //        //        }
        //        //    }
        //        //}
        //        ////  Load Places

        //        return View(model);
        //    }
        //}

        //[Authorize]
        //public ActionResult ConfirmFacebookEvent(string id, EventType? eventType)
        //{
        //    var model = new ConfirmFacebookEvent();
        //    var userid = User.Identity.GetUserId();
        //    var user = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();
        //    model.User = user;

        //    //  Load Dance Styles
        //    var selectedStyles = new List<DanceStyleListItem>();
        //    model.SelectedStyles = selectedStyles;

        //    var styles = new List<DanceStyleListItem>();
        //    foreach (DanceStyle s in DataContext.DanceStyles)
        //    {
        //        styles.Add(new DanceStyleListItem { Id = s.Id, Name = s.Name });
        //    }
        //    model.AvailableStyles = styles.OrderBy(x => x.Name);
        //    //  Load Dance Styles

        //    model.Type = eventType;

        //    var fbevent = FacebookHelper.GetEvent(id, user.FacebookToken, "id,cover,description,end_time,is_date_only,location,name,owner,privacy,start_time,ticket_uri,timezone,updated_time,venue,parent_group");            //  ((List<FacebookEvent>)Session["FacebookEvents"]).Where(x => x.Id == id).FirstOrDefault();
        //    var available = (fbevent.Privacy == "OPEN" || fbevent.Privacy == "FRIENDS") ? true : false;
        //    model.Event = new Event() { Name = fbevent.Name, Description = fbevent.Description, StartDate = fbevent.StartTime, StartTime = fbevent.StartTime, EndDate = fbevent.EndTime, EndTime = fbevent.EndTime, PhotoUrl = fbevent.CoverPhoto.LargeSource, FacebookId = fbevent.Id, FacebookLink = fbevent.EventLink, Interval = 1, IsAvailable = available, UpdatedDate = fbevent.Updated };

        //    //  For Month Days Checkbox List
        //    var selectedMonthDays = new List<SelectListItem>();
        //    string[] daysarray;
        //    model.SelectedMonthDays = new List<SelectListItem>();
        //    if (model.Event.MonthDays != null)
        //    {
        //        daysarray = model.Event.MonthDays.Split(new char[] { '-' });
        //        foreach (var day in daysarray)
        //        {
        //            model.SelectedMonthDays.Add(new SelectListItem() { Value = day, Text = day });
        //        }
        //    }
        //    model.MonthDays = new List<SelectListItem>() { new SelectListItem() { Value = "1", Text = "1st" }, new SelectListItem() { Value = "2", Text = "2nd" }, new SelectListItem() { Value = "3", Text = "3rd" }, new SelectListItem() { Value = "4", Text = "4th" } };
        //    //  For Month Days Checkbox List

        //    //  Set Month day text
        //    var daysofmonth = new string[] { "blank", "1st", "2nd", "3rd", "4th", "5th", "6th", "7th", "8th", "9th", "10th", "11th", "12th", "13th", "14th", "15th", "16th", "17th", "18th", "19th", "20th", "21st", "22nd", "23rd", "24th", "25th", "26th", "27th", "28th", "29th", "30th", "31st" };
        //    model.MonthDay = daysofmonth[model.Event.StartDate.Day];
        //    //  Set Month day text

        //    //  Extract Place from Facebook
        //    if (fbevent.Address != null)
        //    {
        //        //  Place is a Page in Facebook
        //        if (fbevent.Address.FacebookId != null)
        //        {
        //            var place = DataContext.Places.Where(p => p.FacebookId == fbevent.Address.FacebookId).FirstOrDefault();

        //            //  Existing Facebook place in the database
        //            if (place != null)
        //            {
        //                model.Event.Place = place;
        //            }
        //            //  New Place from Facebook
        //            else
        //            {
        //                //  Place has a Page in Facebook
        //                if (fbevent.Address.FacebookId != null)
        //                {
        //                    var fbplace = FacebookHelper.GetData(user.FacebookToken, fbevent.Address.FacebookId);

        //                    var placetype = new PlaceType();
        //                    if (fbplace.category_list != null)
        //                    {
        //                        foreach (dynamic category in fbplace.category_list)
        //                        {
        //                            string cat = category.name;
        //                            //  Search for Dance Instruction category
        //                            if (cat.Contains("Dance Instruction") || category.id == "203916779633178")
        //                            {
        //                                placetype = PlaceType.Studio;
        //                                break;
        //                            }
        //                            else if (cat.Contains("Dance Club") || category.id == "176139629103647")
        //                            {
        //                                placetype = PlaceType.Nightclub;
        //                                break;
        //                            }
        //                            else if (category.id == "273819889375819" || cat.Contains("Restaurant"))
        //                            {
        //                                placetype = PlaceType.Restaurant;
        //                                break;
        //                            }
        //                            else if (cat.Contains("Hotel") || category.id == "164243073639257")
        //                            {
        //                                placetype = PlaceType.Hotel;
        //                                break;
        //                            }
        //                            else if (cat.Contains("Meeting Room") || category.id == "210261102322291")
        //                            {
        //                                placetype = PlaceType.ConferenceCenter;
        //                                break;
        //                            }
        //                            else if (cat.Contains("Theater") || category.id == "173883042668223")
        //                            {
        //                                placetype = PlaceType.Theater;
        //                                break;
        //                            }
        //                            else
        //                            {
        //                                placetype = PlaceType.OtherPlace;
        //                            }
        //                        }
        //                    }

        //                    model.Event.Place = new Place() { Name = fbevent.Location, Address = fbevent.Address.Street, City = fbevent.Address.City, State = fbevent.Address.State != null ? (State)Enum.Parse(typeof(State), fbevent.Address.State) : State.CA, Zip = fbevent.Address.ZipCode, Country = fbevent.Address.Country, Latitude = fbevent.Address.Latitude, Longitude = fbevent.Address.Longitude, FacebookId = fbevent.Address.FacebookId, PlaceType = placetype, Public = true, Website = fbevent.Address.WebsiteUrl, FacebookLink = fbevent.Address.FacebookUrl, Filename = fbplace.cover != null ? fbplace.cover.source : null, ThumbnailFilename = fbplace.cover != null ? fbplace.cover.source : null };
        //                }
        //            }
        //        }
        //        //  Place does not have a Facebook Page
        //        else
        //        {
        //            model.Event.Place = new Place() { Name = fbevent.Location, Address = fbevent.Address.Street, City = fbevent.Address.City, State = fbevent.Address.State != null ? (State)Enum.Parse(typeof(State), fbevent.Address.State) : State.CA, Zip = fbevent.Address.ZipCode, Country = fbevent.Address.Country, Latitude = fbevent.Address.Latitude, Longitude = fbevent.Address.Longitude, Public = false, Website = fbevent.Address.WebsiteUrl, FacebookLink = fbevent.Address.FacebookUrl };
        //        }
        //    }
        //    //  Place not in Facebook
        //    else
        //    {
        //        model.Event.Place = new Place() { PlaceType = PlaceType.OtherPlace, Public = false };
        //    }
        //    //  Extract Place

        //    //model.PlaceId = -1;
        //    //model.NewPlace = new PlaceItem() { Id = 0, Name = fbevent.Location, Address = fbevent.Address.Street, City = fbevent.Address.City, State = fbevent.Address.State != null ? (State)Enum.Parse(typeof(State), fbevent.Address.State) : State.CA, Zip = fbevent.Address.ZipCode, Latitude = fbevent.Address.Latitude, Longitude = fbevent.Address.Longitude, FacebookId = fbevent.Address.FacebookId };
        //    //model.Places = new List<PlaceItem>();
        //    //var blankPlace = new PlaceItem() { Id = 0 };
        //    //model.Places.Add(blankPlace);
        //    //if (eventType == EventType.Class)
        //    //{
        //    //    if (Session["MyRole"] != null)
        //    //    {
        //    //        if ((RoleName)Session["MyRole"] == RoleName.Teacher)
        //    //        {
        //    //            foreach (var pl in DataContext.Places.Where(p => p.Teachers.Any(t => t.ApplicationUser.Id == userid)))
        //    //            {
        //    //                model.Places.Add(new PlaceItem() { Address = pl.Address, Address2 = pl.Address2, City = pl.City, Country = pl.Country, FacebookId = pl.FacebookId, FacebookLink = pl.FacebookLink, Filename = pl.Filename, Id = pl.Id, Latitude = pl.Latitude, Longitude = pl.Longitude, Name = pl.Name, PlaceType = pl.PlaceType, State = pl.State, ThumbnailFilename = pl.ThumbnailFilename, Website = pl.Website, Zip = pl.Zip });
        //    //            }
        //    //        }
        //    //        else
        //    //        {
        //    //            foreach (var pl in DataContext.Places.Where(p => p.Owners.Any(o => o.ApplicationUser.Id == userid)))
        //    //            {
        //    //                model.Places.Add(new PlaceItem() { Address = pl.Address, Address2 = pl.Address2, City = pl.City, Country = pl.Country, FacebookId = pl.FacebookId, FacebookLink = pl.FacebookLink, Filename = pl.Filename, Id = pl.Id, Latitude = pl.Latitude, Longitude = pl.Longitude, Name = pl.Name, PlaceType = pl.PlaceType, State = pl.State, ThumbnailFilename = pl.ThumbnailFilename, Website = pl.Website, Zip = pl.Zip });
        //    //            }
        //    //        }
        //    //    }
        //    //}
        //    //else
        //    //{
        //    //    if (Session["MyRole"] != null)
        //    //    {
        //    //        if ((RoleName)Session["MyRole"] == RoleName.Promoter)
        //    //        {
        //    //            foreach (var pl in DataContext.Places.Where(p => p.Promoters.Any(pr => pr.ApplicationUser.Id == userid)))
        //    //            {
        //    //                model.Places.Add(new PlaceItem() { Address = pl.Address, Address2 = pl.Address2, City = pl.City, Country = pl.Country, FacebookId = pl.FacebookId, FacebookLink = pl.FacebookLink, Filename = pl.Filename, Id = pl.Id, Latitude = pl.Latitude, Longitude = pl.Longitude, Name = pl.Name, PlaceType = pl.PlaceType, State = pl.State, ThumbnailFilename = pl.ThumbnailFilename, Website = pl.Website, Zip = pl.Zip });
        //    //            }
        //    //        }
        //    //        else
        //    //        {
        //    //            foreach (var pl in DataContext.Places.Where(p => p.Owners.Any(o => o.ApplicationUser.Id == userid)))
        //    //            {
        //    //                model.Places.Add(new PlaceItem() { Address = pl.Address, Address2 = pl.Address2, City = pl.City, Country = pl.Country, FacebookId = pl.FacebookId, FacebookLink = pl.FacebookLink, Filename = pl.Filename, Id = pl.Id, Latitude = pl.Latitude, Longitude = pl.Longitude, Name = pl.Name, PlaceType = pl.PlaceType, State = pl.State, ThumbnailFilename = pl.ThumbnailFilename, Website = pl.Website, Zip = pl.Zip });
        //    //            }
        //    //        }
        //    //    }
        //    //}

        //    //if (fbevent.Address.Street != null && fbevent.Address.City != null && fbevent.Address.State != null && fbevent.Address.ZipCode != null)
        //    //{
        //    //    var fbeventAddress = Geolocation.ParseAddress(fbevent.Address.Street + " " + fbevent.Address.City + ", " + fbevent.Address.State + " " + fbevent.Address.ZipCode);
        //    //    var pl = model.Places.Where(p => p.Address == fbeventAddress.Street && p.City == fbeventAddress.City && p.State.ToString() == fbeventAddress.State).FirstOrDefault();
        //    //    if (pl != null)
        //    //    {
        //    //        pl.Selected = true;
        //    //    }
        //    //    else
        //    //    {
        //    //        blankPlace.Selected = true;
        //    //    }
        //    //}
        //    //else
        //    //{
        //    //    blankPlace.Selected = true;
        //    //}

        //    return View(model);
        //}

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

        //[Authorize]
        //public PartialViewResult PostReview(EventViewModel model)
        //{
        //    var userid = User.Identity.GetUserId();
        //    if (model.Review.Id == 0)
        //    {
        //        var auth = DataContext.Users.Where(x => x.Id == userid).FirstOrDefault();
        //        var ev = DataContext.Events.Where(e => e.Id == model.Event.Id).Include("Reviews").FirstOrDefault();
        //        ev.Reviews.Add(new Review() { ReviewText = model.Reviews.NewReview.ReviewText, ReviewDate = DateTime.Now, Like = model.Reviews.NewReview.Like, Author = auth });
        //        DataContext.SaveChanges();

        //        var reviews = DataContext.Reviews.Where(x => x.Event.Id == model.Event.Id);
        //        return PartialView("~/Views/Shared/Events/_ReviewsPartial.cshtml", reviews);
        //    }
        //    else
        //    {
        //        model.Review.ReviewDate = DateTime.Today;
        //        DataContext.Entry(model.Review).State = EntityState.Modified;
        //        DataContext.SaveChanges();
        //        return PartialView();
        //    }
            
        //    //if (ModelState.IsValid)
        //    //{
        //    //    return RedirectToAction("Class", "Event", new { id = model.Event.Id});
        //    //}
        //    //else
        //    //{
        //    //    var allErrors = ModelState.Values.SelectMany(v => v.Errors);
        //    //}
        //    //return View(model);
        //}

        [Authorize]
        [HttpPost]
        public JsonResult PostReviewAsync(int id, int eventId, int rating, string review)
        {
            var userid = User.Identity.GetUserId();
            var auth = DataContext.Users.Where(x => x.Id == userid).FirstOrDefault();
            if (id == 0)
            {
                var ev = DataContext.Events.Where(e => e.Id == eventId).Include("Reviews").FirstOrDefault();
                var rev = new Review() { ReviewText = review, ReviewDate = DateTime.Now, Author = auth, Rating = rating };
                ev.Reviews.Add(rev);
                DataContext.SaveChanges();

                //  var reviews = DataContext.Reviews.Where(x => x.Event.Id == eventId);
                var objUpload = new { Id = rev.Id, ReviewDate = rev.ReviewDate, Rating = rev.Rating, Review = rev.ReviewText };
                return Json(objUpload, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var rev = new Review() { Id = id, ReviewText = review, ReviewDate = DateTime.Now, Author = auth, Rating = rating };
                DataContext.Entry(rev).State = EntityState.Modified;
                DataContext.SaveChanges();
                var objUpload = new { Id = rev.Id, ReviewDate = rev.ReviewDate, Rating = rev.Rating, Review = rev.ReviewText };
                return Json(objUpload, JsonRequestBehavior.AllowGet);
            }
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
            return RedirectToAction(evt is Class ? EventType.Class.ToString() : EventType.Social.ToString(), "Event", new { id = id });
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
            return RedirectToAction(evt is Class ? EventType.Class.ToString() : EventType.Social.ToString(), "Event", new { id = id });
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

        //[Authorize]
        //public ActionResult AddTeacher(int id)
        //{
        //    var model = new AddTeacherViewModel();
        //    var clss = DataContext.Events.OfType<Class>().Where(e => e.Id == id).FirstOrDefault();
        //    model.Class = clss;

        //    //  Load Dance Styles
        //    var styles = new List<DanceStyleListItem>();
        //    foreach (DanceStyle s in DataContext.DanceStyles)
        //    {
        //        styles.Add(new DanceStyleListItem { Id = s.Id, Name = s.Name });
        //    }
        //    model.AvailableStyles = styles.OrderBy(x => x.Name);
        //    //  Load Dance Styles

        //    model.Teachers = DataContext.Teachers.Include("ApplicationUser").Where(t => !t.Classes.Any(c => c.Id == model.Class.Id)).ToList();

        //    //  Search based on location
        //    if (model.Location != null)
        //    {
        //        var address = Geolocation.ParseAddress(model.Location);
        //        var NELat = address.Latitude + .5;
        //        var SWLat = address.Latitude - .5;
        //        var NELng = address.Longitude + .5;
        //        var SWLng = address.Longitude - .5;
        //        model.Teachers = model.Teachers.Where(t => t.ApplicationUser.Longitude >= SWLng && t.ApplicationUser.Longitude <= NELng && t.ApplicationUser.Latitude >= SWLat && t.ApplicationUser.Latitude <= NELat).ToList();
        //    }

        //    //  Search By Last Name
        //    if (model.LastName != null)
        //    {
        //        model.Teachers = model.Teachers.Where(t => t.ApplicationUser.LastName.Contains(model.LastName)).ToList();
        //    }

        //    //  Search By First Name
        //    if (model.LastName != null)
        //    {
        //        model.Teachers = model.Teachers.Where(t => t.ApplicationUser.FirstName.Contains(model.FirstName)).ToList();
        //    }

        //    return View(model);
        //}

        //[Authorize]
        //[HttpPost]
        //public ActionResult AddTeacher(AddTeacherViewModel model)
        //{
        //    var clss = DataContext.Events.OfType<Class>().Where(e => e.Id == model.Class.Id).FirstOrDefault();
        //    model.Class = clss;
        //    //  Load Dance Styles
        //    var styles = new List<DanceStyleListItem>();
        //    foreach (DanceStyle s in DataContext.DanceStyles)
        //    {
        //        styles.Add(new DanceStyleListItem { Id = s.Id, Name = s.Name });
        //    }
        //    model.AvailableStyles = styles.OrderBy(x => x.Name);
        //    if (model.PostedStyles != null)
        //    {
        //        model.SelectedStyles = model.AvailableStyles.Where(s => model.PostedStyles.DanceStyleIds.Contains(s.Id.ToString()));
        //    }
        //    //  Load Dance Styles

        //    model.Teachers = DataContext.Teachers.Include("ApplicationUser").Where(t => !t.Classes.Any(c => c.Id == model.Class.Id)).ToList();

        //    //  Search based on location
        //    if (model.Location != null)
        //    {
        //        var address = Geolocation.ParseAddress(model.Location);
        //        var NELat = address.Latitude + .5;
        //        var SWLat = address.Latitude - .5;
        //        var NELng = address.Longitude + .5;
        //        var SWLng = address.Longitude - .5;
        //        model.Teachers = model.Teachers.Where(t => t.ApplicationUser.Longitude >= SWLng && t.ApplicationUser.Longitude <= NELng && t.ApplicationUser.Latitude >= SWLat && t.ApplicationUser.Latitude <= NELat).ToList();
        //    }

        //    //  Search By First Name
        //    if (model.FirstName != null)
        //    {
        //        model.Teachers = model.Teachers.Where(t => t.ApplicationUser.FirstName.IndexOf(model.FirstName, StringComparison.CurrentCultureIgnoreCase) != -1).ToList();
        //    }

        //    //  Search By Last Name
        //    if (model.LastName != null)
        //    {
        //        model.Teachers = model.Teachers.Where(t => t.ApplicationUser.LastName.IndexOf(model.LastName, StringComparison.CurrentCultureIgnoreCase) != -1).ToList();
        //    }

        //    //  Search By Dance Styles
        //    if (model.PostedStyles != null)
        //    {
        //        model.Teachers = model.Teachers.Where(t => t.DanceStyles.Any(s => model.PostedStyles.DanceStyleIds.Contains(s.Id.ToString()))).ToList();
        //    }

        //    return View(model);
        //}

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

            return RedirectToAction("Class", "Event", new { id = id });
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

            return RedirectToAction("Class", "Event", new { id = id });
        }

        [Authorize]
        public ActionResult EditFees(int id)
        {
            var model = new EventFeeViewModel();
            model.Event = DataContext.Events.Where(e => e.Id == id).Include("EventTickets").Include("EventTickets.Ticket").FirstOrDefault();
            model.SchoolId = DataContext.Classes.Where(c => c.Id == id).FirstOrDefault().SchoolId;

            return View(model);
        }

        [Authorize(Roles = "Owner,Promoter,Teacher")]
        public ActionResult DeleteInstance(int id, string returnUrl)
        {
            var instance = DataContext.EventInstances.Include("EventRegistrations").Where(i => i.Id == id).FirstOrDefault();
            var eventid = instance.EventId;

            if (instance.EventRegistrations.Count() == 0)
            {
                DataContext.EventInstances.Remove(instance);
                DataContext.SaveChanges();
            }

            if (returnUrl != null)
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Manage", new { id = eventid });
            }
        }

        [Authorize(Roles = "Owner,Promoter,Teacher")]
        public ActionResult DeleteRegistration(int id)
        {
            var registration = DataContext.EventRegistrations.Where(i => i.Id == id).FirstOrDefault();
            var instanceid = registration.EventInstanceId;

            DataContext.EventRegistrations.Remove(registration);
            DataContext.SaveChanges();

            return RedirectToAction("ManageInstance", new { id = instanceid });
        }

        [Authorize(Roles = "Owner,Promoter,Teacher")]
        public ActionResult CheckinRegistration(int id)
        {
            var registration = DataContext.EventRegistrations.Where(i => i.Id == id).FirstOrDefault();
            registration.Checkedin = registration.Checkedin == null ? (DateTime?)DateTime.Now : null;
            var instanceid = registration.EventInstanceId;
            DataContext.SaveChanges();

            return RedirectToAction("ManageInstance", new { id = instanceid });
        }

        public ActionResult CheckinByCode(int id)
        {
            var registration = DataContext.EventRegistrations.Where(i => i.Id == id).FirstOrDefault();
            string message;
            if (registration != null)
            {
                if (DateTime.Now >= ((DateTime)registration.Instance.StartTime).AddMinutes(-15))
                {
                    registration.Checkedin = registration.Checkedin == null ? (DateTime?)DateTime.Now : null;
                    var instanceid = registration.EventInstanceId;
                    DataContext.Entry(registration).State = EntityState.Modified;
                    DataContext.SaveChanges();

                    message = (registration.FirstName ?? registration.User.FirstName) + " is checked in";
                }
                else
                {
                    message = "Event hasn't started yet";
                }
            }
            else
            {
                message = "Code Not Found";
            }

            return RedirectToAction("ScanRegistrants", new { message = message });
        }

        public ActionResult ScanRegistrants(string message)
        {
            ViewBag.Message = message;
            if (HttpContext.Request.Browser.IsMobileDevice)
            {
                return View("Mobile/ScanRegistrants");
            }
            else
            {
                return View();
            }
        }

        [Authorize(Roles = "Owner,Promoter,Teacher")]
        [HttpPost]
        public ActionResult ScanRegistrants(ScanRegistrantsViewModel model)
        {
            var registration = DataContext.EventRegistrations.Where(i => i.Id == model.RegistrationId).FirstOrDefault();
            if (registration != null)
            {
                registration.Checkedin = registration.Checkedin == null ? (DateTime?)DateTime.Now : null;
                var instanceid = registration.EventInstanceId;
                DataContext.SaveChanges();

                ViewBag.Message = "Registrant Checked In";
            }
            else
            {
                ViewBag.Message = "Code Not Found";
            }

            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult EditFees(Event evnt)
        {
            return RedirectToAction((evnt is Class ? EventType.Class : EventType.Social).ToString(), "Event", new { id = evnt.Id });
        }

        [Authorize(Roles = "Teacher,Owner,Admin")]
        [HttpPost]
        public ActionResult AddTeacher(FormCollection formCollection)
        {
            int id = Convert.ToInt32(formCollection["id"]);
            string teacherid = formCollection["teacherid"];
            if (DataContext.Classes.Where(c => c.Id == id && c.Teachers.Any(t => t.ApplicationUser.Id == teacherid)).Count() == 0)
            {
                DataContext.Classes.Single(s => s.Id == id).Teachers.Add(DataContext.Teachers.Single(t => t.ApplicationUser.Id == teacherid));
                DataContext.SaveChanges();
            }
            return RedirectToAction("Manage", new { id = id, eventType = EventType.Class });
        }

        [Authorize(Roles = "Teacher,Owner,Admin")]
        public ActionResult RemoveTeacher(int id, int teacherid)
        {
            DataContext.Classes.Where(s => s.Id == id).Include("Teachers").FirstOrDefault().Teachers.Remove(DataContext.Teachers.Single(t => t.Id == teacherid));
            DataContext.SaveChanges();
            return RedirectToAction("Manage", new { id = id, eventType = EventType.Class });
        }

        [Authorize(Roles = "Promoter,Owner,Admin")]
        [HttpPost]
        public ActionResult AddPromoter(FormCollection formCollection)
        {
            int id = Convert.ToInt32(formCollection["id"]);
            string promid = formCollection["promoterid"];
            if (DataContext.Socials.Where(c => c.Id == id && c.Promoters.Any(t => t.ApplicationUser.Id == promid)).Count() == 0)
            {
                DataContext.Socials.Single(s => s.Id == id).Promoters.Add(DataContext.Promoters.Single(t => t.ApplicationUser.Id == promid));
                DataContext.SaveChanges();
            }
            return RedirectToAction("Manage", new { id = id, eventType = EventType.Social });
        }

        [Authorize(Roles = "Promoter,Owner,Admin")]
        public ActionResult RemovePromoter(int id, int promoterid)
        {
            DataContext.Socials.Where(s => s.Id == id).Include("Promoters").FirstOrDefault().Promoters.Remove(DataContext.Promoters.Single(t => t.Id == promoterid));
            DataContext.SaveChanges();
            return RedirectToAction("Manage", new { id = id, eventType = EventType.Social });
        }

        ////
        //// POST: /Checkout/AddressAndPayment
        //[HttpPost]
        //public ActionResult AddFee(FormCollection values)
        //{
        //    var ticket = new Ticket();

        //    var eid = Convert.ToInt32(values["Event.Id"]);
        //    var evnt = DataContext.Events.Where(e => e.Id == eid).Include("EventTickets").FirstOrDefault();

        //    try
        //    {
        //        ticket.Price = Convert.ToDecimal(values["Fee"]);
        //        ticket.Quantity = Convert.ToDecimal(values["Quantity"]);
        //        ticket.SchoolId = Convert.ToInt32(values["SchoolId"]);

        //        evnt.EventTickets.Add(new EventTicket() { Ticket = ticket });

        //        DataContext.Entry(evnt).State = EntityState.Modified;
        //        DataContext.SaveChanges();

        //        return RedirectToAction("EditFees", "Event", new { id = evnt.Id, eventType = (evnt is Class ? EventType.Class : EventType.Social) });
        //    }
        //    catch
        //    {
        //        //Invalid - redisplay with errors
        //        return View(ticket);
        //    }
        //}

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
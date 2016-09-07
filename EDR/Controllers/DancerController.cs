using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using EDR.Models.ViewModels;
using System.Data.Entity;
using EDR.Models;
using Facebook;
using Microsoft.AspNet.Facebook;
using Microsoft.AspNet.Facebook.Client;
using System.Xml.Linq;
using EDR.Utilities;
using System.Web;
using System.IO;
using System.Web.Helpers;
using Google.Apis.Auth.OAuth2;
using System.Threading;
using Google.Apis.Services;
using System.Data.Entity.Spatial;
using DayPilot.Web.Mvc;
using DayPilot.Web.Mvc.Enums;
using DayPilot.Web.Mvc.Events.Month;
using EDR.Data;
using DHTMLX.Scheduler;
using DHTMLX.Scheduler.Controls;
using DHTMLX.Scheduler.Data;
using EDR.Enums;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Configuration;

namespace EDR.Controllers
{
    public class DancerController : BaseController
    {
        [Route("Manage")]
        [Authorize]
        public ActionResult Manage()
        {
            var model = LoadDancer();
            if (model.Dancer.NewPassword)
            {
                return RedirectToAction("ChangePassword", "Account");
            }
            else
            {
                if (HttpContext.Request.Browser.IsMobileDevice)
                {
                    return View("Mobile/Manage", model);
                }
                else
                {
                    return View(model);
                }
            }
        }

        private DancerManageViewModel LoadDancer()
        {
            var userid = User.Identity.GetUserId();
            var model = new DancerManageViewModel();
            model.Dancer = DataContext.Users
                            .Include("Tickets")
                            .Include("Tickets.EventRegistrations")
                            .Include("Tickets.Ticket")
                            .Include("Tickets.Ticket.School")
                            .Include("EventRegistrations")
                            .Include("EventRegistrations.Instance")
                            .Include("EventRegistrations.Instance.Event")
                            .Include("OrganizationMembers.Organization")
                            .Single(s => s.Id == userid);

            if (model.Dancer.FacebookToken != null)
            {
                model.FacebookPictures = FacebookHelper.GetPhotos(model.Dancer.FacebookToken);
            }

            //  Get Spotify Token for user
            if (model.Dancer.SpotifyRefreshToken != null)
            {
                var accesstoken = SpotifyHelper.GetAccessToken(model.Dancer.SpotifyRefreshToken, SpotifyGrantType.refresh_token);
                if (accesstoken.Access_Token != null)
                {
                    model.Dancer.SpotifyToken = accesstoken.Access_Token;
                    model.Dancer.SpotifyRefreshToken = accesstoken.Refresh_Token;
                }
                else
                {
                    model.Dancer.SpotifyToken = null;
                }
            }
            if (model.Dancer.SpotifyToken != null)
            {
                var token = new SpotifyAccessToken() { Access_Token = model.Dancer.SpotifyToken, Refresh_Token = model.Dancer.SpotifyRefreshToken };
                model.SpotifyPlaylists = SpotifyHelper.GetPlaylists(ref token, model.Dancer.SpotifyId);
            }
            //  Get Spotify Token for user

            return model;
        }

        [Route("Manage")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Manage(DancerManageViewModel model)
        {
            if (ModelState.IsValid)
            {
                var dancer = DataContext.Users.Single(d => d.Id == model.Dancer.Id);
                TryUpdateModel(dancer, "Dancer");
                DataContext.Entry(dancer).State = EntityState.Modified;
                DataContext.SaveChanges();

                model = LoadDancer();

                if (HttpContext.Request.Browser.IsMobileDevice)
                {
                    return View("Mobile/Manage", model);
                }
                else
                {
                    return View(model);
                }
            }
            if (HttpContext.Request.Browser.IsMobileDevice)
            {
                return View("Mobile/Manage", model);
            }
            else
            {
                return View(model);
            }
        }

        [Route("Dancer/List")]
        public ActionResult List()
        {
            var model = new DancerListViewModel();
            model.Dancers = DataContext.Users;

            return View(model);
        }

        private DancerViewViewModel LoadDancerModel(string username)
        {
            var viewModel = new DancerViewViewModel();

            if (String.IsNullOrWhiteSpace(username))
            {
                if (User != null)
                {
                    username = User.Identity.GetUserName();
                }
            }

            var user = DataContext.Users.Where(u => u.UserName == username).FirstOrDefault();

            var dancer = DataContext.Users.Where(x => x.UserName == username)
                                .Include("DanceStyles")
                                .Include("UserPictures")
                                .FirstOrDefault();
            viewModel.Teachers = DataContext.Teachers.Where(t => t.Classes.Any(c => c.EventInstances.Any(i => i.EventRegistrations.Any(r => r.UserId == user.Id)))).Distinct().ToList();

            if (dancer != null)
            {
                viewModel.Dancer = dancer;

                //if (viewModel.Dancer.ZipCode != null)
                //{
                //    viewModel.Address = Geolocation.ParseAddress(viewModel.Dancer.ZipCode);
                //}
                //else
                //{
                //    viewModel.Address = Geolocation.ParseAddress("90065");
                //}
            }

            //  Load Roles
            viewModel.Roles = new List<RoleName>();
            if (UserManager.IsInRole(viewModel.Dancer.Id, "Owner"))
            {
                viewModel.Roles.Add(RoleName.Owner);
            }
            if (UserManager.IsInRole(viewModel.Dancer.Id, "Promoter"))
            {
                viewModel.Roles.Add(RoleName.Promoter);
            }
            if (UserManager.IsInRole(viewModel.Dancer.Id, "Teacher"))
            {
                viewModel.Roles.Add(RoleName.Teacher);
            }
            //  Load Roles

            return viewModel;
        }

        [Route("Dancer/Search")]
        [Authorize]
        public JsonResult Search(string searchString)
        {
            var users = DataContext.Users.Where(u => (u.FirstName + " " + u.LastName).Contains(searchString)).Select(s => new { Id = s.Id, Name = s.FirstName + " " + s.LastName }).ToList();

            if (users.Count() == 0)
            {
                users.Add(new { Id = "", Name = "No results" });
            }
            return Json(users, JsonRequestBehavior.AllowGet);
        }

        [Route("Dancer/GetFacebookPictures")]
        [Authorize]
        public JsonResult GetFacebookPictures()
        {
            var userid = User.Identity.GetUserId();
            var user = DataContext.Users.Single(u => u.Id == userid);
            var photos = new List<FacebookPhoto>();
            if (user.FacebookToken != null)
            {
                photos = FacebookHelper.GetPhotos(user.FacebookToken);
            }
            return Json(photos.Select(p => new { Url = p.LargeSource, Thumbnail = p.Source }), JsonRequestBehavior.AllowGet);
        }

        [Route("Dancer/GetFacebookPicturesPartial")]
        [Authorize]
        [HttpGet]
        public virtual ActionResult GetFacebookPicturesPartial()
        {
            var userid = User.Identity.GetUserId();
            var user = DataContext.Users.Single(u => u.Id == userid);
            var photos = new List<FacebookPhoto>();
            if (user.FacebookToken != null)
            {
                photos = EDR.Utilities.FacebookHelper.GetPhotos(user.FacebookToken);
            }
            return PartialView("~/Views/Shared/_PickFacebookPicturePartial.cshtml", photos);
        }

        [Route("Dancer/GetClassesPartial")]
        [HttpGet]
        public virtual ActionResult GetClassesPartial(string id)
        {
            var start = DateTime.Today;
            var classes = DataContext.Classes
                                .Include("DanceStyles")
                                .Include("Place")
                                .Include("EventInstances.EventRegistrations")
                                .Include("Reviews")
                                .Include("Teachers.ApplicationUser")
                                .Where(c => c.EventInstances.Any(i => i.DateTime >= start)
                                        && c.EventInstances.Any(i => i.EventRegistrations.Any(r => r.User.Id == id)));
            return PartialView("~/Views/Shared/_EventsPartial.cshtml", classes);
        }

        [Route("Dancer/GetSocialsPartial")]
        [HttpGet]
        public virtual ActionResult GetSocialsPartial(string id)
        {
            var start = DateTime.Today;
            var socials = DataContext.Socials
                                .Include("DanceStyles")
                                .Include("Place")
                                .Include("EventInstances.EventRegistrations")
                                .Include("Reviews")
                                .Where(c => c.EventInstances.Any(i => i.DateTime >= start)
                                        && c.EventInstances.Any(i => i.EventRegistrations.Any(r => r.User.Id == id)));
            return PartialView("~/Views/Shared/_EventsPartial.cshtml", socials);
        }

        //[Authorize]
        //public ActionResult View(string username)
        //{
        //    if (username == "View")
        //    {
        //        RedirectToAction("View", "Dancer", User.Identity.Name);
        //    }

        //    var viewModel = LoadDancerModel(username);

        //    if (viewModel.Dancer == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }

        //    //if (User.Identity.IsAuthenticated && username == "View")
        //    //{
        //    //    username = User.Identity.Name;
        //    //}
        //    //if (String.IsNullOrWhiteSpace(username))
        //    //{
        //    //    if (User != null)
        //    //    {
        //    //        username = User.Identity.GetUserName();
        //    //    }
        //    //    else
        //    //    {
        //    //    }
        //    //}

        //    //var dancer = DataContext.Users.Where(x => x.UserName == username).Include("DanceStyles").Include("UserPictures").FirstOrDefault();
        //    //if(dancer == null)
        //    //{
        //    //    return HttpNotFound();
        //    //}

        //    var today = DateTime.Today;
        //    var id = User.Identity.GetUserId();

        //    //var viewModel = new DancerViewViewModel();
        //    //viewModel.Dancer = dancer;
        //    viewModel.Teachers = new List<Teacher>();
        //    viewModel.Teachers = DataContext.Teachers.Where(x => x.Students.Any(s => s.DancerId == id)).Include("ApplicationUser").Include("ApplicationUser.UserPictures").ToList();
        //    viewModel.Classes = new EventListViewModel();
        //    var classes = DataContext.Events.OfType<Class>().Where(x => x.Users.Any(u => u.UserName == username)).Include("Users").Include("Teachers").ToList();
        //    viewModel.Classes.Events = classes.Where(e => e.NextDate >= today);
        //    viewModel.Socials = new EventListViewModel();
        //    viewModel.Socials.Events = DataContext.Events.OfType<Social>().Where(x => x.Users.Any(u => u.UserName == username)).ToList();
        //    viewModel.SuggestedClasses = new List<Class>();
        //    //  y.DanceStyles.Any(x => dancer.DanceStyles.Contains(x)) && y.NextDate >= today && 
        //    var myLocation = DbGeography.FromText("POINT(" + viewModel.Address.Longitude.ToString() + " " + viewModel.Address.Latitude.ToString() + ")");
        //    //  viewModel.SuggestedEvents = DataContext.Events.Where(y => y.DanceStyles.Any(x => dancer.DanceStyles.Contains(x)) && DbGeography.FromText("POINT(" + y.Place.Longitude.ToString() + " " + y.Place.Latitude.ToString() + ")").Distance(myLocation) * .00062 < 50).ToList(); // In Miles

        //    var arrStyles = viewModel.Dancer.DanceStyles.Select(s => s.Id).ToArray();
        //    var suggestedClasses = DataContext.Events.OfType<Class>()
        //                                .Where(y => y.DanceStyles.Any(d => arrStyles.Contains(d.Id)) && DbGeography.FromText("POINT(" + y.Place.Longitude.ToString() + " " + y.Place.Latitude.ToString() + ")").Distance(myLocation) * .00062 < 50 && !y.Users.Any(u => u.Id == id))
        //                                .Include("Teachers")
        //                                .Include("Teachers.ApplicationUser")
        //                                .ToList();
        //    viewModel.SuggestedClasses = suggestedClasses.Where(e => e.NextDate >= today);

        //    viewModel.SuggestedSocials = new List<Social>();
        //    var suggestedSocials = DataContext.Events.OfType<Social>()
        //                                .Where(y => y.DanceStyles.Any(d => arrStyles.Contains(d.Id)) && DbGeography.FromText("POINT(" + y.Place.Longitude.ToString() + " " + y.Place.Latitude.ToString() + ")").Distance(myLocation) * .00062 < 50 && !y.Users.Any(u => u.Id == id))
        //                                .ToList();
        //    viewModel.SuggestedSocials = suggestedSocials.Where(e => e.NextDate >= today);

        //    //  viewModel.SuggestedEvents = DataContext.Events.Where(y => y.DanceStyles.Any(x => styles.Any(z => z.Id == x.Id))).ToList(); // In Miles
        //    //  viewModel.SuggestedEvents = DataContext.Events.Where(y => Geolocation.Distance(new Geolocation.Position() { Latitude = y.Place.Latitude, Longitude = y.Place.Longitude }, new Geolocation.Position() { Latitude = location.Latitude, Longitude = location.Longitude }, Geolocation.DistanceType.Miles) < 50).ToList();

        //    if (viewModel.Dancer.YouTubeUsername != null)
        //    {
        //        viewModel.YouTubeVideos = GetVideos(viewModel.Dancer.YouTubeUsername);
        //    }
        //    else
        //    {
        //        viewModel.YouTubeVideos = new List<YouTubeVideo>();
        //    }

        //    var teachers = viewModel.Dancer.Students;

        //    if (viewModel.Dancer.FacebookToken != null)
        //    {
        //        viewModel.FriendList = FacebookHelper.GetFriends(viewModel.Dancer.FacebookToken);

        //        foreach (FacebookFriend f in viewModel.FriendList)
        //        {
        //            var user = DataContext.Users.Where(x => x.FacebookUsername == f.Id).FirstOrDefault();
        //            if (user != null)
        //            {
        //                user.UserPictures = DataContext.Pictures.OfType<UserPicture>().Where(x => x.User.Id == user.Id).ToList();
        //                user.UserPictures.Add(ApplicationUtility.GetNoProfilePicture());
        //                f.User = user;
        //            }
        //        }
        //    }

        //    return View(viewModel);
        //}

        //  [Route("{username:regex(^(?!Owner))}", Order=1000)]
        [Route("Dancer/{username}", Order = 1000)]
        [Authorize]
        public ActionResult Home(string username)
        {
            var viewModel = LoadDancerModel(username);

            if (viewModel.Dancer == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var today = DateTime.Today;
            var id = User.Identity.GetUserId();

            ////  Media Updates
            //var lstMedia = new List<EventMedia>();
            //var events = DataContext.Events.Where(e => e.EventMembers.Any(m => m.Member.Id == id));
            //var newPictures = DataContext.Pictures.OfType<EventPicture>()
            //                    .Include("Event")
            //                    .Include("PostedBy")
            //                    .Where(p => events.Any(e => e.Id == p.Event.Id))
            //                    .OrderByDescending(p => p.PhotoDate)
            //                    .Take(20);
            //foreach(var p in newPictures)
            //{
            //    lstMedia.Add(new EventMedia() { Event = p.Event, SourceName = p.Title, SourceLink = p.SourceLink, Id = p.Id, Author = p.PostedBy, MediaDate = p.PhotoDate, MediaType = Enums.MediaType.Picture, PhotoUrl = p.Filename, Title = p.Title, MediaSource = p.MediaSource });
            //}
            //var newVideos = DataContext.Videos.OfType<EventVideo>()
            //                    .Include("Event")
            //                    .Include("Author")
            //                    .Where(v => events.Any(e => e.Id == v.Event.Id))
            //                    .OrderByDescending(v => v.PublishDate)
            //                    .Take(20);
            //foreach(var v in newVideos)
            //{
            //    lstMedia.Add(new EventMedia() { Event = v.Event, SourceName = v.Title, SourceLink = v.VideoUrl, Id = v.Id, Author = v.Author, MediaDate = v.PublishDate, MediaType = Enums.MediaType.Video, PhotoUrl = v.PhotoUrl, MediaUrl = v.VideoUrl, Title = v.Title, MediaSource = v.MediaSource });
            //}
            //var playlists = DataContext.Playlists.OfType<EventPlaylist>()
            //                    .Include("Event")
            //                    .Include("Author")
            //                    .Where(v => events.Any(e => e.Id == v.Event.Id));
            //foreach (var lst in playlists)
            //{
            //    var videos = YouTubeHelper.GetPlaylistVideos(lst.YouTubeId);

            //    foreach(var movie in videos)
            //    {
            //        lstMedia.Add(new EventMedia() { Event = lst.Event, SourceName = movie.Title, SourceLink = movie.VideoLink.ToString(), Author = lst.Author, MediaDate = movie.PubDate, MediaType = Enums.MediaType.Video, PhotoUrl = movie.Thumbnail.ToString(), MediaUrl = movie.VideoLink.ToString(), Title = movie.Title, MediaSource = lst.MediaSource });
            //    }
            //}
            //viewModel.MediaUpdates = lstMedia;
            ////  Media Updates

            //viewModel.SuggestedClasses = new List<Class>();
            //var myLocation = DbGeography.FromText("POINT(" + viewModel.Dancer.Longitude.ToString() + " " + viewModel.Dancer.Latitude.ToString() + ")");
            //var arrStyles = viewModel.Dancer.DanceStyles.Select(s => s.Id).ToArray();
            //var suggestedClasses = DataContext.Events.OfType<Class>()
            //                            .Where(y => y.DanceStyles.Any(d => arrStyles.Contains(d.Id))
            //                                    && DbGeography.FromText("POINT(" + y.Place.Longitude.ToString() + " " + y.Place.Latitude.ToString() + ")").Distance(myLocation) * .00062 < 50
            //                                    && !y.EventMembers.Any(u => u.Member.Id == id))
            //                            .Include("DanceStyles")
            //                            .Include("Teachers")
            //                            .Include("Teachers.ApplicationUser")
            //                            .Include("Place")
            //                            .ToList();
            //viewModel.SuggestedClasses = suggestedClasses.Where(e => e.NextDate >= today).Take(5);
            //viewModel.SuggestedSocials = new List<Social>();
            //var suggestedSocials = DataContext.Events.OfType<Social>()
            //                            .Where(y => y.DanceStyles.Any(d => arrStyles.Contains(d.Id)) 
            //                                && DbGeography.FromText("POINT(" + y.Place.Longitude.ToString() + " " + y.Place.Latitude.ToString() + ")").Distance(myLocation) * .00062 < 50 
            //                                && !y.EventMembers.Any(u => u.Member.Id == id))
            //                            .Include("DanceStyles")
            //                            .Include("Place")
            //                            .ToList();
            //viewModel.SuggestedSocials = suggestedSocials.Where(e => e.NextDate >= today).Take(5);

            //  var groups = FacebookHelper.GetGroups(viewModel.Dancer.FacebookToken);
            return View(viewModel);
        }

        [Route("Dancer/GetUpdates")]
        public ActionResult GetUpdates(string username)
        {
            var evt = DataContext.Events.Where(e => e.EventMembers.Any(m => m.Member.UserName == username))
                    .Include("Creator")
                    .Include("Pictures")
                    .Include("Pictures.PostedBy")
                    .Include("Videos")
                    .Include("Videos.Author")
                    .Include("Playlists")
                    .Include("Playlists.Author")
                    .Include("LinkedFacebookObjects");

            var lstMedia = EventHelper.BuildAllUpdates(evt, MediaTarget.User);

            return PartialView("~/Views/Shared/_MediaUpdatesPartial.cshtml", lstMedia);
            //  Media Updates

        }

        [Route("Dancer/FacebookEventsPartial")]
        public ActionResult FacebookEventsPartial(string username, EventType? eventType, int? schoolId, int? promotergroupId, RoleName role)
        {
            var model = new FacebookEventsViewModel();
            model.Type = eventType;
            model.SchoolId = schoolId;
            model.PromoterGroupId = promotergroupId;
            model.Role = role;

            //  Load Facebook Events
            if (User.Identity.IsAuthenticated)
            {
                var userid = User.Identity.GetUserId();
                var user = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();

                if (user.FacebookToken != null)
                {
                    model.FacebookEvents = FacebookHelper.GetEvents(user.FacebookToken, DateTime.Now).Where(fe => !DataContext.Events.Select(e => e.FacebookId).Contains(fe.Id)).ToList();
                }
                //  Load Facebook Events
            }

            return PartialView("~/Views/Shared/_ImportFacebookEventsPartial.cshtml", model);
            //  Media Updates

        }

        [Route("Dancer/GetFacebookEvents")]
        public JsonResult GetFacebookEvents()
        {
            if (User.Identity.IsAuthenticated)
            {
                var userid = User.Identity.GetUserId();
                var token = DataContext.Users.Single(u => u.Id == userid).FacebookToken;

                if (token != null)
                {
                    var evnts = FacebookHelper.GetEvents(token, DateTime.Now).Where(fe => !DataContext.Events.Select(e => e.FacebookId).Contains(fe.Id)).OrderBy(f => f.StartTime).ToList();
                    return Json(evnts, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        //[Route("Dancer/MyLearn")]
        //[Authorize]
        //public ActionResult MyLearn(string username)
        //{
        //    var viewModel = LoadDancerModel(username);

        //    if (viewModel.Dancer == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }

        //    var today = DateTime.Today;
        //    var id = User.Identity.GetUserId();

        //    viewModel.Teachers = new List<Teacher>();
        //    viewModel.Teachers = DataContext.Teachers.Where(x => x.Students.Any(s => s.DancerId == id))
        //                            .Include("ApplicationUser")
        //                            .Include("ApplicationUser.UserPictures")
        //                            .ToList();
        //    viewModel.Classes = new EventListViewModel();
        //    viewModel.Classes.EventType = EventType.Class;
        //    var classes = DataContext.Events.OfType<Class>()
        //                            .Where(x => x.EventMembers.Any(u => u.Member.UserName == username))
        //                            .Include("EventMembers.Member")
        //                            .Include("Teachers")
        //                            .Include("DanceStyles")
        //                            .Include("Pictures")
        //                            .Include("Videos")
        //                            .Include("Reviews")
        //                            .Include("Place")
        //                            .ToList();
        //    viewModel.Classes.Events = classes.Where(e => e.NextDate >= today);
        //    viewModel.Classes.Location = new Address() { City = viewModel.Dancer.City, State = viewModel.Dancer.State, ZipCode = viewModel.Dancer.ZipCode, Longitude = viewModel.Dancer.Longitude, Latitude = viewModel.Dancer.Latitude };

        //    //var scheduler = new DHXScheduler(this) { Skin = DHXScheduler.Skins.Terrace };
        //    //scheduler.Templates.map_time = "{start_date.toLocaleString()}"; //   "{start_date.toLocaleTimeString()}";    // "{start_date:date(%d.%m.%Y)}";
        //    //scheduler.Views.Clear();
        //    //scheduler.Views.Add(new MonthView());
        //    //scheduler.Views.Add(new MapView());
        //    //scheduler.InitialView = (new MonthView()).Name;
        //    //scheduler.LoadData = true;
        //    //scheduler.DataAction = "MapEvents";

        //    //viewModel.Scheduler = scheduler;

        //    return View(viewModel);
        //}

        [Route("Dancer/GetTeamsPartial")]
        [HttpGet]
        public virtual ActionResult GetTeamsPartial()
        {
            var userid = User.Identity.GetUserId();
            var teams = DataContext.Teams.Where(t => t.Members.Any(m => m.UserId == userid));
            return PartialView("~/Views/Shared/DisplayTemplates/Teams.cshtml", teams);
        }

        //[Authorize]
        //public ActionResult MyFriends(string username)
        //{
        //    var viewModel = LoadDancerModel(username);

        //    if (viewModel.Dancer == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }

        //    var today = DateTime.Today;
        //    var id = User.Identity.GetUserId();

        //    if (viewModel.Dancer.FacebookToken != null)
        //    {
        //        viewModel.FriendList = FacebookHelper.GetFriends(viewModel.Dancer.FacebookToken);

        //        foreach (FacebookFriend f in viewModel.FriendList)
        //        {
        //            var user = DataContext.Users.Where(x => x.FacebookUsername == f.Id).FirstOrDefault();
        //            if (user != null)
        //            {
        //                user.UserPictures = DataContext.Pictures.OfType<UserPicture>().Where(x => x.User.Id == user.Id).ToList();
        //                user.UserPictures.Add(ApplicationUtility.GetNoProfilePicture());
        //                f.User = user;
        //            }
        //        }
        //    }

        //    return View(viewModel);
        //}

        //[Authorize]
        //public ActionResult MyDance(string username)
        //{
        //    var viewModel = LoadDancerModel(username);

        //    if (viewModel.Dancer == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }

        //    var today = DateTime.Today;
        //    var id = User.Identity.GetUserId();

        //    if (viewModel.Dancer.FacebookToken != null)
        //    {
        //        viewModel.FriendList = FacebookHelper.GetFriends(viewModel.Dancer.FacebookToken);

        //        foreach (FacebookFriend f in viewModel.FriendList)
        //        {
        //            var user = DataContext.Users.Where(x => x.FacebookUsername == f.Id).FirstOrDefault();
        //            if (user != null)
        //            {
        //                user.UserPictures = DataContext.Pictures.OfType<UserPicture>().Where(x => x.User.Id == user.Id).ToList();
        //                user.UserPictures.Add(ApplicationUtility.GetNoProfilePicture());
        //                f.User = user;
        //            }
        //        }
        //    }

        //    viewModel.Socials = new EventListViewModel();
        //    viewModel.Socials.EventType = EventType.Social;
        //    viewModel.Socials.Events = DataContext.Events
        //                                    .OfType<Social>()
        //                                    .Include("Pictures")
        //                                    .Include("EventMembers.Member")
        //                                    .Include("Videos")
        //                                    .Include("Reviews")
        //                                    .Include("Place")
        //                                    .Where(x => x.EventMembers.Any(m => m.Member.UserName == username)).ToList();
        //    viewModel.Socials.Location = new Address() { City = viewModel.Dancer.City, State = viewModel.Dancer.State, ZipCode = viewModel.Dancer.ZipCode, Longitude = viewModel.Dancer.Longitude, Latitude = viewModel.Dancer.Latitude };

        //    return View(viewModel);
        //}

        [Route("Dancer/MapEvents")]
        public ContentResult MapEvents()
        {
            var today = DateTime.Today;

            var events = DataContext.Events.ToList();
            var lstEvents = new List<object>();

            foreach (Event e in events)
            {
                lstEvents.Add(new { id = e.Id, text = e.Name + " @" + e.Place.Name + "<br/>" + e.Description, start_date = e.NextDate, end_date = e.EndDateTime, lat = e.Place.Latitude, lng = e.Place.Longitude, event_location = e.Place.Address + ", " + e.Place.City + ", " + e.Place.State + " " + e.Place.Zip });
            }

            var data = new SchedulerAjaxData(lstEvents);

            return data;
        }

        //[Authorize]
        //public ActionResult Dance(string username)
        //{
        //    if (User.Identity.IsAuthenticated && username == "View")
        //    {
        //        username = User.Identity.Name;
        //    }
        //    if (String.IsNullOrWhiteSpace(username))
        //    {
        //        if (User != null)
        //        {
        //            username = User.Identity.GetUserName();
        //        }
        //        else
        //        {
        //            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //        }
        //    }

        //    var dancer = DataContext.Users.Where(x => x.UserName == username).Include("DanceStyles").Include("UserPictures").FirstOrDefault();
        //    if (dancer == null)
        //    {
        //        return HttpNotFound();
        //    }

        //    var today = DateTime.Today;

        //    var viewModel = new DancerViewViewModel();
        //    viewModel.Dancer = dancer;
        //    var location = Geolocation.ParseAddress(dancer.ZipCode);

        //    if (dancer.ZipCode != null)
        //    {
        //        viewModel.Address = Geolocation.ParseAddress(dancer.ZipCode);
        //    }
        //    else
        //    {
        //        viewModel.Address = Geolocation.ParseAddress("90065");
        //    }

        //    return View(viewModel);
        //}

        //[Authorize]
        //public ActionResult SocialMedia(string username)
        //{
        //    var viewModel = LoadDancerModel(username);

        //    if (viewModel.Dancer == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }

        //    var today = DateTime.Today;
        //    var id = User.Identity.GetUserId();

        //    if (viewModel.Dancer.FacebookToken != null)
        //    {
        //        viewModel.FriendList = FacebookHelper.GetFriends(viewModel.Dancer.FacebookToken);

        //        foreach (FacebookFriend f in viewModel.FriendList)
        //        {
        //            var user = DataContext.Users.Where(x => x.FacebookUsername == f.Id).FirstOrDefault();
        //            if (user != null)
        //            {
        //                user.UserPictures = DataContext.Pictures.OfType<UserPicture>().Where(x => x.User.Id == user.Id).ToList();
        //                user.UserPictures.Add(ApplicationUtility.GetNoProfilePicture());
        //                f.User = user;
        //            }
        //        }
        //    }

        //    return View(viewModel);
        //}

        [Route("Dancer/ChangeSpotifyPlaylist")]
        [Authorize]
        public ActionResult ChangeSpotifyPlaylist(string username)
        {
            if (User.Identity.IsAuthenticated && username == "View")
            {
                username = User.Identity.Name;
            }
            if (String.IsNullOrWhiteSpace(username))
            {
                if (User != null)
                {
                    username = User.Identity.GetUserName();
                }
                else
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
            }

            var dancer = DataContext.Users.Where(x => x.UserName == username).Include("DanceStyles").Include("UserPictures").FirstOrDefault();
            if (dancer == null)
            {
                return HttpNotFound();
            }

            var today = DateTime.Today;

            var viewModel = LoadDancerModel(username);

            if (viewModel.Dancer == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            //var location = Geolocation.ParseAddress(dancer.ZipCode);

            //if (dancer.ZipCode != null)
            //{
            //    viewModel.Address = Geolocation.ParseAddress(dancer.ZipCode);
            //}
            //else
            //{
            //    viewModel.Address = Geolocation.ParseAddress("90065");
            //}

            //  Return Spotify Playlists
            if (viewModel.Dancer.SpotifyId != null)
            {
                //  Get token
                var accesstoken = SpotifyHelper.GetAccessToken(viewModel.Dancer.SpotifyRefreshToken, SpotifyGrantType.refresh_token);

                if (accesstoken.Access_Token == null)
                {
                    var client_id = ConfigurationManager.AppSettings["SpotifyClientId"];
                    var redirect_uri = ConfigurationManager.AppSettings["SpotifyRedirectUri"];
                    return Redirect("https://accounts.spotify.com/en/authorize/?client_id=" + client_id + "&redirect_uri=" + redirect_uri + "&response_type=code&scope=playlist-read-private%20playlist-modify-public");
                }
                else
                {
                    var token = new SpotifyAccessToken() { Access_Token = accesstoken.Access_Token, Refresh_Token = viewModel.Dancer.SpotifyRefreshToken };
                    Session["SpotifyPlaylists"] = SpotifyHelper.GetPlaylists(ref token, viewModel.Dancer.SpotifyId);
                    viewModel.SpotifyPlaylists = new List<SpotifyPlaylist>();
                    viewModel.SpotifyPlaylists = (List<SpotifyPlaylist>)Session["SpotifyPlaylists"];

                    if (token.Refresh_Token == null)
                    {
                        dancer.SpotifyToken = token.Access_Token;
                        DataContext.SaveChanges();
                    }
                }
            }
            //  Return Spotify Playlists

            return View(viewModel);
        }

        [Route("Dancer/UpdateSpotifyPlaylist")]
        [Authorize]
        public ActionResult UpdateSpotifyPlaylist(string playlistid)
        {
            var userid = User.Identity.GetUserId();
            var user = DataContext.Users.Where(x => x.Id == userid).FirstOrDefault();
            user.SpotifyUri = ((List<SpotifyPlaylist>)Session["SpotifyPlaylists"]).Where(l => l.id == playlistid).FirstOrDefault().uri;
            DataContext.Entry(user).State = EntityState.Modified;
            DataContext.SaveChanges();
            return RedirectToAction("Home", "Dancer", new { username = user.UserName });
        }

        //[Authorize]
        //public ActionResult Edit()
        //{
        //    // TODO: FILL MORE VIEWMODEL PROPERTIES (SEE DancerViewModel)
        //    var viewModel = GetInitialDancerEditViewModel();

        //    if (viewModel.Dancer == null)
        //    {
        //        return HttpNotFound();
        //    }

        //    return View(viewModel);
        //}

        private DancerEditViewModel GetInitialDancerEditViewModel()
        {
            var model = new DancerEditViewModel();
            var id = User.Identity.GetUserId();
            model.Dancer = DataContext.Users.Where(x => x.Id == id).Include("DanceStyles").FirstOrDefault();

            var selectedStyles = new List<DanceStyleListItem>();
            if (model.Dancer.DanceStyles != null)
            {
                foreach (DanceStyle ss in model.Dancer.DanceStyles)
                {
                    selectedStyles.Add(new DanceStyleListItem { Id = ss.Id, Name = ss.Name });
                }
            }
            model.SelectedStyles = selectedStyles;

            var styles = new List<DanceStyleListItem>();
            foreach (DanceStyle s in DataContext.DanceStyles)
            {
                styles.Add(new DanceStyleListItem { Id = s.Id, Name = s.Name });
            }
            model.AvailableStyles = styles.OrderBy(x => x.Name);

            return model;
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //[Authorize]
        //public ActionResult Edit(DancerEditViewModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var dancer = DataContext.Users.Where(x => x.Id == model.Dancer.Id).Include("DanceStyles").Include("Parties").FirstOrDefault();
        //        dancer.StartDate = model.Dancer.StartDate;
        //        dancer.YouTubeUsername = model.Dancer.YouTubeUsername;
        //        dancer.SpotifyUri = model.Dancer.SpotifyUri;
        //        dancer.ZipCode = model.Dancer.ZipCode;

        //        if (model.Dancer.ZipCode != null)
        //        {
        //            var address = Geolocation.ParseAddress(model.Dancer.ZipCode);
        //            dancer.City = address.City;
        //            dancer.State = address.State;
        //            dancer.Longitude = address.Longitude;
        //            dancer.Latitude = address.Latitude;
        //        }

        //        if (model.PostedStyles != null)
        //        {
        //            var styles = DataContext.DanceStyles.Where(x => model.PostedStyles.DanceStyleIds.Contains(x.Id.ToString())).ToList();

        //            dancer.DanceStyles.Clear();

        //            foreach (DanceStyle s in styles)
        //            {
        //                dancer.DanceStyles.Add(s);
        //            }

        //        }

        //        DataContext.Entry(dancer).State = EntityState.Modified;
        //        DataContext.SaveChanges();
        //        return RedirectToAction("Home", "Dancer", new { username = dancer.UserName });
        //    }
        //    return View(model);
        //}

        //[Authorize]
        //public JsonResult AddStyle(DancerManageViewModel model)
        //{
        //    DataContext.Users.Single(u => u.Id == model.Dancer.Id).DanceStyles.Add(DataContext.DanceStyles.Single(s => s.Id == model.NewStyleId));
        //    DataContext.SaveChanges();
        //    var styles = DataContext.Users.Single(u => u.Id == model.Dancer.Id).DanceStyles.Select(s => new { Id = s.Id, Name = s.Name });
        //    return Json(styles.ToList(), JsonRequestBehavior.AllowGet); 
        //}

        [Route("Dancer/AddStyle")]
        [Authorize]
        public PartialViewResult AddStyle(DancerManageViewModel model)
        {
            var user = DataContext.Users.Include("DanceStyles").Single(u => u.Id == model.Dancer.Id);
            if (!user.DanceStyles.Select(s => s.Id).Contains((int)model.NewStyleId))
            {
                user.DanceStyles.Add(DataContext.DanceStyles.Single(s => s.Id == model.NewStyleId));
                DataContext.SaveChanges();
            }
            var styles = DataContext.Users.Single(u => u.Id == model.Dancer.Id).DanceStyles.ToList();
            return PartialView("~/Views/Shared/_DancerStylesPartial.cshtml", new DancerStylesViewModel() { Id = model.Dancer.Id, Styles = styles, Controller = "Dancer" });
        }

        [Route("Dancer/DeleteStyle")]
        [Authorize]
        public PartialViewResult DeleteStyle(string id, int styleId)
        {
            var user = DataContext.Users.Single(u => u.Id == id);
            user.DanceStyles.Remove(DataContext.DanceStyles.Single(s => s.Id == styleId));
            DataContext.SaveChanges();
            return PartialView("~/Views/Shared/_DancerStylesPartial.cshtml", new DancerStylesViewModel() { Id = id, Styles = user.DanceStyles.ToList(), Controller = "Dancer" });
        }

        [Authorize]
        private List<YouTubeVideo> GetVideos(string youTubeUsername)
        {
            try
            {
                List<YouTubeVideo> vidList = new List<YouTubeVideo>();
                string url = "http://gdata.youtube.com/feeds/api/users/" + youTubeUsername + "/uploads?orderby=published";

                XDocument ytDoc = XDocument.Load(url);

                var movies = ytDoc.Descendants().Where(p => p.Name.LocalName == "entry").ToList();

                foreach (var movie in movies)
                {
                    vidList.Add(new YouTubeVideo() { Id = movie.Descendants().Where(p => p.Name.LocalName == "id").FirstOrDefault().Value.Replace("http://gdata.youtube.com/feeds/api/videos/", ""), Title = movie.Descendants().Where(p => p.Name.LocalName == "title").FirstOrDefault().Value });
                }

                return vidList;
            }
            catch
            {
                return new List<YouTubeVideo>();
            }
        }

        //[Authorize]
        //public ActionResult UploadPicture()
        //{
        //    return View();
        //}

        //[HttpPost]
        //[Authorize]
        //public ActionResult UploadPicture(HttpPostedFileBase file)
        //{
        //    UploadFile newFile = ApplicationUtility.LoadPicture(file);
        //    string message;

        //    if (newFile.UploadStatus == "Success")
        //    {
        //        var dancer = DataContext.Users.Where(x => x.UserName == User.Identity.Name).Include("UserPictures").FirstOrDefault();
        //        var today = DateTime.Now;
        //        dancer.UserPictures.Add(new UserPicture() { Title = newFile.FileName, Filename = newFile.FilePath, ThumbnailFilename = newFile.ThumbnailFilePath, PhotoDate = today });
        //        DataContext.Entry(dancer).State = EntityState.Modified;
        //        DataContext.SaveChanges();
        //        message = "File was uploaded";
        //    }
        //    else
        //    {
        //        message = newFile.UploadStatus;
        //    }
        //    return RedirectToAction("ChangePicture", "Dancer", new { message = message });
        //}

        [Route("Dancer/UploadImageAsync")]
        [Authorize]
        [HttpPost]
        public JsonResult UploadImageAsync(string imageData)
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
                newFile = ApplicationUtility.UploadFromPath(imageData);
                if (newFile.UploadStatus == "Success")
                {
                    var userid = User.Identity.GetUserId();
                    var user = DataContext.Users.Single(s => s.Id == userid);
                    ApplicationUtility.DeletePicture(new Picture() { Filename = user.PhotoUrl });
                    user.PhotoUrl = newFile.FilePath;
                    DataContext.SaveChanges();
                }
            }
            var objUpload = new { FilePath = Url.Content(newFile.FilePath), UploadStatus = newFile.UploadStatus };
            return Json(objUpload, JsonRequestBehavior.AllowGet);
        }

        //[HttpPost]
        //public async Task<ActionResult> UploadImageAsync(string imageData)
        //{
        //    if (string.IsNullOrEmpty(imageData))
        //        return Redirect(Request.UrlReferrer.AbsolutePath);

        //    Match imageMatch = Regex.Match(imageData, @"^data:(?<mimetype>[^;]+);base64,(?<data>.+)$");
        //    if (!imageMatch.Success)
        //        return Redirect(Request.UrlReferrer.AbsolutePath);

        //    string mimeType = imageMatch.Groups["mimetype"].Value;
        //    Match imageType = Regex.Match(mimeType, @"^[^/]+/(?<type>.+?)$");
        //    if (!imageType.Success)
        //        return Redirect(Request.UrlReferrer.AbsolutePath);

        //    string fileExtension = imageType.Groups["type"].Value;
        //    byte[] data2 = Convert.FromBase64String(imageMatch.Groups["data"].Value);

        //    UploadFile newFile = ApplicationUtility.UploadFromPath(imageData);
        //    if (newFile.UploadStatus == "Success")
        //    {
        //        var userid = User.Identity.GetUserId();
        //        DataContext.Users.Single(s => s.Id == userid).PhotoUrl = newFile.FilePath;
        //        DataContext.SaveChanges();
        //    }
        //    return Redirect(Request.UrlReferrer.AbsolutePath);
        //}

        [Route("Dancer/DeletePicture")]
        public ActionResult DeletePicture(int pictureId)
        {
            var picture = DataContext.Pictures.Find(pictureId);
            var userpic = DataContext.Users.Where(x => x.UserName == User.Identity.Name).Include("UserPictures").FirstOrDefault().UserPictures.Where(x => x.Id == pictureId).FirstOrDefault();
            DataContext.Pictures.Remove(userpic);
            DataContext.Entry(userpic).State = EntityState.Deleted;
            DataContext.SaveChanges();
            ViewBag.Message = ApplicationUtility.DeletePicture(picture);
            return RedirectToAction("ChangePicture", "Dancer", new { username = User.Identity.Name });
        }

        public FileResult OpenPicture(string fileName)
        {
            try
            {
                return File(new FileStream(Server.MapPath("~/MyFiles/" + fileName), FileMode.Open), "application/octetstream", fileName);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Route("Dancer/GetEvents")]
        public JsonResult GetEvents(string id, DateTime start, DateTime end)
        {
            //  var instances = DataContext.Events.Where(e => e.EventInstances.Any(i => i.EventRegistrations.Any(r => r.UserId == id))).SelectMany(c => c.EventInstances).Where(i => i.DateTime >= start && i.DateTime <= end).ToList();
            var instances = DataContext.Events.Where(e => (e is Class || e is Social) && e.EventInstances.Any(i => i.EventRegistrations.Any(r => r.UserId == id))).SelectMany(c => c.EventInstances).Where(i => i.DateTime >= start && i.DateTime <= end).ToList();

            return Json(instances.AsEnumerable().Select(s =>
                        new {
                            id = s.EventId,
                            title = s.Event.Name,
                            start = s.StartTime.Value.ToString("o"),
                            end = s.EndTime.Value.ToString("o"),
                            lat = s.Event.Place.Latitude,
                            lng = s.Event.Place.Longitude,
                            color = s.Event is Class ? "#65AE25" : s.Event is Social ? "#006A90" : s.Event is Performance ? "#f0ad4e" : s.Event is Rehearsal ? "#d9534f" : "#428bca",
                            url = Url.Action(s.Event is Class ? EventType.Class.ToString() : EventType.Social.ToString(), "Event", new { id = s.EventId })
                        }), JsonRequestBehavior.AllowGet);
        }

        [Route("Dancer/GetEventRegitrations")]
        [Authorize]
        public JsonResult GetEventRegitrations(string id, DateTime start, DateTime end)
        {
            var instances = DataContext.EventInstances.Where(i => i.EventRegistrations.Any(r => r.UserId == id)).Where(i => i.DateTime >= start && i.DateTime <= end).ToList();

            return Json(instances.AsEnumerable().Select(s =>
                        new {
                            id = s.EventId,
                            title = s.Event.Name,
                            start = s.StartTime.Value.ToString("o"),
                            end = s.EndTime.Value.ToString("o"),
                            lat = s.Event.Place.Latitude,
                            lng = s.Event.Place.Longitude,
                            color = s.Event is Class ? "#65AE25" : s.Event is Social ? "#006A90" : s.Event is Performance ? "#f0ad4e" : s.Event is Rehearsal ? "#d9534f" : "#428bca",
                            url = Url.Action(s.Event is Class ? EventType.Class.ToString() : EventType.Social.ToString(), "Event", new { id = s.EventId })
                        }), JsonRequestBehavior.AllowGet);
        }

        [Route("Dancer/ChangePicture")]
        [Authorize]
        public ActionResult ChangePicture(string message)
        {
            var model = new ChangePictureViewModel();
            ViewBag.Message = message;

            var id = User.Identity.GetUserId();
            var dancer = DataContext.Users.Where(x => x.Id == id).FirstOrDefault();
            model.PhotoUrl = dancer.PhotoUrl;

            if (dancer.FacebookToken != null)
            {
                model.FacebookPictures = FacebookHelper.GetPhotos(dancer.FacebookToken);
            }

            return View(model);
        }

        [Route("Dancer/ChangePicture")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult ChangePicture(ChangePictureViewModel model)
        {
            return View(model);
        }

        [Route("Dancer/ProfilePicture")]
        [Authorize]
        public ActionResult ProfilePicture(int id)
        {
            try
            {
                string userId = User.Identity.GetUserId();
                var user = UserManager.FindById(userId);

                var dancer = DataContext.Users.Where(x => x.Id == userId).Include("UserPictures").FirstOrDefault();

                foreach (UserPicture p in dancer.UserPictures)
                {
                    if (p.Id == id)
                    {
                        p.ProfilePicture = true;
                        dancer.PhotoUrl = p.Filename;
                    }
                    else
                    {
                        p.ProfilePicture = false;
                    }
                }

                DataContext.Entry(dancer).State = EntityState.Modified;
                DataContext.SaveChanges();

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
            catch (Exception ex)
            {
                return RedirectToAction("ChangePicture", "Dancer", new { username = User.Identity.Name });
            }
        }

        [Route("Dancer/AddFacebookPicture")]
        [Authorize]
        public ActionResult AddFacebookPicture(string id, string album, string name, string largeSource, string link, DateTime photodate, string source)
        {
            try
            {
                string userId = User.Identity.GetUserId();
                var dancer = DataContext.Users.Where(x => x.Id == userId).Include("UserPictures").FirstOrDefault();
                dancer.UserPictures.Add(new UserPicture() { Title = name, ThumbnailFilename = source, Filename = largeSource, PhotoDate = photodate });
                DataContext.Entry(dancer).State = EntityState.Modified;
                DataContext.SaveChanges();
                return RedirectToAction("ChangePicture", "Dancer", new { username = User.Identity.Name });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ChangePicture", "Dancer", new { username = User.Identity.Name });
            }
        }

        [Route("Dancer/Backend")]
        public ActionResult Backend()
        {
            return new Dpm().CallBack(this);
        }

        public class Dpm : DayPilotMonth
        {
            protected override void OnTimeRangeSelected(TimeRangeSelectedArgs e)
            {   
                string name = (string)e.Data["name"];
                if (String.IsNullOrEmpty(name))
                {
                    name = "(default)";
                }
                //  new EventManager(Controller).EventCreate(e.Start, e.End, name);
                Update();
            }

            protected override void OnEventMove(EventMoveArgs e)
            {
                //if (new EventManager(Controller).Get(e.Id) != null)
                //{
                //    new EventManager(Controller).EventMove(e.Id, e.NewStart, e.NewEnd);
                //}

                Update();
            }

            protected override void OnEventResize(EventResizeArgs e)
            {
                //new EventManager(Controller).EventMove(e.Id, e.NewStart, e.NewEnd);
                Update();
            }

            protected override void OnCommand(CommandArgs e)
            {
                switch (e.Command)
                {
                    case "navigate":
                        StartDate = (DateTime)e.Data["start"];
                        Update(CallBackUpdateType.Full);
                        break;

                    case "previous":
                        StartDate = StartDate.AddMonths(-1);
                        Update(CallBackUpdateType.Full);
                        break;

                    case "next":
                        StartDate = StartDate.AddMonths(1);
                        Update(CallBackUpdateType.Full);
                        break;

                    case "today":
                        StartDate = DateTime.Today;
                        Update(CallBackUpdateType.Full);
                        break;

                    case "refresh":
                        Update();
                        break;
                }
            }

            protected override void OnInit(InitArgs initArgs)
            {
                Update(CallBackUpdateType.Full);
            }

            protected override void OnFinish()
            {
                // only load the data if an update was requested by an Update() call
                if (UpdateType == CallBackUpdateType.None)
                {
                    return;
                }

                // this select is a really bad example, no where clause
                var DataContext = new ApplicationDbContext();
                var user = System.Web.HttpContext.Current.User.Identity;
                var today = DateTime.Today;
                var classes = DataContext.Events.OfType<Class>().Where(x => x.Users.Any(u => u.UserName == user.Name)).ToList();
                var events = new List<Event>();

                foreach (var c in classes)
                {
                    today = DateTime.Today;

                    if (c.Recurring)
                    {
                        var nextDate = ApplicationUtility.GetNextDate(c.StartDate, c.Frequency, (int)c.Interval, c.Day, today);

                        while (nextDate <= DateTime.Today.AddYears(2) && nextDate <= (c.EndDate != null ? c.EndDate : DateTime.Today.AddYears(2)))
                        {
                            events.Add(new Class() { Name = c.Name, StartDate = nextDate, EndDate = nextDate.AddMinutes(c.Duration), Id = c.Id });
                            today = nextDate.AddDays(1);
                            nextDate = ApplicationUtility.GetNextDate(nextDate, c.Frequency, (int)c.Interval, c.Day, today);
                        }
                    }
                    else
                    {
                        events.Add(new Event() { Id = c.Id, StartDate = c.NextDate, EndDate = c.EndDateTime, Name = c.Name });
                    }
                }

                today = DateTime.Today;
                Events = events.Where(e => e.NextDate >= today).ToList();

                DataIdField = "Id";
                DataTextField = "Name";
                DataStartField = "StartDate";
                DataEndField = "EndDate";
            }
        }
    }
}
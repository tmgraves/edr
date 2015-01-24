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

namespace EDR.Controllers
{
    public class DancerController : BaseController
    {
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

            var dancer = DataContext.Users.Where(x => x.UserName == username).Include("DanceStyles").Include("UserPictures").FirstOrDefault();

            if (dancer != null)
            {
                viewModel.Dancer = dancer;

                if (viewModel.Dancer.ZipCode != null)
                {
                    viewModel.Address = Geolocation.ParseAddress(viewModel.Dancer.ZipCode);
                }
                else
                {
                    viewModel.Address = Geolocation.ParseAddress("90065");
                }
            }

            return viewModel;
        }

        [Authorize]
        public ActionResult View(string username)
        {
            if (username == "View")
            {
                RedirectToAction("View", "Dancer", User.Identity.Name);
            }

            var viewModel = LoadDancerModel(username);

            if (viewModel.Dancer == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            //if (User.Identity.IsAuthenticated && username == "View")
            //{
            //    username = User.Identity.Name;
            //}
            //if (String.IsNullOrWhiteSpace(username))
            //{
            //    if (User != null)
            //    {
            //        username = User.Identity.GetUserName();
            //    }
            //    else
            //    {
            //    }
            //}

            //var dancer = DataContext.Users.Where(x => x.UserName == username).Include("DanceStyles").Include("UserPictures").FirstOrDefault();
            //if(dancer == null)
            //{
            //    return HttpNotFound();
            //}

            var today = DateTime.Today;
            var id = User.Identity.GetUserId();

            //var viewModel = new DancerViewViewModel();
            //viewModel.Dancer = dancer;
            viewModel.Teachers = new List<Teacher>();
            viewModel.Teachers = DataContext.Teachers.Where(x => x.Students.Any(s => s.DancerId == id)).Include("ApplicationUser").Include("ApplicationUser.UserPictures").ToList();
            viewModel.Classes = new EventListViewModel();
            var classes = DataContext.Events.OfType<Class>().Where(x => x.Users.Any(u => u.UserName == username)).Include("Users").Include("Teachers").ToList();
            viewModel.Classes.Events = classes.Where(e => e.NextDate >= today);
            viewModel.Socials = new EventListViewModel();
            viewModel.Socials.Events = DataContext.Events.OfType<Social>().Where(x => x.Users.Any(u => u.UserName == username)).ToList();
            viewModel.SuggestedClasses = new List<Class>();
            //  y.DanceStyles.Any(x => dancer.DanceStyles.Contains(x)) && y.NextDate >= today && 
            var myLocation = DbGeography.FromText("POINT(" + viewModel.Address.Longitude.ToString() + " " + viewModel.Address.Latitude.ToString() + ")");
            //  viewModel.SuggestedEvents = DataContext.Events.Where(y => y.DanceStyles.Any(x => dancer.DanceStyles.Contains(x)) && DbGeography.FromText("POINT(" + y.Place.Longitude.ToString() + " " + y.Place.Latitude.ToString() + ")").Distance(myLocation) * .00062 < 50).ToList(); // In Miles

            var arrStyles = viewModel.Dancer.DanceStyles.Select(s => s.Id).ToArray();
            var suggestedClasses = DataContext.Events.OfType<Class>()
                                        .Where(y => y.DanceStyles.Any(d => arrStyles.Contains(d.Id)) && DbGeography.FromText("POINT(" + y.Place.Longitude.ToString() + " " + y.Place.Latitude.ToString() + ")").Distance(myLocation) * .00062 < 50 && !y.Users.Any(u => u.Id == id))
                                        .Include("Teachers")
                                        .Include("Teachers.ApplicationUser")
                                        .ToList();
            viewModel.SuggestedClasses = suggestedClasses.Where(e => e.NextDate >= today);

            viewModel.SuggestedSocials = new List<Social>();
            var suggestedSocials = DataContext.Events.OfType<Social>()
                                        .Where(y => y.DanceStyles.Any(d => arrStyles.Contains(d.Id)) && DbGeography.FromText("POINT(" + y.Place.Longitude.ToString() + " " + y.Place.Latitude.ToString() + ")").Distance(myLocation) * .00062 < 50 && !y.Users.Any(u => u.Id == id))
                                        .ToList();
            viewModel.SuggestedSocials = suggestedSocials.Where(e => e.NextDate >= today);

            //  viewModel.SuggestedEvents = DataContext.Events.Where(y => y.DanceStyles.Any(x => styles.Any(z => z.Id == x.Id))).ToList(); // In Miles
            //  viewModel.SuggestedEvents = DataContext.Events.Where(y => Geolocation.Distance(new Geolocation.Position() { Latitude = y.Place.Latitude, Longitude = y.Place.Longitude }, new Geolocation.Position() { Latitude = location.Latitude, Longitude = location.Longitude }, Geolocation.DistanceType.Miles) < 50).ToList();

            if (viewModel.Dancer.YouTubeUsername != null)
            {
                viewModel.YouTubeVideos = GetVideos(viewModel.Dancer.YouTubeUsername);
            }
            else
            {
                viewModel.YouTubeVideos = new List<YouTubeVideo>();
            }

            var teachers = viewModel.Dancer.Students;

            if (viewModel.Dancer.FacebookToken != null)
            {
                viewModel.FriendList = FacebookHelper.GetFriends(viewModel.Dancer.FacebookToken);

                foreach (FacebookFriend f in viewModel.FriendList)
                {
                    var user = DataContext.Users.Where(x => x.FacebookUsername == f.Id).FirstOrDefault();
                    if (user != null)
                    {
                        user.UserPictures = DataContext.Pictures.OfType<UserPicture>().Where(x => x.User.Id == user.Id).ToList();
                        user.UserPictures.Add(ApplicationUtility.GetNoProfilePicture());
                        f.User = user;
                    }
                }
            }

            return View(viewModel);
        }

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

            var lstMedia = new List<EventMedia>();
            var events = DataContext.Events.Where(e => e.Users.Any(u => u.Id == id));
            var newPictures = DataContext.Pictures.OfType<EventPicture>()
                                .Include("Event")
                                .Include("PostedBy")
                                .Where(p => events.Any(e => e.Id == p.Event.Id))
                                .OrderByDescending(p => p.PhotoDate)
                                .Take(20);
            foreach(var p in newPictures)
            {
                lstMedia.Add(new EventMedia() { Event = p.Event, Id = p.Id, Author = p.PostedBy, MediaDate = p.PhotoDate, MediaType = Enums.MediaType.Picture, PhotoUrl = p.Filename, Title = p.Title });
            }
            var newVideos = DataContext.Videos.OfType<EventVideo>()
                                .Include("Event")
                                .Include("Author")
                                .Where(v => events.Any(e => e.Id == v.Event.Id))
                                .OrderByDescending(v => v.PublishDate)
                                .Take(20);
            foreach(var v in newVideos)
            {
                lstMedia.Add(new EventMedia() { Event = v.Event, Id = v.Id, Author = v.Author, MediaDate = v.PublishDate, MediaType = Enums.MediaType.Video, PhotoUrl = v.PhotoUrl, MediaUrl = v.VideoUrl, Title = v.Title });
            }

            viewModel.MediaUpdates = lstMedia;

            viewModel.SuggestedClasses = new List<Class>();
            var myLocation = DbGeography.FromText("POINT(" + viewModel.Address.Longitude.ToString() + " " + viewModel.Address.Latitude.ToString() + ")");
            var arrStyles = viewModel.Dancer.DanceStyles.Select(s => s.Id).ToArray();
            var suggestedClasses = DataContext.Events.OfType<Class>()
                                        .Where(y => y.DanceStyles.Any(d => arrStyles.Contains(d.Id)) && DbGeography.FromText("POINT(" + y.Place.Longitude.ToString() + " " + y.Place.Latitude.ToString() + ")").Distance(myLocation) * .00062 < 50 && !y.Users.Any(u => u.Id == id))
                                        .Include("DanceStyles")
                                        .Include("Teachers")
                                        .Include("Teachers.ApplicationUser")
                                        .ToList();
            viewModel.SuggestedClasses = suggestedClasses.Where(e => e.NextDate >= today);
            viewModel.SuggestedSocials = new List<Social>();
            var suggestedSocials = DataContext.Events.OfType<Social>()
                                        .Where(y => y.DanceStyles.Any(d => arrStyles.Contains(d.Id)) && DbGeography.FromText("POINT(" + y.Place.Longitude.ToString() + " " + y.Place.Latitude.ToString() + ")").Distance(myLocation) * .00062 < 50 && !y.Users.Any(u => u.Id == id))
                                        .Include("DanceStyles")
                                        .ToList();
            viewModel.SuggestedSocials = suggestedSocials.Where(e => e.NextDate >= today);

            return View(viewModel);
        }

        [Authorize]
        public ActionResult MyLearn(string username)
        {
            var viewModel = LoadDancerModel(username);

            if (viewModel.Dancer == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var today = DateTime.Today;
            var id = User.Identity.GetUserId();

            viewModel.Teachers = new List<Teacher>();
            viewModel.Teachers = DataContext.Teachers.Where(x => x.Students.Any(s => s.DancerId == id))
                                    .Include("ApplicationUser")
                                    .Include("ApplicationUser.UserPictures")
                                    .ToList();
            viewModel.Classes = new EventListViewModel();
            var classes = DataContext.Events.OfType<Class>().Where(x => x.Users.Any(u => u.UserName == username))
                                    .Include("Users")
                                    .Include("Teachers")
                                    .Include("DanceStyles")
                                    .Include("Pictures")
                                    .Include("Videos")
                                    .ToList();
            viewModel.Classes.Events = classes.Where(e => e.NextDate >= today);
            viewModel.Classes.Location = viewModel.Address;

            //var scheduler = new DHXScheduler(this) { Skin = DHXScheduler.Skins.Terrace };
            //scheduler.Templates.map_time = "{start_date.toLocaleString()}"; //   "{start_date.toLocaleTimeString()}";    // "{start_date:date(%d.%m.%Y)}";
            //scheduler.Views.Clear();
            //scheduler.Views.Add(new MonthView());
            //scheduler.Views.Add(new MapView());
            //scheduler.InitialView = (new MonthView()).Name;
            //scheduler.LoadData = true;
            //scheduler.DataAction = "MapEvents";

            //viewModel.Scheduler = scheduler;

            return View(viewModel);
        }

        [Authorize]
        public ActionResult MyFriends(string username)
        {
            var viewModel = LoadDancerModel(username);

            if (viewModel.Dancer == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var today = DateTime.Today;
            var id = User.Identity.GetUserId();

            if (viewModel.Dancer.FacebookToken != null)
            {
                viewModel.FriendList = FacebookHelper.GetFriends(viewModel.Dancer.FacebookToken);

                foreach (FacebookFriend f in viewModel.FriendList)
                {
                    var user = DataContext.Users.Where(x => x.FacebookUsername == f.Id).FirstOrDefault();
                    if (user != null)
                    {
                        user.UserPictures = DataContext.Pictures.OfType<UserPicture>().Where(x => x.User.Id == user.Id).ToList();
                        user.UserPictures.Add(ApplicationUtility.GetNoProfilePicture());
                        f.User = user;
                    }
                }
            }

            return View(viewModel);
        }

        [Authorize]
        public ActionResult MyDance(string username)
        {
            var viewModel = LoadDancerModel(username);

            if (viewModel.Dancer == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var today = DateTime.Today;
            var id = User.Identity.GetUserId();

            if (viewModel.Dancer.FacebookToken != null)
            {
                viewModel.FriendList = FacebookHelper.GetFriends(viewModel.Dancer.FacebookToken);

                foreach (FacebookFriend f in viewModel.FriendList)
                {
                    var user = DataContext.Users.Where(x => x.FacebookUsername == f.Id).FirstOrDefault();
                    if (user != null)
                    {
                        user.UserPictures = DataContext.Pictures.OfType<UserPicture>().Where(x => x.User.Id == user.Id).ToList();
                        user.UserPictures.Add(ApplicationUtility.GetNoProfilePicture());
                        f.User = user;
                    }
                }
            }

            viewModel.Socials = new EventListViewModel();
            viewModel.Socials.Events = DataContext.Events
                                            .OfType<Social>()
                                            .Include("Pictures")
                                            .Include("Users")
                                            .Include("Videos")
                                            .Where(x => x.Users.Any(u => u.UserName == username)).ToList();
            viewModel.Socials.Location = viewModel.Address;

            return View(viewModel);
        }

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

        [Authorize]
        public ActionResult Dance(string username)
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

            var viewModel = new DancerViewViewModel();
            viewModel.Dancer = dancer;
            var location = Geolocation.ParseAddress(dancer.ZipCode);

            if (dancer.ZipCode != null)
            {
                viewModel.Address = Geolocation.ParseAddress(dancer.ZipCode);
            }
            else
            {
                viewModel.Address = Geolocation.ParseAddress("90065");
            }

            return View(viewModel);
        }

        [Authorize]
        public ActionResult Edit()
        {
            // TODO: FILL MORE VIEWMODEL PROPERTIES (SEE DancerViewModel)
            var viewModel = GetInitialDancerEditViewModel();

            if (viewModel.Dancer == null)
            {
                return HttpNotFound();
            }

            return View(viewModel);
        }

        private DancerEditViewModel GetInitialDancerEditViewModel()
        {
            var model = new DancerEditViewModel();
            var id = User.Identity.GetUserId();
            model.Dancer = DataContext.Users.Where(x => x.Id == id).Include("DanceStyles").FirstOrDefault();

            var selectedStyles = new List<DanceStyleListItem>();
            foreach (DanceStyle ss in model.Dancer.DanceStyles)
            {
                selectedStyles.Add(new DanceStyleListItem { Id = ss.Id, Name = ss.Name });
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
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Edit(DancerEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                var dancer = DataContext.Users.Where(x => x.Id == model.Dancer.Id).Include("DanceStyles").Include("Parties").FirstOrDefault();
                dancer.Experience = model.Dancer.Experience;
                dancer.YouTubeUsername = model.Dancer.YouTubeUsername;
                dancer.SpotifyUri = model.Dancer.SpotifyUri;
                dancer.ZipCode = model.Dancer.ZipCode;

                if (model.PostedStyles != null)
                {
                    var styles = DataContext.DanceStyles.Where(x => model.PostedStyles.DanceStyleIds.Contains(x.Id.ToString())).ToList();

                    dancer.DanceStyles.Clear();

                    foreach (DanceStyle s in styles)
                    {
                        dancer.DanceStyles.Add(s);
                    }

                }

                DataContext.Entry(dancer).State = EntityState.Modified;
                DataContext.SaveChanges();
                return RedirectToAction("Home", "Dancer", new { username = dancer.UserName });
            }
            return View(model);
        }

        [HttpPost]
        [Authorize]
        public ActionResult AddStyle(DancerViewViewModel model)
        {
            return View(model);
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

        [Authorize]
        public ActionResult UploadPicture()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public ActionResult UploadPicture(HttpPostedFileBase file)
        {
            UploadFile newFile = ApplicationUtility.LoadPicture(file);

            if (newFile.UploadStatus == "Success")
            {
                var dancer = DataContext.Users.Where(x => x.UserName == User.Identity.Name).Include("UserPictures").FirstOrDefault();
                var today = DateTime.Now;
                dancer.UserPictures.Add(new UserPicture() { Title = newFile.FileName, Filename = newFile.FilePath, ThumbnailFilename = newFile.ThumbnailFilePath, PhotoDate = today });
                DataContext.Entry(dancer).State = EntityState.Modified;
                DataContext.SaveChanges();
            }
            else
            {
                ViewBag.Message = newFile.UploadStatus;
            }
            return RedirectToAction("ChangePicture", "Dancer", new { username = User.Identity.Name });
        }

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

        [Authorize]
        public ActionResult ChangePicture()
        {
            var model = new ChangePictureViewModel();

            var id = User.Identity.GetUserId();
            model.Dancer = DataContext.Users.Where(x => x.Id == id).Include("UserPictures").FirstOrDefault();

            if (model.Dancer == null)
            {
                return HttpNotFound();
            }

            if (model.Dancer.FacebookToken != null)
            {
                model.FacebookPictures = FacebookHelper.GetPhotos(model.Dancer.FacebookToken);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult ChangePicture(ChangePictureViewModel model)
        {
            return View(model);
        }

        [Authorize]
        public ActionResult ProfilePicture(int id)
        {
            try
            {
                string userId = User.Identity.GetUserId();

                var dancer = DataContext.Users.Where(x => x.Id == userId).Include("UserPictures").FirstOrDefault();

                foreach (UserPicture p in dancer.UserPictures)
                {
                    if (p.Id == id)
                    {
                        p.ProfilePicture = true;
                    }
                    else
                    {
                        p.ProfilePicture = false;
                    }
                }

                DataContext.Entry(dancer).State = EntityState.Modified;
                DataContext.SaveChanges();
                return RedirectToAction("ChangePicture", "Dancer", new { username = User.Identity.Name });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ChangePicture", "Dancer", new { username = User.Identity.Name });
            }
        }

        [Authorize]
        public ActionResult AddFacebookPicture(string id, string album, string name, string largeSource, string link, DateTime photodate, string source)
        {
            try
            {
                string userId = User.Identity.GetUserId();
                var dancer = DataContext.Users.Where(x => x.Id == userId).Include("UserPictures").FirstOrDefault();
                dancer.UserPictures.Add(new UserPicture() { Title = name, ThumbnailFilename = source, Filename = largeSource });
                DataContext.Entry(dancer).State = EntityState.Modified;
                DataContext.SaveChanges();
                return RedirectToAction("ChangePicture", "Dancer", new { username = User.Identity.Name });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ChangePicture", "Dancer", new { username = User.Identity.Name });
            }
        }

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
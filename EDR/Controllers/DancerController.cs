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
using DayPilot.Web.Mvc.Events.Calendar;
using EDR.Data;

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

        [Authorize]
        public ActionResult View(string username)
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
            if(dancer == null)
            {
                return HttpNotFound();
            }

            var today = DateTime.Today;

            var viewModel = new DancerViewViewModel();
            viewModel.Dancer = dancer;
            var location = Geolocation.ParseAddress(dancer.ZipCode);
            viewModel.Teachers = new List<Teacher>();
            viewModel.Teachers = DataContext.Teachers.Where(x => x.Students.Any(s => s.DancerId == dancer.Id)).Include("ApplicationUser").Include("ApplicationUser.UserPictures").ToList();
            viewModel.Classes = new List<Class>();
            var classes = DataContext.Events.OfType<Class>().Where(x => x.Users.Any(u => u.UserName == username)).ToList();
            viewModel.Classes = classes.Where(e => e.NextDate >= today);
            viewModel.Socials = new List<Social>();
            viewModel.Socials = DataContext.Events.OfType<Social>().Where(x => x.Users.Any(u => u.UserName == username)).ToList();
            viewModel.Concerts = new List<Concert>();
            viewModel.Concerts = DataContext.Events.OfType<Concert>().Where(x => x.Users.Any(u => u.UserName == username)).ToList();
            viewModel.Conferences = new List<Conference>();
            viewModel.Conferences = DataContext.Events.OfType<Conference>().Where(x => x.Users.Any(u => u.UserName == username)).ToList();
            viewModel.OpenHouses = new List<OpenHouse>();
            viewModel.OpenHouses = DataContext.Events.OfType<OpenHouse>().Where(x => x.Users.Any(u => u.UserName == username)).ToList();
            viewModel.Parties = new List<Party>();
            viewModel.Parties = DataContext.Events.OfType<Party>().Where(x => x.Users.Any(u => u.UserName == username)).ToList();
            viewModel.SuggestedEvents = new List<Event>();
            //  y.DanceStyles.Any(x => dancer.DanceStyles.Contains(x)) && y.NextDate >= today && 
            var myLocation = DbGeography.FromText("POINT(" + location.Longitude.ToString() + " " + location.Latitude.ToString() + ")");
            //  viewModel.SuggestedEvents = DataContext.Events.Where(y => y.DanceStyles.Any(x => dancer.DanceStyles.Contains(x)) && DbGeography.FromText("POINT(" + y.Place.Longitude.ToString() + " " + y.Place.Latitude.ToString() + ")").Distance(myLocation) * .00062 < 50).ToList(); // In Miles

            var arrStyles = dancer.DanceStyles.Select(s => s.Id).ToArray();
            var suggestedEvents = DataContext.Events.Where(y => y.DanceStyles.Any(d => arrStyles.Contains(d.Id)) && DbGeography.FromText("POINT(" + y.Place.Longitude.ToString() + " " + y.Place.Latitude.ToString() + ")").Distance(myLocation) * .00062 < 50 && !y.Users.Any(u => u.Id == dancer.Id)).ToList();
            viewModel.SuggestedEvents = suggestedEvents.Where(e => e.NextDate >= today);

            //  viewModel.SuggestedEvents = DataContext.Events.Where(y => y.DanceStyles.Any(x => styles.Any(z => z.Id == x.Id))).ToList(); // In Miles
            //  viewModel.SuggestedEvents = DataContext.Events.Where(y => Geolocation.Distance(new Geolocation.Position() { Latitude = y.Place.Latitude, Longitude = y.Place.Longitude }, new Geolocation.Position() { Latitude = location.Latitude, Longitude = location.Longitude }, Geolocation.DistanceType.Miles) < 50).ToList();
            
            if (dancer.ZipCode != null)
            {
                viewModel.Address = Geolocation.ParseAddress(dancer.ZipCode);
            }
            else
            {
                viewModel.Address = Geolocation.ParseAddress("90065");
            }
            if (dancer.YouTubeUsername != null)
            {
                viewModel.YouTubeVideos = GetVideos(dancer.YouTubeUsername);
            }
            else
            {
                viewModel.YouTubeVideos = new List<YouTubeVideo>();
            }

            var teachers = dancer.Students;

            if (dancer.FacebookToken != null)
            {
                viewModel.FriendList = FacebookHelper.GetFriends(dancer.FacebookToken);

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
                return RedirectToAction("View", "Dancer", new { username = dancer.UserName });
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
                dancer.UserPictures.Add(new UserPicture() { Title = newFile.FileName, Filename = newFile.FilePath, ThumbnailFilename = newFile.ThumbnailFilePath });
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
            return new Dpc().CallBack(this);
        }

        public class Dpc : DayPilotCalendar
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

            protected override void OnEventMove(DayPilot.Web.Mvc.Events.Calendar.EventMoveArgs e)
            {
                //if (new EventManager(Controller).Get(e.Id) != null)
                //{
                //    new EventManager(Controller).EventMove(e.Id, e.NewStart, e.NewEnd);
                //}

                Update();
            }

            protected override void OnEventClick(EventClickArgs e)
            {
                
            }

            protected override void OnEventResize(DayPilot.Web.Mvc.Events.Calendar.EventResizeArgs e)
            {
                //  new EventManager(Controller).EventMove(e.Id, e.NewStart, e.NewEnd);
                Update();
            }

            protected override void OnCommand(CommandArgs e)
            {
                switch (e.Command)
                {
                    case "navigate":
                        StartDate = (DateTime) e.Data["start"];
                        Update(CallBackUpdateType.Full);
                        break;

                    case "refresh":
                        Update();
                        break;

                    case "previous":
                        StartDate = StartDate.AddDays(-7);
                        Update(CallBackUpdateType.Full);
                        break;

                    case "next":
                        StartDate = StartDate.AddDays(7);
                        Update(CallBackUpdateType.Full);
                        break;

                    case "today":
                        StartDate = DateTime.Today;
                        Update(CallBackUpdateType.Full);
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
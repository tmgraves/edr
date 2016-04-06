using EDR.Models.ViewModels;
using System;
using System.Data;
using System.Data.Entity;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using EDR.Models;
using System.Text.RegularExpressions;
using EDR.Utilities;
using System.Data.Entity.Validation;
using EDR.Enums;

namespace EDR.Controllers
{
    public class TeacherController : BaseController
    {
        public ActionResult List(TeacherListViewModel model)
        {
            model.Teachers = DataContext.Teachers.Include("ApplicationUser").Include("DanceStyles").Include("ApplicationUser.UserPictures").Include("Classes").Include("Classes.Reviews");
            model.DanceStyles = DataContext.DanceStyles;
            model.Zoom = model.Zoom == 0 ? 10 : model.Zoom;

            if (model.Location != "" && model.Location != null)
            {
                var address = new Address();
                address = Geolocation.ParseAddress(model.Location);
                model.CenterLat = address.Latitude;
                model.CenterLng = address.Longitude;
                model.NELat = model.CenterLat + .5;
                model.SWLat = model.CenterLat - .5;
                model.NELng = model.CenterLng + .5;
                model.SWLng = model.CenterLng - .5;
            }

            if (model.NELat != null && model.NELng != null)
            {
                model.Teachers = model.Teachers.Where(t => t.ApplicationUser.Longitude != null && t.ApplicationUser.Longitude >= model.SWLng && t.ApplicationUser.Longitude <= model.NELng && t.ApplicationUser.Latitude != null && t.ApplicationUser.Latitude >= model.SWLat && t.ApplicationUser.Latitude <= model.NELat);
            }

            if (model.DanceStyleId != null)
            {
                model.Teachers = model.Teachers.Where(t => t.DanceStyles.Any(s => s.Id == model.DanceStyleId));
            }

            return View(model);
        }

        private TeacherViewViewModel LoadTeacher(string username)
        {
            // TODO: FILL MORE VIEWMODEL PROPERTIES (SEE TeacherViewModel)
            var viewModel = new TeacherViewViewModel();
            viewModel.Teacher = DataContext.Teachers
                                    .Include("Classes")
                                    .Include("Classes.Users")
                                    .Include("Classes.Reviews")
                                    .Include("DanceStyles")
                                    .Include("ApplicationUser")
                                    .Include("ApplicationUser.UserPictures")
                                    .Include("ApplicationUser.Roles")
                                    .Include("Students.Dancer")
                                    .Include("Students.Dancer.UserPictures")
                                    .Include("Classes.Playlists")
                                    .Include("Classes.Playlists.Author")
                                    .Include("Classes.Place")
                                    .Include("Places")
                                    .Include("Schools")
                                    .Where(x => x.ApplicationUser.UserName == username)
                                    .FirstOrDefault();
            viewModel.Events = new EventListViewModel();

            if (viewModel.Teacher.ApplicationUser.ZipCode != null)
            {
                viewModel.Address = Geolocation.ParseAddress(viewModel.Teacher.ApplicationUser.ZipCode);
                viewModel.Events.Location = viewModel.Address;
            }
            else
            {
                viewModel.Address = Geolocation.ParseAddress("90065");
                viewModel.Events.Location = viewModel.Address;
            }

            // TODO: FILL MORE VIEWMODEL PROPERTIES (SEE PromoterViewModel)
            viewModel.Events.EventType = Enums.EventType.Class;
            viewModel.Events.Events = new List<Event>();
            viewModel.Events.Events = viewModel.Teacher.Classes;

            //  Load Roles
            viewModel.Roles = new List<RoleName>();
            if (UserManager.IsInRole(viewModel.Teacher.ApplicationUser.Id, "Owner"))
            {
                viewModel.Roles.Add(RoleName.Owner);
            }
            if (UserManager.IsInRole(viewModel.Teacher.ApplicationUser.Id, "Promoter"))
            {
                viewModel.Roles.Add(RoleName.Promoter);
            }
            //  Load Roles

            ////  Set Role
            //if (User.Identity.IsAuthenticated)
            //{
            //    Session["MyRole"] = RoleName.Teacher;
            //}
            ////  Set Role

            return viewModel;
        }

        //[Authorize]
        //public ActionResult View(string username)
        //{
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

        //    var teacher = DataContext.Teachers.Include("Classes")
        //        .Where(x => x.ApplicationUser.UserName == username)
        //        .FirstOrDefault();

        //    if (teacher == null)
        //    {
        //        if (username == User.Identity.Name && !User.IsInRole("Teacher"))
        //        {
        //            return RedirectToAction("Apply", "Teacher");
        //        }
        //        else
        //        {
        //            return HttpNotFound();
        //        }
        //    }

        //    var viewModel = LoadTeacher(username);

        //    return View(viewModel);
        //}

        [Authorize]
        public ActionResult Home(string username)
        {
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

            var teacher = DataContext.Teachers
                .Where(x => x.ApplicationUser.UserName == username)
                .FirstOrDefault();

            if (teacher == null || teacher.Approved == null)
            {
                if (username == User.Identity.Name && !User.IsInRole("Teacher"))
                {
                    return RedirectToAction("Apply", "Teacher");
                }
                else
                {
                    return HttpNotFound();
                }
            }

            var viewModel = LoadTeacher(username);

            //  Media Updates
            var lstMedia = new List<EventMedia>();
            var events = DataContext.Events.OfType<Class>().Where(e => e.Teachers.Any(t => t.Id == viewModel.Teacher.Id));
            var newPictures = DataContext.Pictures.OfType<EventPicture>()
                                .Include("Event")
                                .Include("PostedBy")
                                .Where(p => events.Any(e => e.Id == p.Event.Id))
                                .OrderByDescending(p => p.PhotoDate)
                                .Take(20);
            foreach (var p in newPictures)
            {
                lstMedia.Add(new EventMedia() { Event = p.Event, SourceName = p.Title, SourceLink = p.SourceLink, Id = p.Id, Author = p.PostedBy, MediaDate = p.PhotoDate, MediaType = Enums.MediaType.Picture, PhotoUrl = p.Filename, Title = p.Title });
            }
            var newVideos = DataContext.Videos.OfType<EventVideo>()
                                .Include("Event")
                                .Include("Author")
                                .Where(v => events.Any(e => e.Id == v.Event.Id))
                                .OrderByDescending(v => v.PublishDate)
                                .Take(20);
            foreach (var v in newVideos)
            {
                lstMedia.Add(new EventMedia() { Event = v.Event, SourceName = v.Title, SourceLink = v.VideoUrl, Id = v.Id, Author = v.Author, MediaDate = v.PublishDate, MediaType = Enums.MediaType.Video, PhotoUrl = v.PhotoUrl, MediaUrl = v.VideoUrl, Title = v.Title });
            }

            foreach (var cls in viewModel.Teacher.Classes)
            {
                foreach (var list in cls.Playlists)
                {
                    var videos = YouTubeHelper.GetPlaylistVideos(list.YouTubeId);

                    foreach (var movie in videos)
                    {
                        lstMedia.Add(new EventMedia() { Event = cls, SourceName = movie.Title, SourceLink = movie.VideoLink.ToString(), Author = list.Author, MediaDate = movie.PubDate, MediaType = Enums.MediaType.Video, PhotoUrl = movie.Thumbnail.ToString(), MediaUrl = movie.VideoLink.ToString(), Title = movie.Title });
                    }
                }
            }
            viewModel.MediaUpdates = lstMedia;
            //  Media Updates

            viewModel.NewClasses = new EventListViewModel();
            viewModel.NewClasses.EventType = Enums.EventType.Class;
            viewModel.NewClasses.Location = viewModel.Address;
            viewModel.NewClasses.Events = new List<Event>();
            viewModel.NewClasses.Events = viewModel.Teacher.Classes.Where(s => s.NextDate >= DateTime.Today).OrderByDescending(e => e.NextDate).Take(5);

            viewModel.NewStudents = new List<ApplicationUser>();
            var classArray = viewModel.Teacher.Classes.Select(c => c.Id).ToArray();
            viewModel.NewStudents = DataContext.Users.Include("Events").Where(u => u.Events.Any(e => classArray.Contains(e.Id)));

            //  Load Facebook Events
            if (User.Identity.IsAuthenticated)
            {
                var userid = User.Identity.GetUserId();
                var user = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();

                if (user.FacebookToken != null)
                {
                    viewModel.FacebookEvents = FacebookHelper.GetEvents(user.FacebookToken, DateTime.Now).Where(fe => !DataContext.Events.Select(e => e.FacebookId).Contains(fe.Id)).ToList();
                }
                //  Load Facebook Events
            }

            Session["ReturnUrl"] = Url.Action("Home", "Teacher", new { username = User.Identity.Name });

            return View(viewModel);
        }

        public ActionResult GetUpdates(string username)
        {
            var evt = DataContext.Events.OfType<Class>().Where(c => c.Teachers.Any(t => t.ApplicationUser.UserName == username))
                    .Include("Creator")
                    .Include("Pictures")
                    .Include("Pictures.PostedBy")
                    .Include("Videos")
                    .Include("Videos.Author")
                    .Include("Playlists")
                    .Include("Playlists.Author")
                    .Include("LinkedFacebookObjects")
                    .Cast<Event>();

            var lstMedia = EventHelper.BuildAllUpdates(evt, MediaTarget.User);

            return PartialView("~/Views/Shared/_MediaUpdatesPartial.cshtml", lstMedia);
            //  Media Updates

        }

        [Authorize]
        public ActionResult MyTeach(string username)
        {
            Session["ReturnUrl"] = Url.Action("MyTeach", "Teacher", new { username = User.Identity.Name });
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

            var teacher = DataContext.Teachers
                .Where(x => x.ApplicationUser.UserName == username)
                .FirstOrDefault();

            if (teacher == null || teacher.Approved == null)
            {
                if (username == User.Identity.Name && !User.IsInRole("Teacher"))
                {
                    return RedirectToAction("Apply", "Teacher");
                }
                else
                {
                    return HttpNotFound();
                }
            }

            var viewModel = LoadTeacher(username);
            viewModel.Events.ReturnUrl = Url.Action("MyTeach", "Teacher", new { id = teacher.Id });

            return View(viewModel);
        }

        [Authorize]
        public ActionResult Resume(string username)
        {
            Session["ReturnUrl"] = Url.Action("Resume", "Teacher", new { username = User.Identity.Name });
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

            var teacher = DataContext.Teachers
                .Where(x => x.ApplicationUser.UserName == username)
                .FirstOrDefault();

            if (teacher == null || teacher.Approved == null)
            {
                if (username == User.Identity.Name && !User.IsInRole("Teacher"))
                {
                    return RedirectToAction("Apply", "Teacher");
                }
                else
                {
                    return HttpNotFound();
                }
            }

            var viewModel = LoadTeacher(username);

            return View(viewModel);
        }

        [Authorize]
        public ActionResult Edit()
        {
            var viewModel = new TeacherEditViewModel();
            LoadStyles(viewModel);

            if (viewModel.Teacher == null)
            {
                return HttpNotFound();
            }

            return View(viewModel);
        }

        private void LoadStyles(TeacherEditViewModel model)
        {
            var id = User.Identity.GetUserId();
            model.Teacher = DataContext.Teachers.Where(x => x.ApplicationUser.Id == id).Include("ApplicationUser").Include("DanceStyles").FirstOrDefault();

            var selectedStyles = new List<DanceStyleListItem>();
            foreach (DanceStyle ss in model.Teacher.DanceStyles)
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
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(TeacherEditViewModel model)
        {
            try
            {
                var teacher = DataContext.Teachers.Where(x => x.ApplicationUser.Id == model.Teacher.ApplicationUser.Id).Include("ApplicationUser").Include("DanceStyles").FirstOrDefault();
                teacher.StartDate = model.Teacher.StartDate;
                teacher.FacebookLink = model.Teacher.FacebookLink;
                teacher.Resume = model.Teacher.Resume;
                teacher.Website = model.Teacher.Website;
                teacher.ContactEmail = model.Teacher.ContactEmail;

                if (model.PostedStyles != null)
                {
                    var styles = DataContext.DanceStyles.Where(x => model.PostedStyles.DanceStyleIds.Contains(x.Id.ToString())).ToList();

                    teacher.DanceStyles.Clear();

                    foreach (DanceStyle s in styles)
                    {
                        teacher.DanceStyles.Add(s);
                    }
                }

                DataContext.Entry(teacher).State = EntityState.Modified;
                DataContext.SaveChanges();
                return RedirectToAction("MyTeach", "Teacher", new { username = teacher.ApplicationUser.UserName });
            }
            catch(Exception ex)
            {
                LoadStyles(model);
                return View(model);
            }
        }

        [Authorize]
        public ActionResult Apply()
        {
            // TODO: SAVE TEACHER RECORD WITH 'Active=false' ONCE APPROVED SWITCH TO 'True'
            //       AND ADD USER TO 'Teacher' ROLE

            var user = UserManager.FindByName(User.Identity.Name);

            var viewModel = new TeacherApplyViewModel();
            viewModel.Teacher = DataContext.Teachers.Where(x => x.ApplicationUser.Id == user.Id).FirstOrDefault();

            return View(viewModel);
        }

        // POST: Teacher Apply
        [HttpPost]
        public ActionResult Apply(Teacher teacher)
        {
            DataContext.Teachers.Add(new Teacher { ApplicationUser = DataContext.Users.Find(User.Identity.GetUserId()) });
            DataContext.SaveChanges();
            return RedirectToAction("Home", "Dancer", new { username = User.Identity.Name } );
        }
    }
}
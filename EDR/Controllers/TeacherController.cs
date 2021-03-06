﻿using EDR.Models.ViewModels;
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
using EDR.Attributes;

namespace EDR.Controllers
{
    public class TeacherController : BaseController
    {
        [Route("Teacher/Manage")]
        [AccessDeniedAuthorize(Roles = "Teacher", AccessDeniedAction = "Apply", AccessDeniedController = "Teacher")]
        public ActionResult Manage()
        {
            var model = new TeacherManageViewModel();
            var id = User.Identity.GetUserId();
            model.Teacher = DataContext.Teachers
                                .Include("Schools")
                                .Include("Teams")
                                .Single(t => t.ApplicationUser.Id == id);
            return View(model);
        }

        [Route("Teacher/Manage")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teacher")]
        public ActionResult Manage(TeacherManageViewModel model)
        {
            if (ModelState.IsValid)
            {
                var teacher = DataContext.Teachers.Include("ApplicationUser").Single(d => d.Id == model.Teacher.Id);
                TryUpdateModel(teacher, "Teacher");
                DataContext.Entry(teacher).State = EntityState.Modified;
                DataContext.SaveChanges();
                return RedirectToAction("Manage", "Teacher");
            }
            return View(model);
        }

        [Route("Teachers/{Location?}")]
        public ActionResult List(TeacherListViewModel model)
        {
            model.Teachers = DataContext.Teachers
                                .Include("ApplicationUser")
                                .Include("DanceStyles")
                                .Include("ApplicationUser.UserPictures")
                                .Include("Classes")
                                .Include("Classes.Reviews");

            if (model.Location != null)
            {
                var add = Geolocation.ParseAddress(model.Location);
                model.Teachers = model.Teachers.Where(e => (e.ApplicationUser.Longitude >= add.Longitude - .5 && e.ApplicationUser.Longitude <= add.Longitude + .5) && (e.ApplicationUser.Latitude >= add.Latitude - .5 && e.ApplicationUser.Latitude <= add.Latitude + .5)).ToList();
            }
            if (model.TeacherId != null)
            {
                model.Teachers = model.Teachers.Where(t => t.ApplicationUser.Id == model.TeacherId);
            }
            else if (model.Teacher != null)
            {
                model.Teachers = model.Teachers.Where(t => t.ApplicationUser.FullName.ToLower().Contains(model.Teacher.ToLower()));
            }
            if (model.DanceStyleId != null)
            {
                model.Teachers = model.Teachers.Where(t => t.DanceStyles.Select(st => st.Id).Contains((int)model.DanceStyleId));
            }
            if (model.NELat != null && model.SWLng != null)
            {
                model.Teachers = model.Teachers.Where(c => c.ApplicationUser.Longitude >= model.SWLng && c.ApplicationUser.Longitude <= model.NELng && c.ApplicationUser.Latitude >= model.SWLat && c.ApplicationUser.Latitude <= model.NELat);
            }

            model.Teachers = model.Teachers.ToList().Take(100);
            if (HttpContext.Request.Browser.IsMobileDevice)
            {
                return View("Mobile/List", model);
            }
            else
            {
                return View(model);
            }
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

            //  DON'T NEED THIS
            //if (viewModel.Teacher.ApplicationUser.ZipCode != null)
            //{
            //    viewModel.Address = Geolocation.ParseAddress(viewModel.Teacher.ApplicationUser.ZipCode);
            //    viewModel.Events.Location = viewModel.Address;
            //}
            //else
            //{
            //    viewModel.Address = Geolocation.ParseAddress("90065");
            //    viewModel.Events.Location = viewModel.Address;
            //}
            //  DON'T NEED THIS

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

        [Route("Teacher/AddStyle")]
        [Authorize]
        public PartialViewResult AddStyle(TeacherManageViewModel model)
        {
            var teacher = DataContext.Teachers.Include("ApplicationUser").Include("DanceStyles").Single(u => u.ApplicationUser.Id == model.Teacher.ApplicationUser.Id);
            if (!teacher.DanceStyles.Select(s => s.Id).Contains((int)model.NewStyleId))
            {
                teacher.DanceStyles.Add(DataContext.DanceStyles.Single(s => s.Id == model.NewStyleId));
                DataContext.SaveChanges();
            }
            var styles = DataContext.Teachers.Single(u => u.ApplicationUser.Id == model.Teacher.ApplicationUser.Id).DanceStyles.ToList();
            return PartialView("~/Views/Shared/_DancerStylesPartial.cshtml", new DancerStylesViewModel() { Id = model.Teacher.ApplicationUser.Id, Styles = styles, Controller = "Teacher" });
        }

        [Route("Teacher/DeleteStyle")]
        [Authorize]
        public PartialViewResult DeleteStyle(string id, int styleId)
        {
            var teacher = DataContext.Teachers.Single(u => u.ApplicationUser.Id == id);
            teacher.DanceStyles.Remove(DataContext.DanceStyles.Single(s => s.Id == styleId));
            DataContext.SaveChanges();
            return PartialView("~/Views/Shared/_DancerStylesPartial.cshtml", new DancerStylesViewModel() { Id = id, Styles = teacher.DanceStyles.ToList(), Controller = "Teacher" });
        }

        [Route("Teacher/Search")]
        public JsonResult Search(string searchString)
        {
            var teachers = DataContext.Teachers.Where(t => (t.ApplicationUser.FirstName + " " + t.ApplicationUser.LastName).ToLower().Contains(searchString.ToLower())).Select(s => new { Id = s.ApplicationUser.Id, Name = s.ApplicationUser.FirstName + " " + s.ApplicationUser.LastName }).ToList();
            return Json(teachers, JsonRequestBehavior.AllowGet);
        }

        [Route("Teacher/GetStudentsPartial")]
        [HttpGet]
        public virtual ActionResult GetStudentsPartial(string id)
        {
            var start = DateTime.Today;
            var students = DataContext.Classes.Where(c => c.Teachers.Any(t => t.ApplicationUser.Id == id)).SelectMany(c => c.EventInstances.SelectMany(i => i.EventRegistrations).Select(r => r.User)).Distinct().ToList();
            return PartialView("~/Views/Shared/DisplayTemplates/DancerThumbLinks.cshtml", students);
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

        [Route("Teacher/{username}")]
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

            ////  Media Updates
            //var lstMedia = new List<EventMedia>();
            //var events = DataContext.Events.OfType<Class>().Where(e => e.Teachers.Any(t => t.Id == viewModel.Teacher.Id));
            //var newPictures = DataContext.Pictures.OfType<EventPicture>()
            //                    .Include("Event")
            //                    .Include("PostedBy")
            //                    .Where(p => events.Any(e => e.Id == p.Event.Id))
            //                    .OrderByDescending(p => p.PhotoDate)
            //                    .Take(20);
            //foreach (var p in newPictures)
            //{
            //    lstMedia.Add(new EventMedia() { Event = p.Event, SourceName = p.Title, SourceLink = p.SourceLink, Id = p.Id, Author = p.PostedBy, MediaDate = p.PhotoDate, MediaType = Enums.MediaType.Picture, PhotoUrl = p.Filename, Title = p.Title });
            //}
            //var newVideos = DataContext.Videos.OfType<EventVideo>()
            //                    .Include("Event")
            //                    .Include("Author")
            //                    .Where(v => events.Any(e => e.Id == v.Event.Id))
            //                    .OrderByDescending(v => v.PublishDate)
            //                    .Take(20);
            //foreach (var v in newVideos)
            //{
            //    lstMedia.Add(new EventMedia() { Event = v.Event, SourceName = v.Title, SourceLink = v.VideoUrl.ToString(), Id = v.Id, Author = v.Author, MediaDate = v.PublishDate, MediaType = Enums.MediaType.Video, PhotoUrl = v.PhotoUrl.ToString(), MediaUrl = v.VideoUrl.ToString(), Title = v.Title });
            //}

            //foreach (var cls in viewModel.Teacher.Classes)
            //{
            //    foreach (var list in cls.Playlists)
            //    {
            //        var videos = YouTubeHelper.GetPlaylistVideos(list.YouTubeId);

            //        foreach (var movie in videos)
            //        {
            //            lstMedia.Add(new EventMedia() { Event = cls, SourceName = movie.Title, SourceLink = movie.VideoLink.ToString(), Author = list.Author, MediaDate = movie.PubDate, MediaType = Enums.MediaType.Video, PhotoUrl = movie.Thumbnail.ToString(), MediaUrl = movie.VideoLink.ToString(), Title = movie.Title });
            //        }
            //    }
            //}
            //viewModel.MediaUpdates = lstMedia;
            ////  Media Updates

            viewModel.NewClasses = new EventListViewModel();
            viewModel.NewClasses.EventType = Enums.EventType.Class;
            viewModel.NewClasses.Location = viewModel.Address;
            viewModel.NewClasses.Events = new List<Event>();
            viewModel.NewClasses.Events = viewModel.Teacher.Classes.Where(s => s.NextDate >= DateTime.Today).OrderByDescending(e => e.NextDate).Take(5);

            viewModel.NewStudents = new List<ApplicationUser>();
            var classArray = viewModel.Teacher.Classes.Select(c => c.Id).ToArray();
            viewModel.NewStudents = DataContext.Users.Include("Events").Where(u => u.Events.Any(e => classArray.Contains(e.Id)));

            //  DON'T NEED FACEBOOK EVENTS
            ////  Load Facebook Events
            //if (User.Identity.IsAuthenticated)
            //{
            //    var userid = User.Identity.GetUserId();
            //    var user = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();

            //    if (user.FacebookToken != null)
            //    {
            //        viewModel.FacebookEvents = FacebookHelper.GetEvents(user.FacebookToken, DateTime.Now).Where(fe => !DataContext.Events.Select(e => e.FacebookId).Contains(fe.Id)).ToList();
            //    }
            //    //  Load Facebook Events
            //}
            //  DON'T NEED FACEBOOK EVENTS

            //Session["ReturnUrl"] = Url.Action("Home", "Teacher", new { username = User.Identity.Name });

            return View(viewModel);
        }

        [Route("Teacher/GetUpdates")]
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

        //[Authorize]
        //public ActionResult MyTeach(string username)
        //{
        //    Session["ReturnUrl"] = Url.Action("MyTeach", "Teacher", new { username = User.Identity.Name });
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

        //    var teacher = DataContext.Teachers
        //        .Where(x => x.ApplicationUser.UserName == username)
        //        .FirstOrDefault();

        //    if (teacher == null || teacher.Approved == null)
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
        //    viewModel.Events.ReturnUrl = Url.Action("MyTeach", "Teacher", new { id = teacher.Id });

        //    return View(viewModel);
        //}

        //[Authorize]
        //public ActionResult Resume(string username)
        //{
        //    Session["ReturnUrl"] = Url.Action("Resume", "Teacher", new { username = User.Identity.Name });
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

        //    var teacher = DataContext.Teachers
        //        .Where(x => x.ApplicationUser.UserName == username)
        //        .FirstOrDefault();

        //    if (teacher == null || teacher.Approved == null)
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

        //[Authorize]
        //public ActionResult Edit()
        //{
        //    var viewModel = new TeacherEditViewModel();
        //    LoadStyles(viewModel);

        //    if (viewModel.Teacher == null)
        //    {
        //        return HttpNotFound();
        //    }

        //    return View(viewModel);
        //}

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

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit(TeacherEditViewModel model)
        //{
        //    try
        //    {
        //        var teacher = DataContext.Teachers.Where(x => x.ApplicationUser.Id == model.Teacher.ApplicationUser.Id).Include("ApplicationUser").Include("DanceStyles").FirstOrDefault();
        //        teacher.StartDate = model.Teacher.StartDate;
        //        teacher.FacebookLink = model.Teacher.FacebookLink;
        //        teacher.Resume = model.Teacher.Resume;
        //        teacher.Website = model.Teacher.Website;
        //        teacher.ContactEmail = model.Teacher.ContactEmail;

        //        if (model.PostedStyles != null)
        //        {
        //            var styles = DataContext.DanceStyles.Where(x => model.PostedStyles.DanceStyleIds.Contains(x.Id.ToString())).ToList();

        //            teacher.DanceStyles.Clear();

        //            foreach (DanceStyle s in styles)
        //            {
        //                teacher.DanceStyles.Add(s);
        //            }
        //        }

        //        DataContext.Entry(teacher).State = EntityState.Modified;
        //        DataContext.SaveChanges();
        //        return RedirectToAction("MyTeach", "Teacher", new { username = teacher.ApplicationUser.UserName });
        //    }
        //    catch(Exception ex)
        //    {
        //        LoadStyles(model);
        //        return View(model);
        //    }
        //}

        [Route("Teacher/Apply")]
        [Authorize]
        public ActionResult Apply()
        {
            // TODO: SAVE TEACHER RECORD WITH 'Active=false' ONCE APPROVED SWITCH TO 'True'
            //       AND ADD USER TO 'Teacher' ROLE

            var user = UserManager.FindByName(User.Identity.Name);

            var viewModel = new TeacherApplyViewModel();
            viewModel.Teacher = DataContext.Teachers.Where(x => x.ApplicationUser.Id == user.Id).FirstOrDefault();

            if (viewModel.Teacher == null)
            {
                viewModel.Teacher = new Teacher() { ApplicationUser = user };
            }

            return View(viewModel);
        }

        // POST: Teacher Apply
        [Route("Teacher/Apply")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Apply(TeacherApplyViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (model.Teacher.Id == 0)
                    {
                        model.Teacher.ApplicationUser = DataContext.Users.Find(User.Identity.GetUserId());
                        DataContext.Teachers.Add(model.Teacher);
                    }
                    else
                    {
                        DataContext.Entry(model.Teacher).State = EntityState.Modified;
                    }
                    DataContext.SaveChanges();
                    return RedirectToAction("Home", "Dancer", new { username = User.Identity.Name });
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
                    return View(model);
                }
            }
            else
            {
                return View(model);
            }
        }

        [Route("Teacher/GetClassesPartial")]
        [HttpGet]
        public virtual ActionResult GetClassesPartial(string id)
        {
            var start = DateTime.Today;
            var classes = DataContext.Classes
                                .Include("DanceStyles")
                                .Include("Place")
                                .Include("Tickets")
                                .Include("School.Tickets")
                                .Include("EventInstances.EventRegistrations")
                                .Include("Reviews")
                                .Include("Teachers.ApplicationUser")
                                .Where(c => c.Teachers.Any(t => t.ApplicationUser.Id == id))
                                .Where(c => c.EventInstances.Where(i => i.DateTime >= DateTime.Today).Count() != 0).ToList();
            return PartialView("~/Views/Shared/_EventsPartial.cshtml", classes);
        }

        #region JSON
        [Route("Teacher/GetSchools")]
        [Authorize(Roles = "Teacher")]
        public JsonResult GetSchools()
        {
            if (User.Identity.IsAuthenticated)
            {
                var userid = User.Identity.GetUserId();
                var schools = DataContext.Schools.Where(s => s.Teachers.Any(t => t.ApplicationUser.Id == userid));

                return Json(schools.Select(s => new { Name = s.Name, Id = s.Id }), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return null;
            }
        }

        [Route("Teacher/GetTeams")]
        [Authorize(Roles = "Teacher")]
        public JsonResult GetTeams()
        {
            if (User.Identity.IsAuthenticated)
            {
                var userid = User.Identity.GetUserId();
                var schools = DataContext.Teams.Where(s => s.Teachers.Any(t => t.ApplicationUser.Id == userid));

                return Json(schools.Select(s => new { Name = s.Name, Id = s.Id }), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return null;
            }
        }

        [Route("Teacher/GetClasses")]
        public JsonResult GetClasses(int teacherId, DateTime start, DateTime end)
        {
            var instances = DataContext.Classes.Where(c => c.Teachers.Any(t => t.Id == teacherId)).SelectMany(c => c.EventInstances).Where(i => i.DateTime >= start && i.DateTime <= end).ToList();

            var lst = instances.AsEnumerable().Select(s =>
                        new {
                            id = s.EventId,
                            title = s.Event.Name,
                            start = s.StartTime.Value.ToString("o"),
                            end = s.EndTime.Value.ToString("o"),
                            lat = s.Event.Place.Latitude,
                            lng = s.Event.Place.Longitude,
                            color = "#65AE25",
                            url = Url.Action("Class", "Event", new { id = s.EventId })
                        });
            return Json(lst, JsonRequestBehavior.AllowGet);
        }
        #endregion

        //[Authorize]
        //public ActionResult ApplyNew()
        //{
        //    // TODO: SAVE TEACHER RECORD WITH 'Active=false' ONCE APPROVED SWITCH TO 'True'
        //    //       AND ADD USER TO 'Teacher' ROLE

        //    var model = new Teacher();

        //    return View(model);
        //}
    }
}
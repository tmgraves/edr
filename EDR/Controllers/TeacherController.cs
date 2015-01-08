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

namespace EDR.Controllers
{
    public class TeacherController : BaseController
    {
        public ActionResult List(int? danceStyle)
        {
            var model = new TeacherListViewModel();
            model.Teachers = DataContext.Teachers.Include("ApplicationUser").Include("DanceStyles");
            model.DanceStyleList = DataContext.DanceStyles.Select(d => new SelectListItem() { Text = d.Name, Value = d.Id.ToString() }).ToList();

            if (danceStyle != null)
            {
                model.Teachers = model.Teachers.Where(x => x.DanceStyles.Any(s => s.Id == danceStyle));
            }
            return View(model);
        }

        private TeacherViewViewModel LoadTeacher(string username)
        {
            // TODO: FILL MORE VIEWMODEL PROPERTIES (SEE TeacherViewModel)
            var viewModel = new TeacherViewViewModel();
            viewModel.Teacher = DataContext.Teachers
                                    .Include("Classes")
                                    .Include("Workshops")
                                    .Include("DanceStyles")
                                    .Include("ApplicationUser")
                                    .Include("ApplicationUser.UserPictures")
                                    .Include("Students.Dancer")
                                    .Include("Students.Dancer.UserPictures")
                                    .Include("Places")
                                    .Where(x => x.ApplicationUser.UserName == username).FirstOrDefault();

            if (viewModel.Teacher.ApplicationUser.ZipCode != null)
            {
                viewModel.Address = Geolocation.ParseAddress(viewModel.Teacher.ApplicationUser.ZipCode);
            }
            else
            {
                viewModel.Address = Geolocation.ParseAddress("90065");
            }
            return viewModel;
        }

        [Authorize]
        public ActionResult View(string username)
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

            var teacher = DataContext.Teachers.Include("Classes")
                .Where(x => x.ApplicationUser.UserName == username)
                .FirstOrDefault();

            if (teacher == null)
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
        public ActionResult MyTeach(string username)
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

            if (teacher == null)
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
        public ActionResult Resume(string username)
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

            if (teacher == null)
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
        public ActionResult AddClass(string username)
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

            if (teacher == null)
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
            viewModel.NewClassModel = new ClassNewViewModel();
            if (viewModel.Teacher.ApplicationUser.FacebookToken != null)
            {
                viewModel.NewClassModel.FacebookEvents = FacebookHelper.GetEvents(viewModel.Teacher.ApplicationUser.FacebookToken);
            }

            return View(viewModel);
        }

        [Authorize]
        public ActionResult AddFacebookClass(string id, string name, string photoSource, string description, DateTime start, DateTime? end, string location, string ticketUri, string privacy, string timezone, string link, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var userid = User.Identity.GetUserId();
                    var teacher = DataContext.Teachers.Where(x => x.ApplicationUser.Id == userid).Include("ApplicationUser").FirstOrDefault();

                    Address ad = Utilities.Geolocation.ParseAddress(teacher.ApplicationUser.ZipCode != null ? teacher.ApplicationUser.ZipCode : "90065");
                    Place pl = new Place() { Name = location, PlaceType = Enums.PlaceType.OtherPlace, Zip = teacher.ApplicationUser.ZipCode != null ? teacher.ApplicationUser.ZipCode : "90065", Address = ad.StreetNumber + " " + ad.StreetName, City = ad.City, State = (Enums.State)System.Enum.Parse(typeof(Enums.State), ad.State), Latitude = ad.Latitude, Longitude = ad.Longitude };

                    teacher.Classes.Add(new Class() { Name = name, ClassType = Enums.ClassType.Class, Description = description, EndDate = end, FacebookId = id, StartDate = start, PhotoUrl = photoSource, Place = pl });
                    DataContext.Entry(teacher).State = EntityState.Modified;
                    DataContext.SaveChanges();
                    return Redirect(returnUrl);
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
            return View();
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
            if (ModelState.IsValid)
            {
                var teacher = DataContext.Teachers.Where(x => x.ApplicationUser.Id == model.Teacher.ApplicationUser.Id).Include("ApplicationUser").Include("DanceStyles").FirstOrDefault();
                teacher.Experience = model.Teacher.Experience;
                teacher.FacebookLink = model.Teacher.FacebookLink;
                teacher.Resume = model.Teacher.Resume;
                teacher.Website = model.Teacher.Website;
                teacher.ContactEmail = model.Teacher.ContactEmail;

                var styles = DataContext.DanceStyles.Where(x => model.PostedStyles.DanceStyleIds.Contains(x.Id.ToString())).ToList();

                teacher.DanceStyles.Clear();

                foreach (DanceStyle s in styles)
                {
                    teacher.DanceStyles.Add(s);
                }

                DataContext.Entry(teacher).State = EntityState.Modified;
                DataContext.SaveChanges();
                return RedirectToAction("MyTeach", "Teacher", new { username = teacher.ApplicationUser.UserName });
            }
            LoadStyles(model);
            return View(model);
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
            return RedirectToAction("Manage", "Account");
        }
    }
}
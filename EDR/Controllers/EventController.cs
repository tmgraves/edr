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

namespace EDR.Controllers
{
    public class EventController : BaseController
    {
        public ActionResult Details(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var model = DataContext.Events.Where(x => x.Id == id).FirstOrDefault();
            if (model == null)
            {
                return HttpNotFound();
            }

            DataContext.Entry(model).Collection(x => x.DanceStyles).Load();

            return View(model);
        }

        public ActionResult Signup(int id)
        {
            var viewModel = new EventSignupViewModel();
            viewModel.EventId = id;
            viewModel.Event = DataContext.Events.Find(id);
            viewModel.UserId = DataContext.Users.Find(User.Identity.GetUserId()).Id;

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Signup(EventSignupViewModel model)
        {
            var user = DataContext.Users.Where(x => x.Id == model.UserId).FirstOrDefault();
            var userevent = DataContext.Events.Include("Users").Where(x => x.Id == model.EventId).FirstOrDefault();
            if (!userevent.Users.Contains(user))
            {
                userevent.Users.Add(user);
                DataContext.SaveChanges();
            }
            return RedirectToAction("Learn", "Home");
        }

        [Authorize(Roles = "Teacher, Owner")]
        public ActionResult Create(string role, string eventType)
        {
            var model = GetInitialClassCreateViewModel(role, eventType);
            return View(model);
        }

        private EventCreateViewModel GetInitialClassCreateViewModel(string role, string eventType)
        {
            var model = new EventCreateViewModel();
            var id = User.Identity.GetUserId();
            model.Role = role;
            model.EventType = eventType;

            var selectedStyles = new List<DanceStyleListItem>();
            model.SelectedStyles = selectedStyles;

            var styles = new List<DanceStyleListItem>();
            foreach (DanceStyle s in DataContext.DanceStyles)
            {
                styles.Add(new DanceStyleListItem { Id = s.Id, Name = s.Name });
            }
            model.AvailableStyles = styles.OrderBy(x => x.Name);

            if (role == "Teacher")
            {
                model.PlaceList = DataContext.Places.Where(x => x.Teachers.Any(t => t.ApplicationUser.Id == id)).Select(p => new SelectListItem() { Text = p.Name, Value = p.Id.ToString() }).ToList();
            }
            else if (role == "Owner")
            {
                model.PlaceList = DataContext.Places.Where(x => x.Owners.Any(t => t.ApplicationUser.Id == id)).Select(p => new SelectListItem() { Text = p.Name, Value = p.Id.ToString() }).ToList();
            }
            //  model.PlaceList = new SelectList(DataContext.Places.Where(x => x.Teachers.Any(t => t.ApplicationUser.Id == id) || x.Owners.Any(t => t.ApplicationUser.Id == id)).OrderBy(x => x.Name), "Id", "Name", null);
            //  model.PlaceList = DataContext.Places.Where(x => x.Teachers.Any(t => t.ApplicationUser.Id == id) || x.Owners.Any(t => t.ApplicationUser.Id == id)).Select(p => new SelectListItem() { Text = p.Name, Value = p.Id.ToString() }).ToList();

            return model;
        }

        private void LoadModel(EventCreateViewModel model)
        {
            var id = User.Identity.GetUserId();

            var selectedStyles = new List<DanceStyleListItem>();
            model.SelectedStyles = selectedStyles;

            var styles = new List<DanceStyleListItem>();
            foreach (DanceStyle s in DataContext.DanceStyles)
            {
                styles.Add(new DanceStyleListItem { Id = s.Id, Name = s.Name });
            }
            model.AvailableStyles = styles.OrderBy(x => x.Name);

            if (model.Role == "Teacher")
            {
                model.PlaceList = DataContext.Places.Where(x => x.Teachers.Any(t => t.ApplicationUser.Id == id)).Select(p => new SelectListItem() { Text = p.Name, Value = p.Id.ToString() }).ToList();
            }
            else if (model.Role == "Owner")
            {
                model.PlaceList = DataContext.Places.Where(x => x.Owners.Any(t => t.ApplicationUser.Id == id)).Select(p => new SelectListItem() { Text = p.Name, Value = p.Id.ToString() }).ToList();
            }
        }

        [Authorize(Roles = "Teacher, Owner")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(EventCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var event1 = model.Event;
                event1.Day = model.Event.StartDate.DayOfWeek;
                event1.StartDate = event1.StartDate.AddHours(model.Time.Hour).AddMinutes(model.Time.Minute);
                event1.Place = DataContext.Places.Find(model.PlaceId);
                var id = User.Identity.GetUserId();

                event1.DanceStyles = new List<DanceStyle>();
                var styles = DataContext.DanceStyles.Where(x => model.PostedStyles.DanceStyleIds.Contains(x.Id.ToString())).ToList();

                event1.DanceStyles.Clear();

                foreach (DanceStyle s in styles)
                {
                    event1.DanceStyles.Add(s);
                }

                if (model.EventType == "Class")
                {
                    var class1 = ConvertToClass(event1);
                    if (model.Role == "Teacher")
                    {
                        var teacher = DataContext.Teachers.Include("ApplicationUser").Where(x => x.ApplicationUser.Id == id).FirstOrDefault();
                        class1.Teachers.Add(teacher);
                    }
                    DataContext.Events.Add(class1);
                }
                if (model.EventType == "Workshop")
                {
                    var workshop1 = ConvertToWorkshop(event1);
                    if (model.Role == "Teacher")
                    {
                        var teacher = DataContext.Teachers.Include("ApplicationUser").Where(x => x.ApplicationUser.Id == id).FirstOrDefault();
                        workshop1.Teachers.Add(teacher);
                    }
                    DataContext.Events.Add(workshop1);
                }
                DataContext.SaveChanges();
                //promoter.ContactEmail = model.Promoter.ContactEmail;
                //promoter.Website = model.Promoter.Website;
                //promoter.Facebook = model.Promoter.Facebook;

                //DataContext.Entry(promoter).State = EntityState.Modified;
                //DataContext.SaveChanges();
                return RedirectToAction("Manage", "Account");
            }
            else
            {
                var allErrors = ModelState.Values.SelectMany(v => v.Errors);
            }
            LoadModel(model);
            return View(model);
        }

        public Class ConvertToClass(Event event1)
        {
            var class1 = new Class()
            {
                Name = event1.Name,
                Description = event1.Description,
                FacebookLink = event1.FacebookLink,
                StartDate = event1.StartDate,
                EndDate = event1.EndDate,
                Price = event1.Price,
                IsAvailable = event1.IsAvailable,
                Recurring = event1.Recurring,
                Frequency = event1.Frequency,
                Interval = event1.Interval,
                Day = event1.Day,
                Duration = event1.Duration,
                Place = event1.Place,
                Teachers = new List<Teacher>(),
                DanceStyles = event1.DanceStyles
            };

            return class1;
        }

        public Workshop ConvertToWorkshop(Event event1)
        {
            var workshop1 = new Workshop()
            {
                Name = event1.Name,
                Description = event1.Description,
                FacebookLink = event1.FacebookLink,
                StartDate = event1.StartDate,
                EndDate = event1.EndDate,
                Price = event1.Price,
                IsAvailable = event1.IsAvailable,
                Recurring = event1.Recurring,
                Frequency = event1.Frequency,
                Interval = event1.Interval,
                Day = event1.Day,
                Duration = event1.Duration,
                Place = event1.Place,
                Teachers = new List<Teacher>(),
                DanceStyles = event1.DanceStyles
            };

            return workshop1;
        }
    }
}
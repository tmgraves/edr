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

namespace EDR.Controllers
{
    public class EventController : BaseController
    {
        public ActionResult Reviews(int id)
        {
            var model = new EventReviewsViewModel();
            model.EventReviews = DataContext.Reviews.Where(x => x.Event.Id == id);
            model.EventId = id;
            model.NewReview = new Review();

            return View(model);
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

        public ActionResult Details(int id, EventType eventType)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var model = new EventDetailViewModel();
            model.Event = DataContext.Events.Include("DanceStyles").Include("Reviews").Where(x => x.Id == id).FirstOrDefault();
            model.EventType = eventType;

            if (model.Event == null)
            {
                return HttpNotFound();
            }

            if (model.Event is Class)
            {
                model.Teachers = DataContext.Teachers.Where(t => t.Classes.Any(c => c.Id == id)).ToList();
            }
            else if (model.Event is Workshop)
            {
                model.Teachers = DataContext.Teachers.Where(t => t.Workshops.Any(w => w.Id == id)).ToList();
            }

            return View(model);
        }

        public ActionResult Class(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var model = LoadEvent(id);
            model.Class = DataContext.Events.OfType<Class>().Where(x => x.Id == id).Include("Teachers").Include("Teachers.ApplicationUser").FirstOrDefault();

            if (model.Class == null)
            {
                return HttpNotFound();
            }

            return View(model);
        }

        private EventViewModel LoadEvent(int id)
        {
            var model = new EventViewModel();
            model.Event = DataContext.Events.Where(x => x.Id == id).Include("Place").Include("DanceStyles").Include("Reviews").Include("Users").FirstOrDefault();
            model.Review = new Review();
            model.Review.Like = true;
            var userid = User.Identity.GetUserId();
            model.Review.Author = DataContext.Users.Where(x => x.Id == userid).FirstOrDefault();
            return model;
        }
        public ActionResult Social(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var model = LoadEvent(id);
            model.Social = DataContext.Events.OfType<Social>().Where(x => x.Id == id).Include("Promoters").Include("Promoters.ApplicationUser").FirstOrDefault();

            if (model.Social == null)
            {
                return HttpNotFound();
            }

            return View(model);
        }

        public ActionResult Signup(int id, string returnUrl)
        {
            var userId = User.Identity.GetUserId();
            var user = DataContext.Users.Where(x => x.Id == userId).FirstOrDefault();
            var userevent = DataContext.Events.Include("Users").Where(x => x.Id == id).FirstOrDefault();
            if (!userevent.Users.Contains(user))
            {
                userevent.Users.Add(user);
            }
            if (userevent is Class)
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
        public ActionResult Create(string role, string eventType, int? placeId)
        {
            var model = GetInitialClassCreateViewModel(role, eventType);
            if (placeId != null)
            {
                model.PlaceId = (int)placeId;
            }
            return View(model);
        }

        private EventCreateViewModel GetInitialClassCreateViewModel(string role, string eventType)
        {
            var model = new EventCreateViewModel();
            var id = User.Identity.GetUserId();
            model.Role = role;
            model.EventType = eventType;

            LoadModel(model);

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
            else if (model.Role == "Promoter")
            {
                model.PlaceList = DataContext.Places.Where(x => x.Promoters.Any(t => t.ApplicationUser.Id == id)).Select(p => new SelectListItem() { Text = p.Name, Value = p.Id.ToString() }).ToList();
            }
        }

        [Authorize]
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
                else if (model.EventType == "Workshop")
                {
                    var workshop1 = ConvertToWorkshop(event1);
                    if (model.Role == "Teacher")
                    {
                        var teacher = DataContext.Teachers.Include("ApplicationUser").Where(x => x.ApplicationUser.Id == id).FirstOrDefault();
                        workshop1.Teachers.Add(teacher);
                    }
                    DataContext.Events.Add(workshop1);
                }
                else if (model.EventType == "Social")
                {
                    var social = ConvertToSocial(event1);
                    if (model.Role == "Promoter")
                    {
                        var promoter = DataContext.Promoters.Include("ApplicationUser").Where(x => x.ApplicationUser.Id == id).FirstOrDefault();
                        social.Promoters.Add(promoter);
                    }
                    DataContext.Events.Add(social);
                }
                else if (model.EventType == "Concert")
                {
                    var concert = ConvertToConcert(event1);
                    if (model.Role == "Promoter")
                    {
                        var promoter = DataContext.Promoters.Include("ApplicationUser").Where(x => x.ApplicationUser.Id == id).FirstOrDefault();
                        concert.Promoters.Add(promoter);
                    }
                    DataContext.Events.Add(concert);
                }
                else if (model.EventType == "Conference")
                {
                    var conference = ConvertToConference(event1);
                    if (model.Role == "Promoter")
                    {
                        var promoter = DataContext.Promoters.Include("ApplicationUser").Where(x => x.ApplicationUser.Id == id).FirstOrDefault();
                        conference.Promoters.Add(promoter);
                    }
                    DataContext.Events.Add(conference);
                }
                else if (model.EventType == "OpenHouse")
                {
                    var openHouse = ConvertToOpenHouse(event1);
                    if (model.Role == "Promoter")
                    {
                        var promoter = DataContext.Promoters.Include("ApplicationUser").Where(x => x.ApplicationUser.Id == id).FirstOrDefault();
                        openHouse.Promoters.Add(promoter);
                    }
                    DataContext.Events.Add(openHouse);
                }
                else if (model.EventType == "Party")
                {
                    event1.Place = new OtherPlace { Name = model.Name, Address = model.Address, Address2 = model.Address2, City = model.City, State = model.State, Zip = model.Zip };
                    var party = ConvertToParty(event1);
                    party.Dancer = DataContext.Users.Find(id);
                    DataContext.Events.Add(party);
                }
                else if (model.EventType == "Rehearsal")
                {
                    var rehearsal = ConvertToRehearsal(event1);
                    if (model.Role == "Teacher")
                    {
                        var teacher = DataContext.Teachers.Include("ApplicationUser").Where(x => x.ApplicationUser.Id == id).FirstOrDefault();
                        rehearsal.Teacher = teacher;
                    }
                    DataContext.Events.Add(rehearsal);
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

        [Authorize]
        public PartialViewResult PostReview(EventViewModel model)
        {
            var userid = User.Identity.GetUserId();
            if (DataContext.Reviews.Where(r => r.Event.Id == model.Event.Id && r.Author.Id == userid).Count() == 0)
            {
                var auth = DataContext.Users.Where(x => x.Id == userid).FirstOrDefault();
                var ev = DataContext.Events.Where(e => e.Id == model.Event.Id).Include("Reviews").FirstOrDefault();
                ev.Reviews.Add(new Review() { ReviewText = model.Review.ReviewText, ReviewDate = DateTime.Now, Like = model.Review.Like, Author = auth });
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
            var workshop = new Workshop()
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

            return workshop;
        }

        public Social ConvertToSocial(Event event1)
        {
            var social = new Social()
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
                DanceStyles = event1.DanceStyles,
                Promoters = new List<Promoter>()
            };

            return social;
        }

        public Concert ConvertToConcert(Event event1)
        {
            var concert = new Concert()
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
                DanceStyles = event1.DanceStyles,
                Promoters = new List<Promoter>()
            };

            return concert;
        }

        public Conference ConvertToConference(Event event1)
        {
            var conference = new Conference()
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
                DanceStyles = event1.DanceStyles,
                Promoters = new List<Promoter>()
            };

            return conference;
        }

        public OpenHouse ConvertToOpenHouse(Event event1)
        {
            var openHouse = new OpenHouse()
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
                DanceStyles = event1.DanceStyles,
                Promoters = new List<Promoter>()
            };

            return openHouse;
        }

        public Party ConvertToParty(Event event1)
        {
            var party = new Party()
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
                DanceStyles = event1.DanceStyles
            };

            return party;
        }

        public Rehearsal ConvertToRehearsal(Event event1)
        {
            var rehearsal = new Rehearsal()
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
                DanceStyles = event1.DanceStyles
            };

            return rehearsal;
        }
    }
}
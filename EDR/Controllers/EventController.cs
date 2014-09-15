using EDR.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using EDR.Models;

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

        [Authorize(Roles = "Teacher, Owner")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(EventCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                //var teacher = DataContext.Promoters.Where(x => x.ApplicationUser.Id == model.Promoter.ApplicationUser.Id).Include("ApplicationUser").FirstOrDefault();
                //promoter.ContactEmail = model.Promoter.ContactEmail;
                //promoter.Website = model.Promoter.Website;
                //promoter.Facebook = model.Promoter.Facebook;

                //DataContext.Entry(promoter).State = EntityState.Modified;
                //DataContext.SaveChanges();
                return RedirectToAction("Manage", "Account");
            }
            return View(model);
        }
    }
}
using EDR.Models;
using EDR.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using System.Data;
using EDR.Controllers;
using EDR.Models;
using EDR.Models.ViewModels;
using Microsoft.AspNet.Identity.EntityFramework;

namespace EDR.Controllers
{
    public class ClassController : BaseController
    {
        public ActionResult Details(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var model = DataContext.Events.OfType<Class>().Where(x => x.Id == id).FirstOrDefault();
            if (model == null)
            {
                return HttpNotFound();
            }

            model.DanceStyles = DataContext.DanceStyles.Where(c => c.Events.Any(e => e.Id == id)).ToList();
            model.Reviews = DataContext.Reviews.Where(c => c.Event.Id == id).OrderByDescending(x => x.ReviewDate).ToList();
            model.Teachers = DataContext.Teachers.Include("ApplicationUser").Where(t => t.Classes.Any(c => c.Id == id)).ToList();
            model.Users = DataContext.Users.Where(u => u.Events.Any(e => e.Id == id)).ToList();

            return View(model);
        }

        [Authorize(Roles = "Teacher, Owner")]
        public ActionResult Create()
        {
            var model = GetInitialClassCreateViewModel();
            return View(model);
        }

        private ClassCreateViewModel GetInitialClassCreateViewModel()
        {
            var model = new ClassCreateViewModel();
            var id = User.Identity.GetUserId();

            var selectedStyles = new List<DanceStyleListItem>();
            var styles = new List<DanceStyleListItem>();
            foreach (DanceStyle s in DataContext.DanceStyles)
            {
                styles.Add(new DanceStyleListItem { Id = s.Id, Name = s.Name });
            }
            model.AvailableStyles = styles.OrderBy(x => x.Name);

            model.PlaceList = new SelectList(DataContext.Places.Where(x => x.Teachers.Any(t => t.ApplicationUser.Id == id) || x.Owners.Any(t => t.ApplicationUser.Id == id)).OrderBy(x => x.Name), "Id", "Name", null);
            //  model.PlaceList = DataContext.Places.Where(x => x.Teachers.Any(t => t.ApplicationUser.Id == id) || x.Owners.Any(t => t.ApplicationUser.Id == id)).Select(p => new SelectListItem() { Text = p.Name, Value = p.Id.ToString() }).ToList();
    
            return model;
        }

        public ActionResult CreateReview(int id)
        {
            var viewModel = new EventReviewViewModel();
            viewModel.EventId = id;
            return View("CreateReview", viewModel);
        }

        // POST: DanceStyle/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateReview([Bind(Include = "EventId, Review")] EventReviewViewModel viewModel)
        {
            viewModel.Review.Event = DataContext.Events.Find(viewModel.EventId);
            viewModel.Review.ReviewDate = DateTime.Now;
            viewModel.Review.Author = DataContext.Users.Find(User.Identity.GetUserId());
            DataContext.Reviews.Add(viewModel.Review);
            DataContext.SaveChanges();
            return RedirectToAction("Details/" + viewModel.EventId.ToString());
        }
    }
}
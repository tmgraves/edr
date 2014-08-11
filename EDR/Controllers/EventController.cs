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
    }
}
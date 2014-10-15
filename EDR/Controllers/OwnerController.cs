using EDR.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using EDR.Models;
using System.Data.Entity;

namespace EDR.Controllers
{
    public class OwnerController : BaseController
    {
        public ActionResult List()
        {
            var model = new OwnerListViewModel();
            model.Owners = DataContext.Owners.Include("ApplicationUser");

            return View(model);
        }

        public ActionResult View(string username)
        {
            if (String.IsNullOrWhiteSpace(username))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var owner = DataContext.Owners
                .Where(x => x.ApplicationUser.UserName == username)
                .FirstOrDefault();

            if (owner == null)
            {
                return HttpNotFound();
            }

            // TODO: FILL MORE VIEWMODEL PROPERTIES (SEE PromoterViewModel)
            var viewModel = new OwnerViewViewModel();
            viewModel.Owner = DataContext.Owners.Where(x => x.ApplicationUser.UserName == username).Include("ApplicationUser").Include("Places").FirstOrDefault();
            viewModel.ConferenceCenters = viewModel.Owner.Places.OfType<ConferenceCenter>().ToList();
            viewModel.Hotels = viewModel.Owner.Places.OfType<Hotel>().ToList();
            viewModel.Nightclubs = viewModel.Owner.Places.OfType<Nightclub>().ToList();
            viewModel.OtherPlaces = viewModel.Owner.Places.OfType<OtherPlace>().ToList();
            viewModel.Restaurants = viewModel.Owner.Places.OfType<Restaurant>().ToList();
            viewModel.Studios = viewModel.Owner.Places.OfType<Studio>().ToList();
            viewModel.Theaters = viewModel.Owner.Places.OfType<Theater>().ToList();

            return View(viewModel);
        }

        [Authorize]
        public ActionResult Edit()
        {
            var id = User.Identity.GetUserId();
            var owner = DataContext.Owners
                .Where(x => x.ApplicationUser.Id == id)
                .Include("ApplicationUser")
                .FirstOrDefault();

            if (owner == null)
            {
                return HttpNotFound();
            }

            var viewModel = new OwnerEditViewModel();
            viewModel.Owner = owner;

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(OwnerEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                var owner = DataContext.Owners.Where(x => x.ApplicationUser.Id == model.Owner.ApplicationUser.Id).Include("ApplicationUser").FirstOrDefault();
                owner.ContactEmail = model.Owner.ContactEmail;
                owner.Website = model.Owner.Website;
                owner.Facebook = model.Owner.Facebook;

                DataContext.Entry(owner).State = EntityState.Modified;
                DataContext.SaveChanges();
                return RedirectToAction("Manage", "Account");
            }
            return View(model);
        }

        [Authorize]
        public ActionResult Apply()
        {
            // TODO: SAVE OWNER RECORD WITH 'Active=false' ONCE APPROVED SWITCH TO 'True'
            //       AND ADD USER TO 'Owner' ROLE

            var user = UserManager.FindByName(User.Identity.Name);

            var viewModel = new OwnerApplyViewModel();
            viewModel.Owner = DataContext.Owners.Where(x => x.ApplicationUser.Id == user.Id).FirstOrDefault();

            return View(viewModel);
        }

        // POST: Owner Apply
        [HttpPost]
        public ActionResult Apply(Owner owner)
        {
            DataContext.Owners.Add(new Owner { ApplicationUser = DataContext.Users.Find(User.Identity.GetUserId()) });
            DataContext.SaveChanges();
            return RedirectToAction("Manage", "Account");
        }
    }
}
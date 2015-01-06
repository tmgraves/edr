using EDR.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using System.Net;
using EDR.Models;
using System.Data.Entity;
using EDR.Utilities;

namespace EDR.Controllers
{
    public class PromoterController : BaseController
    {
        public ActionResult List()
        {
            var model = new PromoterListViewModel();
            model.Promoters = DataContext.Promoters.Include("ApplicationUser");

            return View(model);
        }

        private PromoterViewViewModel LoadPromoter(string username)
        {
            // TODO: FILL MORE VIEWMODEL PROPERTIES (SEE TeacherViewModel)
            var viewModel = new PromoterViewViewModel();
            viewModel.Promoter = DataContext.Promoters
                                    .Where(x => x.ApplicationUser.UserName == username)
                                    .Include("Events")
                                    .Include("Places")
                                    .Include("Events.DanceStyles")
                                    .FirstOrDefault();

            if (viewModel.Promoter.ApplicationUser.ZipCode != null)
            {
                viewModel.Address = Geolocation.ParseAddress(viewModel.Promoter.ApplicationUser.ZipCode);
            }
            else
            {
                viewModel.Address = Geolocation.ParseAddress("90065");
            }

            // TODO: FILL MORE VIEWMODEL PROPERTIES (SEE PromoterViewModel)
            viewModel.Socials = new List<Social>();
            viewModel.Socials = viewModel.Promoter.Events.OfType<Social>();
            viewModel.Concerts = new List<Concert>();
            viewModel.Conferences = new List<Conference>();
            viewModel.OpenHouses = new List<OpenHouse>();
            viewModel.Parties = new List<Party>();

            return viewModel;
        }

        [Authorize]
        public ActionResult View(string username)
        {
            if (String.IsNullOrWhiteSpace(username))
            {
                if (User != null)
                {
                    username = User.Identity.Name;
                }
                else
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
            }

            var promoter = DataContext.Promoters
                .Where(x => x.ApplicationUser.UserName == username)
                .FirstOrDefault();

            if (promoter == null)
            {
                if (username == User.Identity.Name && !User.IsInRole("Promoter"))
                {
                    return RedirectToAction("Apply", "Promoter");
                }
                else 
                {
                    return HttpNotFound();
                }
            }

            var viewModel = LoadPromoter(username);

            return View(viewModel);
        }

        [Authorize]
        public ActionResult MySocials(string username)
        {
            if (String.IsNullOrWhiteSpace(username))
            {
                if (User != null)
                {
                    username = User.Identity.Name;
                }
                else
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
            }

            var promoter = DataContext.Promoters
                .Where(x => x.ApplicationUser.UserName == username)
                .FirstOrDefault();

            if (promoter == null)
            {
                if (username == User.Identity.Name && !User.IsInRole("Promoter"))
                {
                    return RedirectToAction("Apply", "Promoter");
                }
                else
                {
                    return HttpNotFound();
                }
            }

            var viewModel = LoadPromoter(username);

            return View(viewModel);
        }

        [Authorize]
        public ActionResult Edit()
        {
            var id = User.Identity.GetUserId();
            var promoter = DataContext.Promoters
                .Where(x => x.ApplicationUser.Id == id)
                .FirstOrDefault();

            if (promoter == null)
            {
                return HttpNotFound();
            }

            var viewModel = new PromoterEditViewModel();
            viewModel.Promoter = promoter;

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(PromoterEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                var promoter = DataContext.Promoters.Where(x => x.ApplicationUser.Id == model.Promoter.ApplicationUser.Id).Include("ApplicationUser").FirstOrDefault();
                promoter.ContactEmail = model.Promoter.ContactEmail;
                promoter.Website = model.Promoter.Website;
                promoter.Facebook = model.Promoter.Facebook;

                DataContext.Entry(promoter).State = EntityState.Modified;
                DataContext.SaveChanges();
                return RedirectToAction("MySocials", "Promoter", new { username = promoter.ApplicationUser.UserName });
            }
            return View(model);
        }

        [Authorize]
        public ActionResult Apply()
        {
            // TODO: SAVE PROMOTER RECORD WITH 'Active=false' ONCE APPROVED SWITCH TO 'True'
            //       AND ADD USER TO 'Promoter' ROLE

            var user = UserManager.FindByName(User.Identity.Name);

            var viewModel = new PromoterApplyViewModel();
            viewModel.Promoter = DataContext.Promoters.Where(x => x.ApplicationUser.Id == user.Id).FirstOrDefault();

            return View(viewModel);
        }

        // POST: Promoter Apply
        [HttpPost]
        public ActionResult Apply(Promoter promoter)
        {
            DataContext.Promoters.Add(new Promoter { ApplicationUser = DataContext.Users.Find(User.Identity.GetUserId()) });
            DataContext.SaveChanges();
            return RedirectToAction("Manage", "Account");
        }
    }
}
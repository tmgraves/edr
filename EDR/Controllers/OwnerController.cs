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
using EDR.Utilities;
using EDR.Enums;

namespace EDR.Controllers
{
    public class OwnerController : BaseController
    {
        [Authorize(Roles = "Owner")]
        public ActionResult Manage()
        {
            var userid = User.Identity.GetUserId();
            var model = DataContext.Owners.Include("Schools").Single(o => o.ApplicationUser.Id == userid);

            return View(model);
        }

        public ActionResult List()
        {
            var model = new OwnerListViewModel();
            model.Owners = DataContext.Owners.Include("ApplicationUser");

            return View(model);
        }

        private OwnerViewViewModel LoadOwner(string username)
        {
            // TODO: FILL MORE VIEWMODEL PROPERTIES (SEE TeacherViewModel)
            var viewModel = new OwnerViewViewModel();
            viewModel.Owner = DataContext.Owners
                                .Where(x => x.ApplicationUser.UserName == username)
                                .Include("ApplicationUser")
                                .Include("Places")
                                .Include("ApplicationUser.Roles")
                                .FirstOrDefault();

            if (viewModel.Owner.ApplicationUser.ZipCode != null)
            {
                viewModel.Address = Geolocation.ParseAddress(viewModel.Owner.ApplicationUser.ZipCode);
            }
            else
            {
                viewModel.Address = Geolocation.ParseAddress("90065");
            }

            //  Load Roles
            viewModel.Roles = new List<RoleName>();
            if (UserManager.IsInRole(viewModel.Owner.ApplicationUser.Id, "Teacher"))
            {
                viewModel.Roles.Add(RoleName.Teacher);
            }
            if (UserManager.IsInRole(viewModel.Owner.ApplicationUser.Id, "Promoter"))
            {
                viewModel.Roles.Add(RoleName.Promoter);
            }
            //  Load Roles

            ////  Set Role
            //if (User.Identity.IsAuthenticated)
            //{
            //    Session["MyRole"] = RoleName.Owner;
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

        //    var owner = DataContext.Owners
        //        .Where(x => x.ApplicationUser.UserName == username)
        //        .FirstOrDefault();

        //    if (owner == null || owner.Approved == null)
        //    {
        //        if (username == User.Identity.Name && !User.IsInRole("Owner"))
        //        {
        //            return RedirectToAction("Apply", "Owner");
        //        }
        //        else
        //        {
        //            return HttpNotFound();
        //        }
        //    }

        //    var viewModel = LoadOwner(username);

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

            var owner = DataContext.Owners
                .Where(x => x.ApplicationUser.UserName == username)
                .FirstOrDefault();

            if (owner == null || owner.Approved == null)
            {
                if (username == User.Identity.Name && !User.IsInRole("Owner"))
                {
                    return RedirectToAction("Apply", "Owner");
                }
                else
                {
                    return HttpNotFound();
                }
            }

            var viewModel = LoadOwner(username);

            //  Load Events
            viewModel.NewClasses = DataContext.Events.OfType<Class>().Where(c => c.Place.Owners.Any(o => o.ApplicationUser.UserName == username)).Include("Place").OrderBy(c => c.StartDate);
            viewModel.NewSocials = DataContext.Events.OfType<Social>().Where(c => c.Place.Owners.Any(o => o.ApplicationUser.UserName == username)).Include("Place").OrderBy(c => c.StartDate);
            viewModel.NewClasses = viewModel.NewClasses.Where(e => e.NextDate >= DateTime.Today).Take(5);
            viewModel.NewSocials = viewModel.NewSocials.Where(e => e.NextDate >= DateTime.Today).Take(5);
            //  Load Events

            //  Load Users
            viewModel.Teachers = DataContext.Teachers.Where(t => t.Classes.Any(c => c.Place.Owners.Any(o => o.ApplicationUser.UserName == username))).Include("ApplicationUser").Include("ApplicationUser.UserPictures");
            viewModel.Dancers = DataContext.Users.Where(u => u.EventMembers.Any(m => m.Event.Place.Owners.Any(o => o.ApplicationUser.UserName == username))).Include("UserPictures");
            //  Load Users
            
            return View(viewModel);
        }

        [Authorize]
        public ActionResult MyPlaces(string username)
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

            var owner = DataContext.Owners
                .Where(x => x.ApplicationUser.UserName == username)
                .FirstOrDefault();

            if (owner == null || owner.Approved == null)
            {
                if (username == User.Identity.Name && !User.IsInRole("Owner"))
                {
                    return RedirectToAction("Apply", "Owner");
                }
                else
                {
                    return HttpNotFound();
                }
            }

            var viewModel = LoadOwner(username);

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
                return RedirectToAction("MyPlaces", "Owner", new { username = owner.ApplicationUser.UserName } );
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
            return RedirectToAction("Home", "Dancer", new { username = User.Identity.Name });
        }

        public ActionResult GetUpdates(string username)
        {
            var evt = DataContext.Events.Where(e => e.Place.Owners.Any(o => o.ApplicationUser.UserName == username))
                    .Include("Creator")
                    .Include("Pictures")
                    .Include("Pictures.PostedBy")
                    .Include("Videos")
                    .Include("Videos.Author")
                    .Include("Playlists")
                    .Include("Playlists.Author")
                    .Include("LinkedFacebookObjects");

            var lstMedia = EventHelper.BuildAllUpdates(evt, MediaTarget.User);

            return PartialView("~/Views/Shared/_MediaUpdatesPartial.cshtml", lstMedia);
            //  Media Updates

        }
    }
}
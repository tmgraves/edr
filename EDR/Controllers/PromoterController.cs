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
using EDR.Enums;

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
                                    .Include("Socials")
                                    .Include("Places")
                                    .Include("Socials.Place")
                                    .Include("Socials.DanceStyles")
                                    .Include("Socials.Videos")
                                    .Include("Socials.Pictures")
                                    .Include("Socials.Users")
                                    .Include("Socials.Reviews")
                                    .Include("ApplicationUser")
                                    .Include("ApplicationUser.Roles")
                                    .FirstOrDefault();
            viewModel.Events = new EventListViewModel();

            if (viewModel.Promoter.ApplicationUser.ZipCode != null)
            {
                viewModel.Address = Geolocation.ParseAddress(viewModel.Promoter.ApplicationUser.ZipCode);
                viewModel.Events.Location = viewModel.Address;
            }
            else
            {
                viewModel.Address = Geolocation.ParseAddress("90065");
                viewModel.Events.Location = viewModel.Address;
            }

            //  Load Roles
            viewModel.Roles = new List<RoleName>();
            if (UserManager.IsInRole(viewModel.Promoter.ApplicationUser.Id, "Teacher"))
            {
                viewModel.Roles.Add(RoleName.Teacher);
            }
            if (UserManager.IsInRole(viewModel.Promoter.ApplicationUser.Id, "Owner"))
            {
                viewModel.Roles.Add(RoleName.Owner);
            }
            //  Load Roles

            ////  Set Role
            //if (User.Identity.IsAuthenticated)
            //{
            //    Session["MyRole"] = RoleName.Promoter;
            //}
            ////  Set Role

            // TODO: FILL MORE VIEWMODEL PROPERTIES (SEE PromoterViewModel)
            viewModel.Events.EventType = Enums.EventType.Social;
            viewModel.Events.Events = new List<Event>();
            viewModel.Events.Events = viewModel.Promoter.Socials;

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

            if (promoter == null || promoter.Approved == null)
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
        public ActionResult Home(string username)
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

            if (promoter == null || promoter.Approved == null)
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

            ////  Media Updates
            //var lstMedia = new List<EventMedia>();
            //var events = DataContext.Events.OfType<Social>().Where(e => e.Promoters.Any(t => t.Id == viewModel.Promoter.Id));
            //var newPictures = DataContext.Pictures.OfType<EventPicture>()
            //                    .Include("Event")
            //                    .Include("PostedBy")
            //                    .Where(p => events.Any(e => e.Id == p.Event.Id))
            //                    .OrderByDescending(p => p.PhotoDate)
            //                    .Take(20);
            //foreach (var p in newPictures)
            //{
            //    lstMedia.Add(new EventMedia() { Event = p.Event, Id = p.Id, Author = p.PostedBy, MediaDate = p.PhotoDate, MediaType = Enums.MediaType.Picture, PhotoUrl = p.Filename, Title = p.Title });
            //}
            //var newVideos = DataContext.Videos.OfType<EventVideo>()
            //                    .Include("Event")
            //                    .Include("Author")
            //                    .Where(v => events.Any(e => e.Id == v.Event.Id))
            //                    .OrderByDescending(v => v.PublishDate)
            //                    .Take(20);
            //foreach (var v in newVideos)
            //{
            //    lstMedia.Add(new EventMedia() { Event = v.Event, Id = v.Id, Author = v.Author, MediaDate = v.PublishDate, MediaType = Enums.MediaType.Video, PhotoUrl = v.PhotoUrl, MediaUrl = v.VideoUrl, Title = v.Title });
            //}

            //viewModel.MediaUpdates = lstMedia;
            ////  Media Updates

            viewModel.NewSocials = new EventListViewModel();
            viewModel.NewSocials.EventType = Enums.EventType.Social;
            viewModel.NewSocials.Location = viewModel.Address;
            viewModel.NewSocials.Events = new List<Event>();
            viewModel.NewSocials.Events = viewModel.Promoter.Socials.Where(s => s.NextDate >= DateTime.Today).OrderByDescending(e => e.NextDate).Take(5);

            viewModel.NewDancers = new List<ApplicationUser>();
            var socialArray = viewModel.Promoter.Socials.Select(c => c.Id).ToArray();
            viewModel.NewDancers = DataContext.Users.Include("EventMembers").Where(u => u.EventMembers.Any(m => socialArray.Contains(m.Event.Id)));

            //  Load Facebook Events
            if (User.Identity.IsAuthenticated)
            {
                var userid = User.Identity.GetUserId();
                var user = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();

                if (user.FacebookToken != null)
                {
                    viewModel.FacebookEvents = FacebookHelper.GetEvents(user.FacebookToken, DateTime.Now).Where(fe => !DataContext.Events.Select(e => e.FacebookId).Contains(fe.Id)).ToList();
                }
                //  Load Facebook Events
            }

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

            if (promoter == null || promoter.Approved == null)
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
            return RedirectToAction("Home", "Dancer", new { username = User.Identity.Name });
        }

        public ActionResult GetUpdates(string username)
        {
            var evts = DataContext.Events.OfType<Social>().Where(s => s.Promoters.Any(p => p.ApplicationUser.UserName == username))
                    .Include("Creator")
                    .Include("Pictures")
                    .Include("Pictures.PostedBy")
                    .Include("Videos")
                    .Include("Videos.Author")
                    .Include("Playlists")
                    .Include("Playlists.Author")
                    .Include("LinkedFacebookObjects")
                    .Cast<Event>();

            var lstMedia = EventHelper.BuildAllUpdates(evts, MediaTarget.User);

            return PartialView("~/Views/Shared/_MediaUpdatesPartial.cshtml", lstMedia);
            //  Media Updates

        }
    }
}
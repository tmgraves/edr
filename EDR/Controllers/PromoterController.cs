﻿using EDR.Models.ViewModels;
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
using EDR.Attributes;

namespace EDR.Controllers
{
    public class PromoterController : BaseController
    {
        [Route("Promoter/Manage")]
        [AccessDeniedAuthorize(Roles = "Promoter", AccessDeniedAction = "Apply", AccessDeniedController = "Promoter")]
        public ActionResult Manage()
        {
            var userid = User.Identity.GetUserId();
            var model = new PromoterManageViewModel();
            model.Promoter = DataContext.Promoters
                                .Include("PromoterGroups")
                                .Single(s => s.ApplicationUser.Id == userid);

            return View(model);
        }

        [Route("Promoter/Manage")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Promoter")]
        public ActionResult Manage(PromoterManageViewModel model)
        {
            if (ModelState.IsValidField("Promoter"))
            {
                var promoter = DataContext.Promoters.Include("ApplicationUser").Single(d => d.Id == model.Promoter.Id);
                TryUpdateModel(promoter);
                DataContext.Entry(promoter).State = EntityState.Modified;
                DataContext.SaveChanges();
                return RedirectToAction("Manage");
            }
            return View(model);
        }

        [Route("Promoter/ManageGroup")]
        [Authorize]
        public ActionResult ManageGroup(int id)
        {
            var userid = User.Identity.GetUserId();
            var group = DataContext.PromoterGroups
                                    .Include("Promoters")
                                    .Include("Socials.EventInstances.EventRegistrations")
                                    .Include("Socials.Tickets.UserTickets")
                                    .Include("Members")
                                .Where(g => g.Id == id).FirstOrDefault();

            if (group.Promoters.Where(p => p.ApplicationUser.Id == userid).Count() > 0 || group.Members.Where(p => p.UserId == userid && p.Admin).Count() > 0 || User.IsInRole("Admin"))
            {
                var model = new PromoterGroupManageViewModel();
                var admin = User.IsInRole("Admin");
                model.PromoterGroup = group;
                return View(model);
            }
            else
            {
                ViewBag.Message = "You're not authorized";
                return View("Error");
            }
        }

        [Route("Promoter/ManageGroup")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult ManageGroup(PromoterGroupManageViewModel model)
        {
            var userid = User.Identity.GetUserId();
            var group = DataContext.PromoterGroups.Include("Promoters").Include("Members").Where(g => g.Id == model.PromoterGroup.Id).FirstOrDefault();

            if (group.Promoters.Where(p => p.ApplicationUser.Id == userid).Count() > 0 || group.Members.Where(p => p.UserId == userid && p.Admin).Count() > 0 || User.IsInRole("Admin"))
            {
                if (ModelState.IsValidField("PromoterGroup"))
                {
                    group.Name = model.PromoterGroup.Name;
                    group.PayeeAddress = model.PromoterGroup.PayeeAddress;
                    group.PayeeCity = model.PromoterGroup.PayeeCity;
                    group.PayeeState = model.PromoterGroup.PayeeState;
                    group.PayeeZip = model.PromoterGroup.PayeeZip;
                    group.BankName = model.PromoterGroup.BankName;
                    group.BankAccount = model.PromoterGroup.BankAccount;
                    group.RoutingNumber = model.PromoterGroup.RoutingNumber;
                    group.PayeeName = model.PromoterGroup.PayeeName;

                    //  UpdateModel(group, "PromoterGroup");
                    DataContext.Entry(group).State = EntityState.Modified;
                    DataContext.SaveChanges();
                    return RedirectToAction("ManageGroup", new { id = model.PromoterGroup.Id });
                }
                return RedirectToAction("ManageGroup", new { id = model.PromoterGroup.Id });
            }
            else
            {
                ViewBag.Message = "You're not authorized";
                return View("Error");
            }
        }

        [Route("Promoter/AddMember")]
        [Authorize(Roles = "Teacher,Owner")]
        [HttpPost]
        public ActionResult AddMember(PromoterGroupManageViewModel model)
        {
            var userid = User.Identity.GetUserId();
            var group = DataContext.PromoterGroups.Include("Promoters").Include("Members").Where(g => g.Id == model.PromoterGroup.Id).FirstOrDefault();

            if (group.Promoters.Where(p => p.ApplicationUser.Id == userid).Count() > 0 || group.Members.Where(p => p.UserId == userid && p.Admin).Count() > 0 || User.IsInRole("Admin"))
            {
                if (DataContext.OrganizationMembers.Where(m => m.OrganizationId == model.PromoterGroup.Id && m.UserId == model.NewMemberId).Count() == 0)
                {
                    DataContext.OrganizationMembers.Add(new OrganizationMember() { OrganizationId = model.PromoterGroup.Id, UserId = model.NewMemberId, Admin = false });
                    DataContext.SaveChanges();
                }
                return RedirectToAction("ManageGroup", new { id = model.PromoterGroup.Id });
            }
            else
            {
                ViewBag.Message = "Not Authorized";
                return View("Error");
            }
        }

        [Route("Promoter/UpdateMembers")]
        // POST: School
        [HttpPost]
        [Authorize(Roles = "Teacher,Owner")]
        public ActionResult UpdateMembers(PromoterGroupManageViewModel model)
        {
            var userid = User.Identity.GetUserId();
            var group = DataContext.PromoterGroups.Include("Promoters").Include("Members").Where(g => g.Id == model.PromoterGroup.Id).FirstOrDefault();

            if (group.Promoters.Where(p => p.ApplicationUser.Id == userid).Count() > 0 || group.Members.Where(p => p.UserId == userid && p.Admin).Count() > 0 || User.IsInRole("Admin"))
            {
                foreach (var m in model.PromoterGroup.Members)
                {
                    var mem = DataContext.OrganizationMembers.Where(om => om.Id == m.Id).FirstOrDefault();
                    mem.Admin = m.Admin;
                }
                DataContext.SaveChanges();
                return RedirectToAction("ManageGroup", new { id = model.PromoterGroup.Id });
            }
            else
            {
                ViewBag.Message = "Not Authorized";
                return View("Error");
            }
        }

        [Route("Promoter/RemoveMember")]
        [Authorize]
        public ActionResult RemoveMember(int id, string memberid)
        {
            var userid = User.Identity.GetUserId();
            var group = DataContext.PromoterGroups.Include("Promoters").Include("Members").Where(g => g.Id == id).FirstOrDefault();

            if (group.Promoters.Where(p => p.ApplicationUser.Id == userid).Count() > 0 || group.Members.Where(p => p.UserId == userid && p.Admin).Count() > 0 || User.IsInRole("Admin"))
            {
                DataContext.OrganizationMembers.Remove(DataContext.OrganizationMembers.Single(m => m.OrganizationId == id && m.UserId == memberid));
                DataContext.SaveChanges();
                return RedirectToAction("ManageGroup", new { id = id });
            }
            else
            {
                ViewBag.Message = "Not Authorized";
                return View("Error");
            }
        }

        [Route("Promoter/AddGroupPromoter")]
        [Authorize(Roles = "Promoter,Owner,Admin")]
        [HttpPost]
        public ActionResult AddGroupPromoter(FormCollection formCollection)
        {
            var userid = User.Identity.GetUserId();
            int id = Convert.ToInt32(formCollection["id"]);
            string promid = formCollection["promoterid"];

            var group = DataContext.PromoterGroups.Include("Promoters").Include("Members").Where(g => g.Id == id).FirstOrDefault();

            if (group.Promoters.Where(p => p.ApplicationUser.Id == userid).Count() > 0 || group.Members.Where(p => p.UserId == userid && p.Admin).Count() > 0 || User.IsInRole("Admin"))
            {
                if (DataContext.PromoterGroups.Where(c => c.Id == id && c.Promoters.Any(t => t.ApplicationUser.Id == promid)).Count() == 0)
                {
                    DataContext.PromoterGroups.Single(s => s.Id == id).Promoters.Add(DataContext.Promoters.Single(t => t.ApplicationUser.Id == promid));
                    DataContext.SaveChanges();
                }
                return RedirectToAction("ManageGroup", new { id = id });
            }
            else
            {
                ViewBag.Message = "Not Authorized";
                return View("Error");
            }
        }

        [Route("Promoter/RemoveGroupPromoter")]
        [Authorize]
        public ActionResult RemoveGroupPromoter(int id, int promoterid)
        {
            var userid = User.Identity.GetUserId();

            var group = DataContext.PromoterGroups.Include("Promoters").Include("Members").Where(g => g.Id == id).FirstOrDefault();
            if (group.Promoters.Where(p => p.ApplicationUser.Id == userid).Count() > 0 || group.Members.Where(p => p.UserId == userid && p.Admin).Count() > 0 || User.IsInRole("Admin"))
            {
                DataContext.PromoterGroups.Where(s => s.Id == id).Include("Promoters").FirstOrDefault().Promoters.Remove(DataContext.Promoters.Single(t => t.Id == promoterid));
                DataContext.SaveChanges();
                return RedirectToAction("ManageGroup", new { id = id });
            }
            else
            {
                ViewBag.Message = "Not Authorized";
                return View("Error");
            }
        }

        [Route("Promoter/AddGroup")]
        [HttpPost]
        [Authorize(Roles = "Promoter")]
        public ActionResult AddGroup(PromoterManageViewModel model)
        {
            if (ModelState.IsValidField("NewPromoterGroup"))
            {
                try
                {
                    model.NewPromoterGroup.Promoters = new List<Promoter>() { DataContext.Promoters.Single(p => p.Id == model.Promoter.Id) };
                    DataContext.PromoterGroups.Add(model.NewPromoterGroup);
                    DataContext.SaveChanges();
                    return RedirectToAction("Manage");
                }
                catch(Exception ex)
                {
                    return RedirectToAction("View", model.Promoter.Id);
                }
            }
            return RedirectToAction("View", model.Promoter.Id);
        }

        [Route("Promoter/DeleteGroup")]
        // GET: School
        [Authorize(Roles = "Teacher")]
        public ActionResult DeleteGroup(int id)
        {
            var userid = User.Identity.GetUserId();

            var group = DataContext.PromoterGroups.Include("Promoters").Include("Members").Where(g => g.Id == id).FirstOrDefault();
            if (group.Promoters.Where(p => p.ApplicationUser.Id == userid).Count() > 0 || group.Members.Where(p => p.UserId == userid && p.Admin).Count() > 0 || User.IsInRole("Admin"))
            {
                group.Promoters.Clear();
                DataContext.PromoterGroups.Remove(group);
                DataContext.SaveChanges();
                return RedirectToAction("Manage");
            }
            else
            {
                ViewBag.Message = "Not Authorized";
                return View("Error");
            }
        }

        [Route("Promoters/{Location?}")]
        public ActionResult List(PromoterListViewModel model)
        {
            model.DanceStyles = DataContext.DanceStyles.Select(s => s.Name).ToArray();
            model.Promoters = DataContext.Promoters
                                    .Include("DanceStyles")
                                    .Include("ApplicationUser");

            if (model.Location != null)
            {
                var add = Geolocation.ParseAddress(model.Location);
                model.Promoters = model.Promoters.Where(e => (e.ApplicationUser.Longitude >= add.Longitude - .5 && e.ApplicationUser.Longitude <= add.Longitude + .5) && (e.ApplicationUser.Latitude >= add.Latitude - .5 && e.ApplicationUser.Latitude <= add.Latitude + .5)).ToList();
            }
            if (model.PromoterId != null)
            {
                model.Promoters = model.Promoters.Where(t => t.ApplicationUser.Id == model.PromoterId);
            }
            else if (model.PromoterName != null)
            {
                model.Promoters = model.Promoters.Where(t => t.ApplicationUser.FullName.ToLower().Contains(model.PromoterName.ToLower()));
            }
            if (model.DanceStyleId != null)
            {
                model.Promoters = model.Promoters.Where(t => t.DanceStyles.Select(st => st.Id).Contains((int)model.DanceStyleId));
            }
            if (model.NELat != null && model.SWLng != null)
            {
                model.Promoters = model.Promoters.Where(c => c.ApplicationUser.Longitude >= model.SWLng && c.ApplicationUser.Longitude <= model.NELng && c.ApplicationUser.Latitude >= model.SWLat && c.ApplicationUser.Latitude <= model.NELat);
            }

            model.Promoters = model.Promoters.ToList().Take(100);
            if (HttpContext.Request.Browser.IsMobileDevice)
            {
                return View("Mobile/List", model);
            }
            else
            {
                return View(model);
            }
        }

        [Route("Promoter/LoadPromoter")]
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

            //if (viewModel.Promoter.ApplicationUser.ZipCode != null && viewModel.Promoter.ApplicationUser.Location == null)
            //{
            //    viewModel.Address = Geolocation.ParseAddress(viewModel.Promoter.ApplicationUser.ZipCode);
            //    viewModel.Events.Location = viewModel.Address;
            //}
            //else
            //{
            //    viewModel.Address = Geolocation.ParseAddress("90065");
            //    viewModel.Events.Location = viewModel.Address;
            //}

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

        [Route("Promoter/GetSocialsPartial")]
        [HttpGet]
        public virtual ActionResult GetSocialsPartial(int id)
        {
            var start = DateTime.Today;
            var socials = DataContext.Socials
                                .Include("DanceStyles")
                                .Include("Place")
                                .Include("EventInstances.EventRegistrations")
                                .Include("Reviews")
                                .Where(c => c.EventInstances.Any(i => i.DateTime >= start)
                                        && c.Promoters.Any(p => p.Id == id));
            return PartialView("~/Views/Shared/_EventsPartial.cshtml", socials);
        }

        //[Route("Promoter/View")]
        //[Authorize]
        //public ActionResult View(string username)
        //{
        //    if (String.IsNullOrWhiteSpace(username))
        //    {
        //        if (User != null)
        //        {
        //            username = User.Identity.Name;
        //        }
        //        else
        //        {
        //            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //        }
        //    }

        //    var promoter = DataContext.Promoters
        //        .Where(x => x.ApplicationUser.UserName == username)
        //        .FirstOrDefault();

        //    if (promoter == null || promoter.Approved == null)
        //    {
        //        if (username == User.Identity.Name && !User.IsInRole("Promoter"))
        //        {
        //            return RedirectToAction("Apply", "Promoter");
        //        }
        //        else 
        //        {
        //            return HttpNotFound();
        //        }
        //    }

        //    var viewModel = LoadPromoter(username);

        //    return View(viewModel);
        //}

        [Route("Promoter/{username}")]
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

        [Route("Promoter/GetSocialInstances")]
        public JsonResult GetSocialInstances(int promoterId, DateTime start, DateTime end)
        {
            var instances = DataContext.Socials.Where(c => c.Promoters.Any(t => t.Id == promoterId)).SelectMany(c => c.EventInstances).Where(i => i.DateTime >= start && i.DateTime <= end).ToList();

            return Json(instances.AsEnumerable().Select(s =>
                        new {
                            id = s.EventId,
                            title = s.Event.Name,
                            start = s.StartTime.Value.ToString("o"),
                            end = s.EndTime.Value.ToString("o"),
                            lat = s.Event.Place.Latitude,
                            lng = s.Event.Place.Longitude,
                            color = "#006A90",
                            url = Url.Action("Social", "Event", new { id = s.EventId })
                        }), JsonRequestBehavior.AllowGet);
        }

        //[Route("Promoter/MySocials")]
        //[Authorize]
        //public ActionResult MySocials(string username)
        //{
        //    if (String.IsNullOrWhiteSpace(username))
        //    {
        //        if (User != null)
        //        {
        //            username = User.Identity.Name;
        //        }
        //        else
        //        {
        //            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //        }
        //    }

        //    var promoter = DataContext.Promoters
        //        .Where(x => x.ApplicationUser.UserName == username)
        //        .FirstOrDefault();

        //    if (promoter == null || promoter.Approved == null)
        //    {
        //        if (username == User.Identity.Name && !User.IsInRole("Promoter"))
        //        {
        //            return RedirectToAction("Apply", "Promoter");
        //        }
        //        else
        //        {
        //            return HttpNotFound();
        //        }
        //    }

        //    var viewModel = LoadPromoter(username);

        //    return View(viewModel);
        //}

        //[Authorize]
        //public ActionResult Edit()
        //{
        //    var id = User.Identity.GetUserId();
        //    var promoter = DataContext.Promoters
        //        .Where(x => x.ApplicationUser.Id == id)
        //        .FirstOrDefault();

        //    if (promoter == null)
        //    {
        //        return HttpNotFound();
        //    }

        //    var viewModel = new PromoterEditViewModel();
        //    viewModel.Promoter = promoter;

        //    return View(viewModel);
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit(PromoterEditViewModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var promoter = DataContext.Promoters.Where(x => x.ApplicationUser.Id == model.Promoter.ApplicationUser.Id).Include("ApplicationUser").FirstOrDefault();
        //        promoter.ContactEmail = model.Promoter.ContactEmail;
        //        promoter.Website = model.Promoter.Website;
        //        promoter.Facebook = model.Promoter.Facebook;

        //        DataContext.Entry(promoter).State = EntityState.Modified;
        //        DataContext.SaveChanges();
        //        return RedirectToAction("MySocials", "Promoter", new { username = promoter.ApplicationUser.UserName });
        //    }
        //    return View(model);
        //}

        [Route("Promoter/Apply")]
        [Authorize]
        public ActionResult Apply()
        {
            // TODO: SAVE PROMOTER RECORD WITH 'Active=false' ONCE APPROVED SWITCH TO 'True'
            //       AND ADD USER TO 'Promoter' ROLE

            var user = UserManager.FindByName(User.Identity.Name);

            var viewModel = new PromoterApplyViewModel();
            viewModel.Promoter = DataContext.Promoters.Where(x => x.ApplicationUser.Id == user.Id).FirstOrDefault();

            if (viewModel.Promoter == null)
            {
                viewModel.Promoter = new Promoter() { ApplicationUser = user };
            }

            return View(viewModel);
        }

        // POST: Promoter Apply
        [Route("Promoter/Apply")]
        [HttpPost]
        public ActionResult Apply(PromoterApplyViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (model.Promoter.Id == 0)
                    {
                        model.Promoter.ApplicationUser = DataContext.Users.Find(User.Identity.GetUserId());
                        DataContext.Promoters.Add(model.Promoter);
                    }
                    else
                    {
                        DataContext.Entry(model.Promoter).State = EntityState.Modified;
                    }
                    DataContext.SaveChanges();
                    return RedirectToAction("Home", "Dancer", new { username = User.Identity.Name });
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException e)
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
                    return View(model);
                }
            }
            else
            {
                return View(model);
            }

        }

        #region JSON
        [Route("Promoter/GetSocials")]
        [Authorize(Roles = "Teacher")]
        public JsonResult GetSocials()
        {
            if (User.Identity.IsAuthenticated)
            {
                var userid = User.Identity.GetUserId();
                var socials = DataContext.Events.OfType<Social>().Where(s => s.Promoters.Any(p => p.ApplicationUser.Id == userid) && s.EventInstances.Any(i => i.DateTime >= DateTime.Today)).Take(5);

                return Json(socials.Select(s => new { Name = s.Name, Id = s.Id }), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return null;
            }
        }

        [Route("Promoter/GetUpdates")]
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

        [Route("Promoter/Search")]
        public JsonResult Search(string searchString)
        {
            var promoters = DataContext.Promoters.Where(t => (t.ApplicationUser.FirstName + " " + t.ApplicationUser.LastName).ToLower().Contains(searchString.ToLower())).Select(s => new { Id = s.ApplicationUser.Id, Name = s.ApplicationUser.FirstName + " " + s.ApplicationUser.LastName }).ToList();
            return Json(promoters, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}
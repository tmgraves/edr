using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using EDR.Models;
using EDR.Models.ViewModels;
using EDR.Enums;
using Microsoft.AspNet.Identity;
using EDR.Utilities;
using System.Data.Entity;

namespace EDR.Controllers
{
    public class PlaceController : BaseController
    {
        public ActionResult List(PlaceListViewModel model)
        {
            model.DanceStyles = DataContext.DanceStyles.Select(s => s.Name).ToArray();
            if (model.TypeParam != null)
            {
                model.Type = model.TypeParam;
            }

            model.Places = DataContext.Places
                                .Include("Events")
                                .Include("Events.DanceStyles")
                                .Include("Events.Reviews")
                                .Where(p => p.Public)
                                .AsQueryable();
            if (model.Type != null)
            {
                model.Places = model.Places.Where(c => c.PlaceType == model.Type);
            }
            if (model.DanceStyleId != null)
            {
                model.Places = model.Places.Where(c => c.Events.Any(e => e.DanceStyles.Select(st => st.Id).Contains((int)model.DanceStyleId)));
            }
            if (model.TeacherId != null)
            {
                model.Places =
                        (from p in model.Places
                         join c in DataContext.Classes.Include("Place").Include("Teachers.ApplicationUser")
                         on p.Id equals c.Place.Id
                         where c.Teachers.Select(ts => ts.ApplicationUser.Id).Contains(model.TeacherId)
                         select p);
                //  model.Places = model.Places.Where(c => c.Events.OfType<Class>().Any(cl => cl.Teachers.Select(t => t.ApplicationUser.Id).Contains(model.TeacherId)));
            }
            if (model.NELat != null && model.SWLng != null)
            {
                model.Places = model.Places.Where(c => c.Longitude >= model.SWLng && c.Longitude <= model.NELng && c.Latitude >= model.SWLat && c.Latitude <= model.NELat);
            }

            model.Places = model.Places.ToList().Take(100);
            return View(model);
        }

        [Authorize(Roles="Owner")]
        public ActionResult ChangePicture(int id, string message)
        {
            var model = new ChangePlacePictureViewModel();
            model.Place = DataContext.Places.Where(p => p.Id == id).FirstOrDefault();
            var userid = User.Identity.GetUserId();
            var user = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();
            if (user.FacebookToken != null)
            {
                model.FacebookPictures = FacebookHelper.GetPhotos(user.FacebookToken);
            }

            return View(model);
        }

        [HttpPost]
        [Authorize]
        public ActionResult UploadPicture(int id, HttpPostedFileBase file)
        {
            UploadFile newFile = ApplicationUtility.LoadPicture(file);
            string message;

            if (newFile.UploadStatus == "Success")
            {
                var place = DataContext.Places.Where(p => p.Id == id).FirstOrDefault();
                place.Filename = newFile.FilePath;
                place.ThumbnailFilename = newFile.ThumbnailFilePath;
                DataContext.Entry(place).State = EntityState.Modified;
                DataContext.SaveChanges();
                return RedirectToAction("Details", "Place", new { id = id});
            }
            else
            {
                message = newFile.UploadStatus;
                return RedirectToAction("ChangePicture", "Place", new { id = id, message = message });
            }
        }

        [Authorize]
        public ActionResult SetFacebookPicture(int id, string largeSource, string source)
        {
            try
            {
                var place = DataContext.Places.Where(p => p.Id == id).FirstOrDefault();
                place.Filename = largeSource;
                place.ThumbnailFilename = source;
                DataContext.Entry(place).State = EntityState.Modified;
                DataContext.SaveChanges();
                return RedirectToAction("Details", "Place", new { id = id });
            }
            catch (Exception ex)
            {
                return RedirectToAction("Details", "Place", new { id = id });
            }
        }

        [HttpGet]
        public virtual ActionResult GetStylesPartial(int id)
        {
            var styles = DataContext.Classes.Where(c => c.PlaceId == id).SelectMany(e => e.DanceStyles).Distinct().OrderBy(s => s.Name).ToList();
            return PartialView("~/Views/Shared/DisplayTemplates/DanceStyleLabels.cshtml", styles);
        }

        [HttpGet]
        public virtual ActionResult GetTeachersPartial(int id)
        {
            var teachers = DataContext.Teachers.Where(t => t.Classes.Any(s => s.PlaceId == id)).ToList();
            return PartialView("~/Views/Shared/DisplayTemplates/TeacherThumbLinks.cshtml", teachers);
        }

        [HttpGet]
        public virtual ActionResult GetDancersPartial(int id)
        {
            var dancers = DataContext.Events.Where(e => e.PlaceId == id).SelectMany(e => e.EventInstances.SelectMany(i => i.EventRegistrations).Select(r => r.User).Distinct()).ToList();
            return PartialView("~/Views/Shared/DisplayTemplates/DancerThumbLinks.cshtml", dancers);
        }

        [HttpGet]
        public virtual ActionResult GetPromotersPartial(int id)
        {
            var promoters = DataContext.Promoters.Where(p => p.Socials.Any(s => s.PlaceId == id)).ToList();
            return PartialView("~/Views/Shared/DisplayTemplates/PromoterThumbLinks.cshtml", promoters);
        }

        [HttpGet]
        public virtual ActionResult GetOwnersPartial(int id)
        {
            var owners = DataContext.Owners.Where(o => o.Classes.Any(c => c.PlaceId == id) || o.Socials.Any(s => s.PlaceId == id)).ToList();
            return PartialView("~/Views/Shared/DisplayTemplates/OwnerThumbLinks.cshtml", owners);
        }

        [HttpGet]
        public virtual ActionResult GetClassesPartial(int id)
        {
            var start = DateTime.Today;
            var classes = DataContext.Classes
                                .Include("DanceStyles")
                                .Include("Place")
                                .Include("EventInstances.EventRegistrations")
                                .Include("Reviews")
                                .Include("Teachers.ApplicationUser")
                                .Where(c => c.EventInstances.Any(i => i.DateTime >= start)
                                        && c.PlaceId == id);
            return PartialView("~/Views/Shared/_EventsPartial.cshtml", classes);
        }

        [HttpGet]
        public virtual ActionResult GetSocialsPartial(int id)
        {
            var start = DateTime.Today;
            var socials = DataContext.Socials
                                .Include("DanceStyles")
                                .Include("Place")
                                .Include("EventInstances.EventRegistrations")
                                .Include("Reviews")
                                .Where(s => s.EventInstances.Any(i => i.DateTime >= start)
                                        && s.PlaceId == id);
            return PartialView("~/Views/Shared/_EventsPartial.cshtml", socials);
        }

        [HttpGet]
        public virtual ActionResult GetSchoolsPartial(int id)
        {
            var schools = DataContext.Schools.Where(s => s.Classes.Any(c => c.PlaceId == id)).ToList();
            return PartialView("~/Views/Shared/DisplayTemplates/SchoolLinks.cshtml", schools);
        }

        [HttpGet]
        public virtual ActionResult GetTeamsPartial(int id)
        {
            var teams = DataContext.Teams.Where(t => t.Auditions.Any(a => a.PlaceId == id) || t.Performances.Any(p => p.PlaceId == id)).ToList();
            return PartialView("~/Views/Shared/DisplayTemplates/TeamLinks.cshtml", teams);
        }

        //private PlaceViewModel LoadPlace(int id)
        //{
        //    var viewModel = new PlaceViewModel();
        //    viewModel.Place = DataContext.Places.Include("Owners").Where(x => x.Id == id).FirstOrDefault();
        //    var date = DateTime.Now.AddDays(21);
        //    viewModel.Classes = DataContext.Events.Include("Teachers").Include("Teachers.ApplicationUser").Include("DanceStyles").Include("EventMembers.Member").Include("Reviews").OfType<Class>().Where(c => c.Place.Id == id).Where(x => x.IsAvailable == true).Where(y => !y.Recurring ? (y.StartDate >= DateTime.Now) : (y.StartDate <= date && (y.EndDate == null || y.EndDate >= DateTime.Now))).OrderBy(z => z.StartDate).ToList();
        //    var Events = DataContext.Events.Include("DanceStyles").Include("EventMembers.Member").Include("Reviews").Where(c => c.Place.Id == id).Where(x => x.IsAvailable == true).Where(y => !y.Recurring ? (y.StartDate >= DateTime.Now) : (y.StartDate <= date && (y.EndDate == null || y.EndDate >= DateTime.Now))).OrderBy(z => z.StartDate).ToList();
        //    viewModel.Socials = Events.OfType<Social>();
        //    return viewModel;
        //}

        //public ActionResult Home(int id, PlaceViewModel model)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }

        //    var viewModel = LoadPlace(id);
        //    return View(viewModel);
        //}

        //public ActionResult Socials(int id, PlaceViewModel model)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }

        //    var viewModel = LoadPlace(id);
        //    viewModel.Socials = DataContext.Events.OfType<Social>()
        //                            .Where(e => e.Place.Id == id)
        //                            .Include("Place")
        //                            .Include("Reviews")
        //                            .Include("DanceStyles")
        //                            .Include("Promoters")
        //                            .Include("Promoters.ApplicationUser");
        //    return View(viewModel);
        //}

        //public ActionResult Classes(int id, PlaceViewModel model)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }

        //    var viewModel = LoadPlace(id);
        //    viewModel.Classes = DataContext.Events.OfType<Class>()
        //                            .Where(e => e.Place.Id == id)
        //                            .Include("Place")
        //                            .Include("Reviews")
        //                            .Include("DanceStyles")
        //                            .Include("Teachers")
        //                            .Include("Teachers.ApplicationUser");
        //    return View(viewModel);
        //}

        public ActionResult Details(int id, PlaceViewModel model)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var viewModel = new PlaceViewModel();
            viewModel.Place = DataContext.Places
                                    .Include("Teachers")
                                    .Include("Owners")
                                    .Include("Events")
                                    .Where(x => x.Id == id).FirstOrDefault();
            //viewModel.ClassDaysOfWeek = new List<DayOfWeek>() { DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday };
            //viewModel.SocialDaysOfWeek = new List<DayOfWeek>() { DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday };
            //viewModel.ClassDays = new List<DayOfWeek>();
            //viewModel.SocialDays = new List<DayOfWeek>();
            //viewModel.DanceStyleList = DataContext.DanceStyles.Select(d => new SelectListItem() { Text = d.Name, Value = d.Id.ToString() }).ToList();
            ////  viewModel.Owners = DataContext.Owners.Where(y => y.Places.Any(x => x.Id == id)).ToList();
            //viewModel.TeacherList = DataContext.Teachers.Where(x => x.Classes.Any(c => c.Place.Id == id) || x.Workshops.Any(w => w.Place.Id == id)).Select(t => new SelectListItem() { Text = t.ApplicationUser.FirstName + " " + t.ApplicationUser.LastName, Value = t.ApplicationUser.Id.ToString() }).ToList();
            //var date = DateTime.Now.AddDays(21);
            //viewModel.Classes = DataContext.Classes
            //                        .Include("Teachers")
            //                        .Include("Teachers.ApplicationUser")
            //                        .Include("DanceStyles")
            //                        .Include("EventMembers.Member")
            //                        .Include("Reviews")
            //                        .Include("Creator")
            //                        .Include("Pictures")
            //                        .Include("Pictures.PostedBy")
            //                        .Include("Videos")
            //                        .Include("Videos.Author")
            //                        .Include("Playlists")
            //                        .Include("Playlists.Author")
            //                        .Include("LinkedFacebookObjects")
            //                        .Where(c => c.Place.Id == id)
            //                        .Where(x => x.IsAvailable == true)
            //                        .ToList();
            //viewModel.Socials = DataContext.Socials
            //                        .Include("Promoters")
            //                        .Include("Promoters.ApplicationUser")
            //                        .Include("DanceStyles")
            //                        .Include("EventMembers.Member")
            //                        .Include("Reviews")
            //                        .Include("Creator")
            //                        .Include("Pictures")
            //                        .Include("Pictures.PostedBy")
            //                        .Include("Videos")
            //                        .Include("Videos.Author")
            //                        .Include("Playlists")
            //                        .Include("Playlists.Author")
            //                        .Include("LinkedFacebookObjects")
            //                        .Where(c => c.Place.Id == id)
            //                        .Where(x => x.IsAvailable == true)
            //                        .ToList();

            ////  Get Current Facebook Picture/Video
            //if (viewModel.Place.FacebookId != null)
            //{
            //    var obj = FacebookHelper.GetData(FacebookHelper.GetGlobalToken(), viewModel.Place.FacebookId + "?fields=cover");
            //    if (obj != null && obj.cover != null)
            //    {
            //        viewModel.Place.Filename = obj.cover.source;
            //    }
            //}

            ////  viewModel.Workshops = DataContext.Events.Include("Teachers").Include("Teachers.ApplicationUser").Include("DanceStyles").Include("EventMembers.Member").OfType<Workshop>().Where(c => c.Place.Id == id).Where(x => x.IsAvailable == true).Where(y => !y.Recurring ? (y.StartDate >= DateTime.Now) : (y.StartDate <= date && (y.EndDate == null || y.EndDate >= DateTime.Now))).OrderBy(z => z.StartDate).ToList();
            //var Events = DataContext.Events.Include("DanceStyles").Include("EventMembers.Member").Include("Reviews").Where(c => c.Place.Id == id).Where(x => x.IsAvailable == true).Where(y => !y.Recurring ? (y.StartDate >= DateTime.Now) : (y.StartDate <= date && (y.EndDate == null || y.EndDate >= DateTime.Now))).OrderBy(z => z.StartDate).ToList();

            //if (model.DanceStyleId != null)
            //{
            //    viewModel.Classes = viewModel.Classes.Where(x => x.DanceStyles.Any(s => s.Id == model.DanceStyleId));
            //    //  viewModel.Workshops = viewModel.Workshops.Where(x => x.DanceStyles.Any(s => s.Id == model.DanceStyleId));
            //    Events = Events.Where(x => x.DanceStyles.Any(s => s.Id == model.DanceStyleId)).ToList();
            //}

            //if (model.TeacherId != null && model.TeacherId != "")
            //{
            //    viewModel.Classes = viewModel.Classes.Where(x => x.Teachers.Any(t => t.ApplicationUser.Id == model.TeacherId));
            //    //  viewModel.Workshops = viewModel.Workshops.Where(x => x.Teachers.Any(t => t.ApplicationUser.Id == model.TeacherId));
            //}

            //if (model.SkillLevel != null && model.SkillLevel != 0)
            //{
            //    viewModel.Classes = viewModel.Classes.Where(x => x.SkillLevel == (int)model.SkillLevel);
            //    //  viewModel.Workshops = viewModel.Workshops.Where(x => x.SkillLevel == (int)model.SkillLevel);
            //}

            //viewModel.Socials = Events.OfType<Social>();
            //viewModel.Concerts = Events.OfType<Concert>();
            //viewModel.Conferences = Events.OfType<Conference>();
            //viewModel.Parties = Events.OfType<Party>();
            //viewModel.OpenHouses = Events.OfType<OpenHouse>();

            return View(viewModel);
        }

        protected void UpdatePlace(Place place, string token)
        {
            //  Get Place from Facebook
            var fbplace = FacebookHelper.GetData(token, place.FacebookId);

            var placetype = new PlaceType();
            if (fbplace.category_list != null)
            {
                foreach (dynamic category in fbplace.category_list)
                {
                    string cat = category.name;
                    //  Search for Dance Instruction category
                    if (cat.Contains("Dance Instruction") || category.id == "203916779633178")
                    {
                        placetype = PlaceType.Studio;
                        break;
                    }
                    else if (cat.Contains("Dance Club") || category.id == "176139629103647")
                    {
                        placetype = PlaceType.Nightclub;
                        break;
                    }
                    else if (category.id == "273819889375819" || cat.Contains("Restaurant"))
                    {
                        placetype = PlaceType.Restaurant;
                        break;
                    }
                    else if (cat.Contains("Hotel") || category.id == "164243073639257")
                    {
                        placetype = PlaceType.Hotel;
                        break;
                    }
                    else if (cat.Contains("Meeting Room") || category.id == "210261102322291")
                    {
                        placetype = PlaceType.ConferenceCenter;
                        break;
                    }
                    else if (cat.Contains("Theater") || category.id == "173883042668223")
                    {
                        placetype = PlaceType.Theater;
                        break;
                    }
                    else
                    {
                        placetype = PlaceType.OtherPlace;
                    }
                }
            }

            place.Name = fbplace.name;
            place.Address = fbplace.location.street;
            place.City = fbplace.location.city;
            place.State = fbplace.location.state != null ? (State)Enum.Parse(typeof(State), fbplace.location.state) : State.CA;
            place.Zip = fbplace.location.zip;
            place.Country = fbplace.location.country;
            place.Latitude = fbplace.location.latitude;
            place.Longitude = fbplace.location.longitude;
            place.PlaceType = placetype;
            place.Public = true;
            place.Website = fbplace.website;
            place.FacebookLink = fbplace.link;
            place.Filename = fbplace.cover != null ? fbplace.cover.source : null;
            place.ThumbnailFilename = fbplace.cover != null ? fbplace.cover.source : null;
            place.Name = fbplace.name;

            DataContext.Entry(place).State = EntityState.Modified;
            DataContext.SaveChanges();
        }

        public PartialViewResult SearchClasses(int id, PlaceViewModel model, string[] classdays)
        {
            var viewModel = new PlaceEventSearchViewModel();

            var dayslist = new List<DayOfWeek>();
            if (classdays != null)
            {
                foreach (var s in classdays)
                {
                    dayslist.Add((DayOfWeek)Enum.Parse(typeof(DayOfWeek), s));
                }
            }

            var classes = DataContext.Events
                            .Include("Teachers")
                            .Include("Teachers.ApplicationUser")
                            .Include("DanceStyles")
                            .Include("EventMembers.Member")
                            .Include("Reviews")
                            .Include("Creator")
                            .Include("Pictures")
                            .Include("Pictures.PostedBy")
                            .Include("Videos")
                            .Include("Videos.Author")
                            .Include("Playlists")
                            .Include("Playlists.Author")
                            .Include("LinkedFacebookObjects")
                            .OfType<Class>().Where(c => c.Place.Id == id)
                                            .Where(x => x.IsAvailable == true)
                                            .Where(y => !y.Recurring ? (y.StartDate >= DateTime.Now) : (y.EndDate == null || y.EndDate >= DateTime.Now))
                                            .OrderBy(z => z.StartDate).ToList();
            if (model.DanceStyleId != null)
            {
                classes = classes.Where(x => x.DanceStyles.Any(s => s.Id == model.DanceStyleId)).ToList();
            }

            if (model.TeacherId != null && model.TeacherId != "")
            {
                classes = classes.Where(x => x.Teachers.Any(t => t.ApplicationUser.Id == model.TeacherId)).ToList();
            }

            if (model.SkillLevel != null && model.SkillLevel != 0)
            {
                classes = classes.Where(x => x.SkillLevel == model.SkillLevel).ToList();
            }

            if (classdays != null)
            {
                classes = classes.Where(x => dayslist.Contains(x.Day)).ToList();
            }

            viewModel.Events = classes;
            viewModel.MediaUpdates = EventHelper.BuildAllUpdates(classes, MediaTarget.Place);

            return PartialView("~/Views/Shared/_EventsNoMapPartial.cshtml", viewModel);
        }

        public PartialViewResult SearchSocials(int id, PlaceViewModel model, string[] socialdays)
        {
            var viewModel = new PlaceEventSearchViewModel();

            var dayslist = new List<DayOfWeek>();
            if (socialdays != null)
            {
                foreach (var s in socialdays)
                {
                    dayslist.Add((DayOfWeek)Enum.Parse(typeof(DayOfWeek), s));
                }
            }

            var socials = DataContext.Events
                                .Include("Promoters")
                                .Include("Promoters.ApplicationUser")
                                .Include("DanceStyles")
                                .Include("EventMembers.Member")
                                .Include("Reviews")
                                .Include("Creator")
                                .Include("Pictures")
                                .Include("Pictures.PostedBy")
                                .Include("Videos")
                                .Include("Videos.Author")
                                .Include("Playlists")
                                .Include("Playlists.Author")
                                .Include("LinkedFacebookObjects")
                                .OfType<Social>().Where(c => c.Place.Id == id)
                                                .Where(x => x.IsAvailable == true)
                                                .Where(y => !y.Recurring ? (y.StartDate >= DateTime.Now) : (y.EndDate == null || y.EndDate >= DateTime.Now))
                                                .OrderBy(z => z.StartDate).ToList();
            if (model.DanceStyleId != null)
            {
                socials = socials.Where(x => x.DanceStyles.Any(s => s.Id == model.DanceStyleId)).ToList();
            }

            if (socialdays != null)
            {
                socials = socials.Where(x => dayslist.Contains(x.Day)).ToList();
            }

            viewModel.Events = socials;
            viewModel.MediaUpdates = EventHelper.BuildAllUpdates(socials, MediaTarget.Place);
            return PartialView("~/Views/Shared/_EventsNoMapPartial.cshtml", viewModel);
        }

        [Authorize(Roles = "Owner")]
        public ActionResult Create(PlaceType placeType)
        {
            var model = new PlaceCreateViewModel();
            model.PlaceType = placeType;
            return View(model);
        }

        [Authorize(Roles = "Owner")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(PlaceCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var id = User.Identity.GetUserId();
                var owner = DataContext.Owners.Where(x => x.ApplicationUser.Id == id).FirstOrDefault();

                if (model.PlaceType == PlaceType.Studio)
                {
                    var place = new Studio() { Name = model.Name, Address = model.Address, Address2 = model.Address2, City = model.City, State = model.State, Zip = model.Zip, Owners = new List<Owner> { owner } };
                    DataContext.Places.Add(place);
                }
                else if (model.PlaceType == PlaceType.ConferenceCenter)
                {
                    var place = new ConferenceCenter() { Name = model.Name, Address = model.Address, Address2 = model.Address2, City = model.City, State = model.State, Zip = model.Zip, Owners = new List<Owner> { owner } };
                    DataContext.Places.Add(place);
                }
                else if (model.PlaceType == PlaceType.Hotel)
                {
                    var place = new Hotel() { Name = model.Name, Address = model.Address, Address2 = model.Address2, City = model.City, State = model.State, Zip = model.Zip, Owners = new List<Owner> { owner } };
                    DataContext.Places.Add(place);
                }
                else if (model.PlaceType == PlaceType.Nightclub)
                {
                    var place = new Nightclub() { Name = model.Name, Address = model.Address, Address2 = model.Address2, City = model.City, State = model.State, Zip = model.Zip, Owners = new List<Owner> { owner } };
                    DataContext.Places.Add(place);
                }
                else if (model.PlaceType == PlaceType.OtherPlace)
                {
                    var place = new OtherPlace() { Name = model.Name, Address = model.Address, Address2 = model.Address2, City = model.City, State = model.State, Zip = model.Zip, Owners = new List<Owner> { owner } };
                    DataContext.Places.Add(place);
                }
                else if (model.PlaceType == PlaceType.Restaurant)
                {
                    var place = new Restaurant() { Name = model.Name, Address = model.Address, Address2 = model.Address2, City = model.City, State = model.State, Zip = model.Zip, Owners = new List<Owner> { owner } };
                    DataContext.Places.Add(place);
                }
                else if (model.PlaceType == PlaceType.Theater)
                {
                    var place = new Theater() { Name = model.Name, Address = model.Address, Address2 = model.Address2, City = model.City, State = model.State, Zip = model.Zip, Owners = new List<Owner> { owner } };
                    DataContext.Places.Add(place);
                }
                DataContext.SaveChanges();
                return RedirectToAction("Manage", "Account");
            }
            else
            {
                var allErrors = ModelState.Values.SelectMany(v => v.Errors);
            }
            return View(model);
        }

        [Authorize(Roles="Owner")]
        public ActionResult Edit(int id)
        {
            var model = new PlaceEditViewModel();
            model.Place = DataContext.Places.Include("Owners").Include("Owners.ApplicationUser").Where(x => x.Id == id).FirstOrDefault();
            return View(model);
        }

        [Authorize(Roles = "Owner")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(PlaceEditViewModel model)
        {
            var place = DataContext.Places.Find(model.Place.Id);
            var address = Geolocation.ParseAddress(Url.Encode(model.Place.Address + " " + model.Place.Address2 + " " + model.Place.City + ", " + model.Place.State + " " + model.Place.Zip));
            place.Address = model.Place.Address;
            place.Address2 = model.Place.Address2;
            place.City = model.Place.City;
            place.State = model.Place.State;
            place.Zip = model.Place.Zip;
            place.Country = address.Country;
            place.Latitude = address.Latitude;
            place.Longitude = address.Longitude;
            place.Name = model.Place.Name;
            place.FacebookLink = model.Place.FacebookLink;
            place.Website = model.Place.Website;

            DataContext.Entry(place).State = EntityState.Modified;
            DataContext.SaveChanges();

            return RedirectToAction("Details", "Place", new { id = model.Place.Id });
        }


        public JsonResult Search(string searchString)
        {
            //  var places = DataContext.Places.Where(p => (p.Address + " " + p.City + " " + p.State + " " + p.Zip).Contains(searchString)).ToList();
            var places = DataContext.Places.Where(p => (p.Name + " " + p.Address + " " + p.City + " " + p.State + " " + p.Zip).Contains(searchString)).Select(s => new { Id = s.Id, Name = s.Name + " " + s.Address + " " + s.City + ", " + s.State + " " + s.Zip }).ToList();
            return Json(places, JsonRequestBehavior.AllowGet);
        }
    }
}
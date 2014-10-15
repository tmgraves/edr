﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using EDR.Models;
using EDR.Models.ViewModels;
using EDR.Enums;
using Microsoft.AspNet.Identity;

namespace EDR.Controllers
{
    public class PlaceController : BaseController
    {
        public ActionResult List()
        {
            var model = new PlaceListViewModel();
            model.ConferenceCenters = DataContext.Places.OfType<ConferenceCenter>().ToList();
            model.Hotels = DataContext.Places.OfType<Hotel>().ToList();
            model.Nightclubs = DataContext.Places.OfType<Nightclub>().ToList();
            model.Restaurants = DataContext.Places.OfType<Restaurant>().ToList();
            model.Studios = DataContext.Places.OfType<Studio>().ToList();
            model.Theaters = DataContext.Places.OfType<Theater>().ToList();

            return View(model);
        }

        public ActionResult Details(int id, PlaceDetailViewModel model)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var viewModel = new PlaceDetailViewModel();
            viewModel.Place = DataContext.Places.Include("Teachers").Include("Owners").Where(x => x.Id == id).FirstOrDefault();
            viewModel.DanceStyleList = DataContext.DanceStyles.Select(d => new SelectListItem() { Text = d.Name, Value = d.Id.ToString() }).ToList();
            viewModel.Owners = DataContext.Owners.Where(y => y.Places.Any(x => x.Id == id)).ToList();
            viewModel.TeacherList = DataContext.Teachers.Where(x => x.Classes.Any(c => c.Place.Id == id) || x.Workshops.Any(w => w.Place.Id == id)).Select(t => new SelectListItem() { Text = t.ApplicationUser.FirstName + " " + t.ApplicationUser.LastName, Value = t.ApplicationUser.Id.ToString() }).ToList();
            var date = DateTime.Now.AddDays(21);
            viewModel.Classes = DataContext.Events.Include("Teachers").Include("Teachers.ApplicationUser").Include("DanceStyles").Include("Users").OfType<Class>().Where(c => c.Place.Id == id).Where(x => x.IsAvailable == true).Where(y => !y.Recurring ? (y.StartDate >= DateTime.Now) : (y.StartDate <= date && (y.EndDate == null || y.EndDate >= DateTime.Now))).OrderBy(z => z.StartDate).ToList();
            viewModel.Workshops = DataContext.Events.Include("Teachers").Include("Teachers.ApplicationUser").Include("DanceStyles").Include("Users").OfType<Workshop>().Where(c => c.Place.Id == id).Where(x => x.IsAvailable == true).Where(y => !y.Recurring ? (y.StartDate >= DateTime.Now) : (y.StartDate <= date && (y.EndDate == null || y.EndDate >= DateTime.Now))).OrderBy(z => z.StartDate).ToList();
            var Events = DataContext.Events.Include("DanceStyles").Include("Users").Where(c => c.Place.Id == id).Where(x => x.IsAvailable == true).Where(y => !y.Recurring ? (y.StartDate >= DateTime.Now) : (y.StartDate <= date && (y.EndDate == null || y.EndDate >= DateTime.Now))).OrderBy(z => z.StartDate).ToList();

            if (model.DanceStyleId != null)
            {
                viewModel.Classes = viewModel.Classes.Where(x => x.DanceStyles.Any(s => s.Id == model.DanceStyleId));
                viewModel.Workshops = viewModel.Workshops.Where(x => x.DanceStyles.Any(s => s.Id == model.DanceStyleId));
                Events = Events.Where(x => x.DanceStyles.Any(s => s.Id == model.DanceStyleId)).ToList();
            }

            if (model.TeacherId != null && model.TeacherId != "")
            {
                viewModel.Classes = viewModel.Classes.Where(x => x.Teachers.Any(t => t.ApplicationUser.Id == model.TeacherId));
                viewModel.Workshops = viewModel.Workshops.Where(x => x.Teachers.Any(t => t.ApplicationUser.Id == model.TeacherId));
            }

            if (model.SkillLevel != null && model.SkillLevel != 0)
            {
                viewModel.Classes = viewModel.Classes.Where(x => x.SkillLevel == (int)model.SkillLevel);
                viewModel.Workshops = viewModel.Workshops.Where(x => x.SkillLevel == (int)model.SkillLevel);
            }

            viewModel.Socials = Events.OfType<Social>();
            viewModel.Concerts = Events.OfType<Concert>();
            viewModel.Conferences = Events.OfType<Conference>();
            viewModel.Parties = Events.OfType<Party>();
            viewModel.OpenHouses = Events.OfType<OpenHouse>();

            return View(viewModel);
        }

        [Authorize(Roles="Owner")]
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
                    var place = new Studio() { Name = model.Name, Address = model.Address, Address2 = model.Address2, City = model.City, State = model.State.ToString(), Zip = model.Zip, Owners = new List<Owner> { owner } };
                    DataContext.Places.Add(place);
                }
                else if (model.PlaceType == PlaceType.ConferenceCenter)
                {
                    var place = new ConferenceCenter() { Name = model.Name, Address = model.Address, Address2 = model.Address2, City = model.City, State = model.State.ToString(), Zip = model.Zip, Owners = new List<Owner> { owner } };
                    DataContext.Places.Add(place);
                }
                else if (model.PlaceType == PlaceType.Hotel)
                {
                    var place = new Hotel() { Name = model.Name, Address = model.Address, Address2 = model.Address2, City = model.City, State = model.State.ToString(), Zip = model.Zip, Owners = new List<Owner> { owner } };
                    DataContext.Places.Add(place);
                }
                else if (model.PlaceType == PlaceType.Nightclub)
                {
                    var place = new Nightclub() { Name = model.Name, Address = model.Address, Address2 = model.Address2, City = model.City, State = model.State.ToString(), Zip = model.Zip, Owners = new List<Owner> { owner } };
                    DataContext.Places.Add(place);
                }
                else if (model.PlaceType == PlaceType.OtherPlace)
                {
                    var place = new OtherPlace() { Name = model.Name, Address = model.Address, Address2 = model.Address2, City = model.City, State = model.State.ToString(), Zip = model.Zip, Owners = new List<Owner> { owner } };
                    DataContext.Places.Add(place);
                }
                else if (model.PlaceType == PlaceType.Restaurant)
                {
                    var place = new Restaurant() { Name = model.Name, Address = model.Address, Address2 = model.Address2, City = model.City, State = model.State.ToString(), Zip = model.Zip, Owners = new List<Owner> { owner } };
                    DataContext.Places.Add(place);
                }
                else if (model.PlaceType == PlaceType.Theater)
                {
                    var place = new Theater() { Name = model.Name, Address = model.Address, Address2 = model.Address2, City = model.City, State = model.State.ToString(), Zip = model.Zip, Owners = new List<Owner> { owner } };
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
    }
}
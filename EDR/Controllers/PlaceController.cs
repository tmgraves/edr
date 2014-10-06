using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using EDR.Models;
using EDR.Models.ViewModels;

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

        public ActionResult Details(int id, int? danceStyle, string teacher, int? skillLevel)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var viewModel = new PlaceDetailViewModel();
            viewModel.Place = DataContext.Places.Include("Teachers").Include("Owners").Where(x => x.Id == id).FirstOrDefault();
            viewModel.DanceStyleList = DataContext.DanceStyles.Select(d => new SelectListItem() { Text = d.Name, Value = d.Id.ToString() }).ToList();
            viewModel.TeacherList = DataContext.Teachers.Where(x => x.Classes.Any(c => c.Place.Id == id) || x.Workshops.Any(w => w.Place.Id == id)).Select(t => new SelectListItem() { Text = t.ApplicationUser.FirstName + " " + t.ApplicationUser.LastName, Value = t.ApplicationUser.Id.ToString() }).ToList();
            var date = DateTime.Now.AddDays(21);
            viewModel.Classes = DataContext.Events.Include("Teachers").Include("Teachers.ApplicationUser").Include("DanceStyles").Include("Users").OfType<Class>().Where(c => c.Place.Id == id).Where(x => x.IsAvailable == true).Where(y => !y.Recurring ? (y.StartDate >= DateTime.Now) : (y.StartDate <= date && (y.EndDate == null || y.EndDate >= DateTime.Now))).OrderBy(z => z.StartDate).ToList();
            viewModel.Workshops = DataContext.Events.Include("Teachers").Include("Teachers.ApplicationUser").Include("DanceStyles").Include("Users").OfType<Workshop>().Where(c => c.Place.Id == id).Where(x => x.IsAvailable == true).Where(y => !y.Recurring ? (y.StartDate >= DateTime.Now) : (y.StartDate <= date && (y.EndDate == null || y.EndDate >= DateTime.Now))).OrderBy(z => z.StartDate).ToList();
            var Events = DataContext.Events.Include("DanceStyles").Include("Users").Where(c => c.Place.Id == id).Where(x => x.IsAvailable == true).Where(y => !y.Recurring ? (y.StartDate >= DateTime.Now) : (y.StartDate <= date && (y.EndDate == null || y.EndDate >= DateTime.Now))).OrderBy(z => z.StartDate).ToList();

            if (danceStyle != null)
            {
                viewModel.Classes = viewModel.Classes.Where(x => x.DanceStyles.Any(s => s.Id == danceStyle));
                viewModel.Workshops = viewModel.Workshops.Where(x => x.DanceStyles.Any(s => s.Id == danceStyle));
                Events = Events.Where(x => x.DanceStyles.Any(s => s.Id == danceStyle)).ToList();
            }

            if (teacher != null && teacher != "")
            {
                viewModel.Classes = viewModel.Classes.Where(x => x.Teachers.Any(t => t.ApplicationUser.Id == teacher));
                viewModel.Workshops = viewModel.Workshops.Where(x => x.Teachers.Any(t => t.ApplicationUser.Id == teacher));
            }

            if (skillLevel != null)
            {
                viewModel.Classes = viewModel.Classes.Where(x => x.SkillLevel == skillLevel);
                viewModel.Workshops = viewModel.Workshops.Where(x => x.SkillLevel == skillLevel);
            }

            viewModel.Socials = Events.OfType<Social>();
            viewModel.Concerts = Events.OfType<Concert>();
            viewModel.Conferences = Events.OfType<Conference>();
            viewModel.Parties = Events.OfType<Party>();
            viewModel.OpenHouses = Events.OfType<OpenHouse>();

            return View(viewModel);
        }
    }
}
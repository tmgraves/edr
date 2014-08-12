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
        public ActionResult Details(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var viewModel = new PlaceDetailViewModel();
            viewModel.Place = DataContext.Places.Find(id);
            viewModel.Classes = DataContext.Events.Include("Teachers").Include("DanceStyles").Include("Users").OfType<Class>().Where(c => c.Place.Id == id).Where(x => x.IsAvailable == true).Where(y => y.StartDate >= DateTime.Now).OrderBy(z => z.StartDate).ToList().Take(5);
            viewModel.Socials = DataContext.Events.Include("DanceStyles").Include("Users").OfType<Social>().Where(c => c.Place.Id == id).Where(x => x.IsAvailable == true).Where(y => y.StartDate >= DateTime.Now).OrderBy(z => z.StartDate).ToList().Take(5);
            viewModel.Concerts = DataContext.Events.Include("DanceStyles").Include("Users").OfType<Concert>().Where(c => c.Place.Id == id).Where(x => x.IsAvailable == true).Where(y => y.StartDate >= DateTime.Now).OrderBy(z => z.StartDate).ToList().Take(5);
            viewModel.Conferences = DataContext.Events.Include("DanceStyles").Include("Users").OfType<Conference>().Where(c => c.Place.Id == id).Where(x => x.IsAvailable == true).Where(y => y.StartDate >= DateTime.Now).OrderBy(z => z.StartDate).ToList().Take(5);
            viewModel.Parties = DataContext.Events.Include("DanceStyles").Include("Users").OfType<Party>().Where(c => c.Place.Id == id).Where(x => x.IsAvailable == true).Where(y => y.StartDate >= DateTime.Now).OrderBy(z => z.StartDate).ToList().Take(5);
            viewModel.OpenHouses = DataContext.Events.Include("DanceStyles").Include("Users").OfType<OpenHouse>().Where(c => c.Place.Id == id).Where(x => x.IsAvailable == true).Where(y => y.StartDate >= DateTime.Now).OrderBy(z => z.StartDate).ToList().Take(5);

            return View(viewModel);
        }
    }
}
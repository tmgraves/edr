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
        public ActionResult Details(int id, int? danceStyle, string teacher, int? skillLevel)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var TeacherLst = DataContext.IdentityUsers.OfType<Teacher>().Where(x => x.Classes.Any(c => c.Place.Id == id) || x.Workshops.Any(w => w.Place.Id == id));
            ViewBag.teacher = new SelectList(TeacherLst, "Id", "FullName", teacher);

            var DanceStyleLst = DataContext.DanceStyles.ToList();
            ViewBag.danceStyle = new SelectList(DanceStyleLst, "Id", "Name", danceStyle);

            var SkillLevelLst = new List<int> { 1, 2, 3, 4, 5 };
            ViewBag.skillLevel = new SelectList(SkillLevelLst);

            var viewModel = new PlaceDetailViewModel();
            viewModel.Place = DataContext.Places.Find(id);
            viewModel.Classes = DataContext.Events.Include("Teachers").Include("DanceStyles").Include("Users").OfType<Class>().Where(c => c.Place.Id == id).Where(x => x.IsAvailable == true).Where(y => !y.Recurring ? (y.StartDate >= DateTime.Now) : (y.StartDate <= DateTime.Now && (y.EndDate == null || y.EndDate >= DateTime.Now))).OrderBy(z => z.StartDate).ToList();
            var Events = DataContext.Events.Include("DanceStyles").Include("Users").Where(c => c.Place.Id == id).Where(x => x.IsAvailable == true).Where(y => y.StartDate >= DateTime.Now).OrderBy(z => z.StartDate).ToList().Take(5);

            if (danceStyle != null)
            {
                viewModel.Classes = viewModel.Classes.Where(x => x.DanceStyles.Any(s => s.Id == danceStyle));
                Events = Events.Where(x => x.DanceStyles.Any(s => s.Id == danceStyle));
            }

            if (teacher != null && teacher != "")
            {
                viewModel.Classes = viewModel.Classes.Where(x => x.Teachers.Any(t => t.Id == teacher));
            }

            if (skillLevel != null)
            {
                viewModel.Classes = viewModel.Classes.Where(x => x.SkillLevel == skillLevel);
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
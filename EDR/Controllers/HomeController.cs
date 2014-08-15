using EDR.Models;
using EDR.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;

namespace EDR.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Explore()
        {
            var viewModel = new HomeExploreViewModel();
            viewModel.DanceStyles = DataContext.DanceStyles.ToList();
            return View(viewModel);
        }

        public ActionResult Learn(int? danceStyle, string teacher, int? place, int? skillLevel)
        {
            var DanceStyleLst = DataContext.DanceStyles.ToList();
            ViewBag.danceStyle = new SelectList(DanceStyleLst, "Id", "Name", danceStyle);

            var TeacherLst = DataContext.Users.OfType<Teacher>().ToList();
            ViewBag.teacher = new SelectList(TeacherLst, "Id", "FullName", teacher);

            var PlaceLst = DataContext.Places.ToList();
            ViewBag.place = new SelectList(PlaceLst, "Id", "Name", place);

            var SkillLevelLst = new List<int> { 1, 2, 3, 4, 5 };
            ViewBag.skillLevel = new SelectList(SkillLevelLst);

            var viewModel = new HomeLearnViewModel();
            viewModel.Classes = DataContext.Events.OfType<Class>().Include("Teachers").Include("DanceStyles").Include("Users").Where(x => x.IsAvailable == true).Where(y => !y.Recurring ? (y.StartDate >= DateTime.Now) : ((y.EndDate == null || y.EndDate >= DateTime.Now))).OrderBy(z => z.StartDate).ToList();
            viewModel.Workshops = DataContext.Events.OfType<Workshop>().Include("Teachers").Include("DanceStyles").Include("Users").Where(x => x.IsAvailable == true).Where(y => !y.Recurring ? (y.StartDate >= DateTime.Now) : ((y.EndDate == null || y.EndDate >= DateTime.Now))).OrderBy(z => z.StartDate).ToList();
            viewModel.ClassSeries = DataContext.Series.OfType<ClassSeries>().Include("DanceStyles").Where(x => x.IsAvailable == true).ToList();

            if (danceStyle != null)
            {
                viewModel.Classes = viewModel.Classes.Where(x => x.DanceStyles.Any(s => s.Id == danceStyle));
                viewModel.ClassSeries = viewModel.ClassSeries.Where(x => x.DanceStyles.Any(s => s.Id == danceStyle));
            }

            if (teacher != null && teacher != "")
            {
                viewModel.Classes = viewModel.Classes.Where(x => x.Teachers.Any(t => t.Id == teacher));
                viewModel.ClassSeries = viewModel.ClassSeries.Where(x => x.Teachers.Any(t => t.Id == teacher));
            }

            if (place != null)
            {
                viewModel.Classes = viewModel.Classes.Where(x => x.Place.Id == place);
                viewModel.ClassSeries = viewModel.ClassSeries.Where(x => x.Place.Id == place);
            }

            if (skillLevel != null)
            {
                viewModel.Classes = viewModel.Classes.Where(x => x.SkillLevel == skillLevel);
                viewModel.ClassSeries = viewModel.ClassSeries.Where(x => x.SkillLevel == skillLevel);
            }

            return View(viewModel);
        }

        public ActionResult Teachers()
        {
            return View(DataContext.IdentityUsers.OfType<Teacher>().Include("DanceStyles").ToList());
        }
    }
}
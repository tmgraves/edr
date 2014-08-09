﻿using EDR.Models;
using EDR.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

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

        public ActionResult Learn(int? danceStyle, string teacher, int? place)
        {
            var DanceStyleLst = DataContext.DanceStyles.ToList();
            ViewBag.danceStyle = new SelectList(DanceStyleLst, "Id", "Name", danceStyle);

            var TeacherLst = DataContext.Users.OfType<Teacher>().ToList();
            ViewBag.teacher = new SelectList(TeacherLst, "Id", "FullName", teacher);

            var PlaceLst = DataContext.Places.ToList();
            ViewBag.place = new SelectList(PlaceLst, "Id", "Name", place);

            var viewModel = new HomeLearnViewModel();
            viewModel.Classes = DataContext.Events.Include("Teachers").Include("DanceStyles").OfType<Class>().Where(x => x.IsAvailable == true).ToList();
            viewModel.ClassSeries = DataContext.Series.Include("DanceStyles").OfType<ClassSeries>().Where(x => x.IsAvailable == true).ToList();

            if (danceStyle != null)
            {
                viewModel.Classes = viewModel.Classes.Where(x => x.DanceStyles.Any(s => s.Id == danceStyle));
                viewModel.ClassSeries = viewModel.ClassSeries.Where(x => x.DanceStyles.Any(s => s.Id == danceStyle));
            }

            if (teacher != "")
            {
                viewModel.Classes = viewModel.Classes.Where(x => x.Teachers.Any(t => t.Id == teacher));
                viewModel.ClassSeries = viewModel.ClassSeries.Where(x => x.Teachers.Any(t => t.Id == teacher));
            }

            if (place != null)
            {
                viewModel.Classes = viewModel.Classes.Where(x => x.Place.Id == place);
                viewModel.ClassSeries = viewModel.ClassSeries.Where(x => x.Place.Id == place);
            }

            return View(viewModel);
        }
    }
}
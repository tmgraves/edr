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
        public ActionResult Index(IEnumerable<int> styles)
        {
            var viewModel = new HomeIndexViewModel();
            viewModel.DanceStyles = DataContext.DanceStyles.ToList();

            if (styles != null && styles.Count() > 0)
                viewModel.Classes = (from result in DataContext.Events.OfType<Class>()
                                    where result.IsAvailable == true
                                    where styles.Contains(result.DanceStyle.Id)
                                    select result).ToList();
            else
                viewModel.Classes = DataContext.Events.OfType<Class>().Where(x => x.IsAvailable == true).ToList();

            var studios = DataContext.Places.OfType<Studio>().ToList();
            var theaters = DataContext.Places.OfType<Theater>().ToList();

            return View(viewModel);
        }

        public ActionResult Explore()
        {
            return View();
        }
    }
}
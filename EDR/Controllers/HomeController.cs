using EDR.Models;
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
            viewModel.Events = DataContext.Events.Where(x => x.IsAvailable == true).ToList();

            return View(viewModel);
        }

        public ActionResult Explore()
        {
            return View();
        }
    }
}
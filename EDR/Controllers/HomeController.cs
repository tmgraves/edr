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
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Explore()
        {
            return View();
        }

        public ActionResult Learn(IEnumerable<int> styles)
        {
            var viewModel = new HomeLearnViewModel();
            viewModel.Classes = DataContext.Classes.Where(x => x.IsAvailable == true).ToList();
            viewModel.ClassSeries = DataContext.ClassSeries.Where(x => x.IsAvailable == true).ToList();

            return View(viewModel);
        }
    }
}
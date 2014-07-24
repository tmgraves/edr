using EDR.Models.ViewModels;
using System;
using System.Linq;
using System.Web.Mvc;

namespace EDR.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            var classes = DataContext.Classes.Where(x => x.IsAvailable == true).ToList();

            return View(classes);
        }

        public ActionResult Explore() { return View(); }

        public ActionResult Learn(string danceStyle) 
        {
            var classes = DataContext.Classes.Where(x => x.DanceStyle.Name == danceStyle).ToList();

            return View(classes);
        }
    }
}
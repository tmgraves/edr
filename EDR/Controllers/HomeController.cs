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
            var classes = DataContext.Classes.Where(x => x.IsAvailable == true).ToList();

            return View(classes);
        }

        public ActionResult Explore() { return View(); }

        public ActionResult Learn(string id) 
        {
            var DanceStyleLst = new List<string>();

            var DanceStyleQry = from s in DataContext.DanceStyles
                           orderby s.Name
                           select s.Name;

            DanceStyleLst.AddRange(DanceStyleQry.Distinct());
            ViewBag.danceStyle = new SelectList(DanceStyleLst); 

            var classes = DataContext.Classes.Where(x => x.DanceStyle.Name == id).ToList();

            return View(classes);
        }
    }
}
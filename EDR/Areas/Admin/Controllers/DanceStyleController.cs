using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EDR.Controllers;
using System.Data.Entity;
using EDR.Utilities;
using System.IO;

namespace EDR.Areas.Admin.Controllers
{
    public class DanceStyleController : BaseController
    {
        // GET: Admin/DanceStyle
        public ActionResult Index()
        {
            var styles = DataContext.DanceStyles;
            return View(styles);
        }

        public ActionResult Details(int id)
        {
            var style = DataContext.DanceStyles.Where(s => s.Id == id).Include("Videos").FirstOrDefault();
            return View(style);
        }
    }
}
using EDR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace EDR.Controllers
{
    public class ClassController : BaseController
    {
        public ActionResult Details(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var model = DataContext.Events.OfType<Class>().Where(x => x.Id == id).FirstOrDefault();
            if (model == null)
            {
                return HttpNotFound();
            }

            model.DanceStyles = DataContext.DanceStyles.Where(c => c.Events.Any(e => e.Id == id)).ToList();
            model.Reviews = DataContext.Reviews.Where(c => c.Id == id).ToList();
            model.Teachers = DataContext.Users.OfType<Teacher>().Where(t => t.Classes.Any(c => c.Id == id)).ToList();

            return View(model);
        }
    }
}
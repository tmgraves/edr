using EDR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace EDR.Controllers
{
    public class ClassSeriesController : BaseController
    {
        public ActionResult Details(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var model = DataContext.ClassSeries.Where(x => x.Id == id).FirstOrDefault();

            if (model == null)
            {
                return HttpNotFound();
            }

            // Correct way to get data from db
            model.Classes = DataContext.Events.OfType<Class>().ToList(); // Notice the .OfType<>() to filter specific types
            model.DanceStyles = DataContext.DanceStyles.ToList();
            model.Events = DataContext.Events.ToList();

            // Incorrect way to get data from db
            DataContext.Entry(model).Collection(x => x.DanceStyles).Load();
            DataContext.Entry(model).Collection(x => x.Teachers).Load();
            DataContext.Entry(model).Collection(x => x.Reviews).Load();
            DataContext.Entry(model).Collection(x => x.Events).Load();

            return View(model);
        }
    }
}
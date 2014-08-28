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

            var model = DataContext.Series.OfType<ClassSeries>().Where(x => x.Id == id).FirstOrDefault();

            if (model == null)
            {
                return HttpNotFound();
            }

            // Correct way to get data from db
            //model.Classes = DataContext.Events.Where(x => x.Series.Id == id).OfType<Class>().ToList(); // Notice the .OfType<>() to filter specific types
            model.Events = DataContext.Events.Include("Reviews").Where(x => x.Series.Id == id).OrderBy(x => x.StartDate).ToList(); // Notice the .OfType<>() to filter specific types
            model.DanceStyles = DataContext.DanceStyles.Where(c => c.Events.Any(e => e.Series.Id == id)).ToList();
            model.Teachers = DataContext.Teachers.Where(t => t.ClassSeries.Any(c => c.Id == id)).ToList();

            //model.Reviews = DataContext.Series.Single(s => s.Id == id).Reviews;
            //model.Teachers = DataContext.Series.OfType<ClassSeries>().Single(s => s.Id == id).Teachers;
            //model.DanceStyles = DataContext.Series.Single(s => s.Id == id).DanceStyles;

            //// Incorrect way to get data from db
            //DataContext.Entry(model).Collection(x => x.DanceStyles).Load();
            //DataContext.Entry(model).Collection(x => x.Teachers).Load();
            //DataContext.Entry(model).Collection(x => x.Reviews).Load();
            //DataContext.Entry(model).Collection(x => x.Events).Load();

            return View(model);
        }
    }
}
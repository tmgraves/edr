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

            DataContext.Entry(model).Collection(x => x.DanceStyles).Load();
            DataContext.Entry(model).Collection(x => x.Teachers).Load();
            DataContext.Entry(model).Collection(x => x.Reviews).Load();
            DataContext.Entry(model).Collection(x => x.Events).Load();

            return View(model);
        }
    }
}
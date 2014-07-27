using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace EDR.Controllers
{
    public class EventController : BaseController
    {
        public ActionResult Details(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var model = DataContext.Events.Where(x => x.Id == id).FirstOrDefault();
            if (model == null)
            {
                return HttpNotFound();
            }
            
            
            return View(model);
        }
    }
}
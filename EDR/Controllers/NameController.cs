using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EDR.Controllers
{
    public class NameController : BaseController
    {
        //[Route("{username}")]
        // GET: Name
        public ActionResult Default(string username)
        {
            return View("Home");
        }
    }
}
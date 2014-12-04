using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EDR.Models.ViewModels;
using System.Threading.Tasks;

namespace EDR.Controllers
{
    public class VisitorController : BaseController
    {
        // GET: Visitor
        public ActionResult View()
        {
            var viewModel = new VisitorViewViewModel();

            return View(viewModel);
        }
    }
}
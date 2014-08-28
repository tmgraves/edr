using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using EDR.Data;
using EDR.Models;
using EDR.Models.ViewModels;
using System.Web.Security;

namespace EDR.Controllers
{
    public class TeacherController : BaseController
    {
        // GET: Teacher/Details/5
        public ActionResult Details(string username)
        {
            if (username == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var viewModel = DataContext.Teachers.Include("ApplicationUser").Include("Classes").Include("Workshops").Include("DanceStyles").Where(x => x.ApplicationUser.UserName == username).FirstOrDefault();

            return View(viewModel);
        }
    }
}

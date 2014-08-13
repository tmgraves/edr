using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using EDR.Data;
using EDR.Models;
using EDR.Models.ViewModels;

namespace EDR.Controllers
{
    public class TeacherController : BaseController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Teacher
        public ActionResult Index()
        {
            return View(DataContext.IdentityUsers.OfType<Teacher>().Include("DanceStyles").ToList());
        }

        // GET: Teacher/Details/5
        public ActionResult Details(string username)
        {
            if (username == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var viewModel = new TeacherDetailViewModel();
            viewModel.Teacher = DataContext.IdentityUsers.OfType<Teacher>().Include("Classes").Include("Workshops").Include("DanceStyles").Where(x => x.UserName == username).FirstOrDefault();

            return View(viewModel);
        }
    }
}

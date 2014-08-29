using EDR.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;

namespace EDR.Controllers
{
    public class TeacherController : BaseController
    {
        public ActionResult View(string username)
        {
            if (String.IsNullOrWhiteSpace(username))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var teacher = DataContext.Teachers
                .Where(x => x.ApplicationUser.UserName == username)
                .FirstOrDefault();

            if (teacher == null)
            {
                return HttpNotFound();
            }

            // TODO: FILL MORE VIEWMODEL PROPERTIES (SEE TeacherViewModel)
            var viewModel = new TeacherViewViewModel();
            viewModel.Name = teacher.ApplicationUser.FullName;

            return View(viewModel);
        }

        [Authorize]
        public ActionResult Edit()
        {
            var teacher = DataContext.Teachers
                .Where(x => x.ApplicationUser.UserName == User.Identity.Name)
                .FirstOrDefault();

            if (teacher == null)
            {
                return HttpNotFound();
            }

            var viewModel = new TeacherEditViewModel();
            viewModel.Name = teacher.ApplicationUser.FullName;

            return View(viewModel);
        }

        [Authorize]
        public ActionResult Apply()
        {
            // TODO: SAVE TEACHER RECORD WITH 'Active=false' ONCE APPROVED SWITCH TO 'True'
            //       AND ADD USER TO 'Teacher' ROLE

            var user = UserManager.FindByName(User.Identity.Name);

            var viewModel = new TeacherApplyViewModel();
            viewModel.Name = user.FullName;

            return View(viewModel);
        }
    }
}
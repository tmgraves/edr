using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using EDR.Models.ViewModels;

namespace EDR.Controllers
{
    public class ProfilesController : BaseController
    {
        public ActionResult Dancer(string username)
        {
            if (String.IsNullOrWhiteSpace(username))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var dancer = UserManager.FindByName(username);
            if(dancer == null)
            {
                return HttpNotFound();
            }

            // TODO: FILL MORE VIEWMODEL PROPERTIES (SEE DancerViewModel)
            var viewModel = new DancerViewModel();
            viewModel.Name = dancer.FullName;
            
            return View(viewModel);
        }

        public ActionResult Teacher(string username)
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
            var viewModel = new TeacherViewModel();
            viewModel.Name = teacher.ApplicationUser.FullName;

            return View(viewModel);
        }

        public ActionResult Promoter(string username)
        {
            if (String.IsNullOrWhiteSpace(username))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var promoter = DataContext.Promoters
                .Where(x => x.ApplicationUser.UserName == username)
                .FirstOrDefault();

            if(promoter == null)
            {
                return HttpNotFound();
            }

            // TODO: FILL MORE VIEWMODEL PROPERTIES (SEE PromoterViewModel)
            var viewModel = new PromoterViewModel();
            viewModel.Name = promoter.ApplicationUser.FullName;

            return View(viewModel);
        }

        public ActionResult Owner(string username)
        {
            if (String.IsNullOrWhiteSpace(username))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var owner = DataContext.Owners
                .Where(x => x.ApplicationUser.UserName == username)
                .FirstOrDefault();

            if(owner == null)
            {
                return HttpNotFound();
            }

            // TODO: FILL MORE VIEWMODEL PROPERTIES (SEE PromoterViewModel)
            var viewModel = new OwnerViewModel();
            viewModel.Name = owner.ApplicationUser.FullName;

            return View(viewModel);
        }
    }
}
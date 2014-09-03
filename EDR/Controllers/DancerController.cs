using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using EDR.Models.ViewModels;
using System.Data;
using System.Data.Entity;
using EDR.Models;
using EDR.Data;

namespace EDR.Controllers
{
    public class DancerController : BaseController
    {
        public ActionResult View(string username)
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
            var viewModel = new DancerViewViewModel();
            viewModel.Dancer = DataContext.Users.Where(x => x.UserName == username).Include("DanceStyles").Include("Events").FirstOrDefault();
            
            return View(viewModel);
        }

        [Authorize]
        public ActionResult Edit()
        {
            var dancer = UserManager.FindByName(User.Identity.Name);
            if (dancer == null)
            {
                return HttpNotFound();
            }

            var viewModel = new DancerEditViewModel();
            viewModel.Username = dancer.UserName;
            viewModel.Name = dancer.FullName;

            return View(viewModel);
        }
    }
}
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
    public class OwnerController : BaseController
    {
        public ActionResult View(string username)
        {
            if (String.IsNullOrWhiteSpace(username))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var owner = DataContext.Owners
                .Where(x => x.ApplicationUser.UserName == username)
                .FirstOrDefault();

            if (owner == null)
            {
                return HttpNotFound();
            }

            // TODO: FILL MORE VIEWMODEL PROPERTIES (SEE PromoterViewModel)
            var viewModel = new OwnerViewViewModel();
            viewModel.Name = owner.ApplicationUser.FullName;

            return View(viewModel);
        }

        [Authorize]
        public ActionResult Edit()
        {
            var owner = DataContext.Owners
                .Where(x => x.ApplicationUser.UserName == User.Identity.Name)
                .FirstOrDefault();

            if (owner == null)
            {
                return HttpNotFound();
            }

            var viewModel = new OwnerEditViewModel();
            viewModel.Name = owner.ApplicationUser.FullName;

            return View(viewModel);
        }

        [Authorize]
        public ActionResult Apply()
        {
            // TODO: SAVE OWNER RECORD WITH 'Active=false' ONCE APPROVED SWITCH TO 'True'
            //       AND ADD USER TO 'Owner' ROLE

            var user = UserManager.FindByName(User.Identity.Name);

            var viewModel = new OwnerApplyViewModel();
            viewModel.Name = user.FullName;

            return View(viewModel);
        }
    }
}
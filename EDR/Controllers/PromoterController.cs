using EDR.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using System.Net;

namespace EDR.Controllers
{
    public class PromoterController : BaseController
    {
        public ActionResult View(string username)
        {
            if (String.IsNullOrWhiteSpace(username))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var promoter = DataContext.Promoters
                .Where(x => x.ApplicationUser.UserName == username)
                .FirstOrDefault();

            if (promoter == null)
            {
                return HttpNotFound();
            }

            // TODO: FILL MORE VIEWMODEL PROPERTIES (SEE PromoterViewModel)
            var viewModel = new PromoterViewViewModel();
            viewModel.Name = promoter.ApplicationUser.FullName;

            return View(viewModel);
        }

        [Authorize]
        public ActionResult Edit()
        {
            var promoter = DataContext.Promoters
                .Where(x => x.ApplicationUser.UserName == User.Identity.Name)
                .FirstOrDefault();

            if (promoter == null)
            {
                return HttpNotFound();
            }

            var viewModel = new PromoterEditViewModel();
            viewModel.Name = promoter.ApplicationUser.FullName;

            return View(viewModel);
        }

        [Authorize]
        public ActionResult Apply()
        {
            // TODO: SAVE PROMOTER RECORD WITH 'Active=false' ONCE APPROVED SWITCH TO 'True'
            //       AND ADD USER TO 'Promoter' ROLE

            var user = UserManager.FindByName(User.Identity.Name);

            var viewModel = new PromoterApplyViewModel();
            viewModel.Name = user.FullName;

            return View(viewModel);
        }
    }
}
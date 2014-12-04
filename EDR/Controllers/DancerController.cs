using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using EDR.Models.ViewModels;
using System.Data.Entity;
using EDR.Models;
using Facebook;
using Microsoft.AspNet.Facebook;
using Microsoft.AspNet.Facebook.Client;
using System.Threading.Tasks;

namespace EDR.Controllers
{
    public class DancerController : BaseController
    {
        public ActionResult List()
        {
            var model = new DancerListViewModel();
            model.Dancers = DataContext.Users;

            return View(model);
        }

        [Authorize]
        public async Task<ActionResult> View(string username)
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

            var viewModel = new DancerViewViewModel();
            viewModel.Dancer = dancer;

            var user = DataContext.Users.Where(x => x.UserName == username).FirstOrDefault();

            var fb = new FacebookClient(user.FacebookToken);
            dynamic myInfo = fb.Get("/me/friends?fields=id,name,email,link"); 
            var friendsList = new List<FacebookFriendViewModel>();
            foreach (dynamic friend in myInfo.data)
            {
                friendsList.Add(new FacebookFriendViewModel()
                   {
                       Id = friend.id,
                       Name = friend.name,
                       Link = friend.link,
                       ImageURL = @"https://graph.facebook.com/" + friend.id + "/picture?type=small",
                       Email = friend.email
                   });
            }
            viewModel.FriendList = friendsList;

            return View(viewModel);
        }

        [Authorize]
        public ActionResult Edit()
        {
            // TODO: FILL MORE VIEWMODEL PROPERTIES (SEE DancerViewModel)
            var viewModel = GetInitialDancerEditViewModel();

            if (viewModel.Dancer == null)
            {
                return HttpNotFound();
            }

            return View(viewModel);
        }

        private DancerEditViewModel GetInitialDancerEditViewModel()
        {
            var model = new DancerEditViewModel();
            var id = User.Identity.GetUserId();
            model.Dancer = DataContext.Users.Where(x => x.Id == id).Include("DanceStyles").FirstOrDefault();

            var selectedStyles = new List<DanceStyleListItem>();
            foreach (DanceStyle ss in model.Dancer.DanceStyles)
            {
                selectedStyles.Add(new DanceStyleListItem { Id = ss.Id, Name = ss.Name });
            }
            model.SelectedStyles = selectedStyles;

            var styles = new List<DanceStyleListItem>();
            foreach (DanceStyle s in DataContext.DanceStyles)
            {
                styles.Add(new DanceStyleListItem { Id = s.Id, Name = s.Name });
            }
            model.AvailableStyles = styles.OrderBy(x => x.Name);

            return model;
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(DancerEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                var dancer = DataContext.Users.Where(x => x.Id == model.Dancer.Id).Include("DanceStyles").Include("Parties").FirstOrDefault();
                dancer.Experience = model.Dancer.Experience;
                var styles = DataContext.DanceStyles.Where(x => model.PostedStyles.DanceStyleIds.Contains(x.Id.ToString())).ToList();

                dancer.DanceStyles.Clear();
                
                foreach(DanceStyle s in styles)
                {
                    dancer.DanceStyles.Add(s);
                }

                DataContext.Entry(dancer).State = EntityState.Modified;
                DataContext.SaveChanges();
                return RedirectToAction("Manage", "Account");
            }
            return View(model);
        }
    }
}
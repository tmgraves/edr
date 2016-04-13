using EDR.Models.ViewModels;
using EDR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;

namespace EDR.Controllers
{
    public class StoreController : BaseController
    {
        // GET: /StoreItems/
        public ActionResult Index()
        {
            // Set up our ViewModel
            var viewModel = new StoreItemsViewModel
            {
                DancePacks = DataContext.DancePacks.ToList()
            };
            // Return the view
            return View(viewModel);
        }

        [Authorize]
        public ActionResult BuyTicket(int id)
        {
            var tickets =
                    (from i in DataContext.EventInstances
                     join c in DataContext.Classes
                     on i.EventId equals c.Id
                     join t in DataContext.Tickets
                     on c.SchoolId equals t.SchoolId
                     where i.Id == id
                     select t).Distinct();

            var model = new BuyTicketViewModel(tickets.ToList());
            return View(model);
        }
        
        [HttpPost]
        public ActionResult BuyTicket(BuyTicketViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userid = User.Identity.GetUserId();
                var user = DataContext.Users.Single(u => u.Id == userid);
                DataContext.UserTickets.Add(new UserTicket() { UserId = userid, TicketId = model.TicketId, Quantity = model.Quantity });
                DataContext.SaveChanges();
                return RedirectToAction("Home", "Dancer", new { id = userid });
            }
            else
            {
                return View(model);
            }
        }
    }
}
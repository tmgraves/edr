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
        public ActionResult BuyTicket(int? instanceId, int? schoolId)
        {
            var model = new BuyTicketViewModel();
            if (instanceId != null)
            {
                var instance = DataContext.EventInstances.Include("Event").Include("Event.Tickets").Single(e => e.Id == (int)instanceId);

                var tickets = new List<Ticket>();
                if (instance.Event.Tickets.Count() != 0)
                {
                    tickets = instance.Event.Tickets.ToList();
                }
                else
                {
                    tickets =
                            (from i in DataContext.EventInstances
                             join c in DataContext.Classes
                             on i.EventId equals c.Id
                             join t in DataContext.Tickets
                             on c.SchoolId equals t.SchoolId
                             where i.Id == instanceId
                             select t).Distinct().ToList();
                }
                model.Tickets = tickets;
                model.EventInstanceId = instanceId;
                model.Type = instance.Event is Class ? Enums.EventType.Class : Enums.EventType.Social;
                model.EventId = instance.EventId;
            }
            else
            {
                var instance = DataContext.EventInstances.Include("Event").Single(e => e.Id == (int)instanceId);

                var tickets = new List<Ticket>();
                tickets = DataContext.Tickets.Where(t => t.SchoolId == schoolId).ToList();

                model.Tickets = tickets;
                model.SchoolId = schoolId;
            }
            return View(model);
        }
        
        [HttpPost]
        public ActionResult BuyTicket(BuyTicketViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userid = User.Identity.GetUserId();
                var user = DataContext.Users.Single(u => u.Id == userid);
                var ticket = new UserTicket() { UserId = userid, TicketId = model.TicketId, Quantity = model.Quantity };
                DataContext.UserTickets.Add(ticket);
                DataContext.SaveChanges();

                if (model.EventInstanceId != null)
                {
                    DataContext.EventRegistrations.Add(new EventRegistration() { UserId = userid, EventInstanceId = (int)model.EventInstanceId, UserTicketId = ticket.Id });
                    DataContext.SaveChanges();
                    return RedirectToAction("View", "Event", new { id = model.EventId, eventType = model.Type });
                }
                else
                {
                    return RedirectToAction("View", "School", new { id = model.SchoolId });
                }
            }
            else
            {
                return View(model);
            }
        }

        // GET: School
        [Authorize(Roles = "Teacher,Owner")]
        public ActionResult AddTicket(int? schoolId, int? eventId)
        {
            var ticket = new Ticket();
            ticket.SchoolId = schoolId;
            ticket.EventId = eventId;
            return View(ticket);
        }

        // POST: School
        [HttpPost]
        [Authorize(Roles = "Teacher,Owner")]
        public ActionResult AddTicket(Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                //Save Ticket
                DataContext.Tickets.Add(ticket);
                DataContext.SaveChanges();

                if (ticket.SchoolId != null)
                {
                    return RedirectToAction("View", "School", new { id = ticket.SchoolId });
                }
                else
                {
                    var evt = DataContext.Events.Single(e => e.Id == ticket.EventId);
                    return RedirectToAction("View", "Event", new { id = ticket.EventId, eventType = evt is Class ? EDR.Enums.EventType.Class : Enums.EventType.Social });
                }
            }
            else
            {
                return View(ticket);
            }
        }

    }
}
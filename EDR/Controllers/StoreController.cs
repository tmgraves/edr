using EDR.Models.ViewModels;
using EDR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using AuthorizeNet.Api.Controllers;
using AuthorizeNet.Api.Contracts.V1;
using AuthorizeNet.Api.Controllers.Bases;
using System.Configuration;

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
                    return RedirectToAction("Manage", "School", new { id = ticket.SchoolId });
                }
                else
                {
                    var evt = DataContext.Events.Single(e => e.Id == ticket.EventId);
                    return RedirectToAction("Manage", "Event", new { id = ticket.EventId, eventType = evt is Class ? EDR.Enums.EventType.Class : Enums.EventType.Social });
                }
            }
            else
            {
                return View(ticket);
            }
        }

        [HttpPost]
        public ActionResult PostTransaction(string CCNumber, string CCMonth, string CCYear, string SecurityCode, decimal amount)
        {
            Run(CCNumber, CCMonth, CCYear, SecurityCode, amount);
            return RedirectToAction("Test3", "Home");
        }

        [Authorize]
        public static ANetApiResponse Run(string CCNumber, string CCMonth, string CCYear, string SecurityCode, decimal amount)
        {
            Console.WriteLine("Charge Credit Card Sample");

            ApiOperationBase<ANetApiRequest, ANetApiResponse>.RunEnvironment = AuthorizeNet.Environment.SANDBOX;

            // define the merchant information (authentication / transaction id)
            ApiOperationBase<ANetApiRequest, ANetApiResponse>.MerchantAuthentication = new merchantAuthenticationType()
            {
                name = ConfigurationManager.AppSettings["AuthorizeNetApiLoginID"],
                ItemElementName = ItemChoiceType.transactionKey,
                Item = ConfigurationManager.AppSettings["AuthorizeNetApiTransactionKey"],
            };

            var creditCard = new creditCardType
            {
                cardNumber = CCNumber,  //  "4111111111111111",
                expirationDate = CCMonth + CCYear, //   "0718",
                cardCode = SecurityCode //   "123"
            };

            var billingAddress = new customerAddressType
            {
                firstName = "John",
                lastName = "Doe",
                address = "123 My St",
                city = "OurTown",
                zip = "98004"
            };

            //standard api call to retrieve response
            var paymentType = new paymentType { Item = creditCard };

            // Add line Items
            var lineItems = new lineItemType[2];
            lineItems[0] = new lineItemType { itemId = "1", name = "t-shirt", quantity = 2, unitPrice = new Decimal(15.00) };
            lineItems[1] = new lineItemType { itemId = "2", name = "snowboard", quantity = 1, unitPrice = new Decimal(450.00) };

            var transactionRequest = new transactionRequestType
            {
                transactionType = transactionTypeEnum.authCaptureTransaction.ToString(),    // charge the card

                amount = amount,
                payment = paymentType,
                billTo = billingAddress,
                lineItems = lineItems
            };

            var request = new createTransactionRequest { transactionRequest = transactionRequest };

            // instantiate the contoller that will call the service
            var controller = new createTransactionController(request);
            controller.Execute();

            // get the response from the service (errors contained if any)
            var response = controller.GetApiResponse();

            if (response != null && response.messages.resultCode == messageTypeEnum.Ok)
            {
                if (response.transactionResponse != null)
                {
                    Console.WriteLine("Success, Auth Code : " + response.transactionResponse.authCode);
                }
            }
            else if (response != null)
            {
                Console.WriteLine("Error: " + response.messages.message[0].code + "  " + response.messages.message[0].text);
                if (response.transactionResponse != null)
                {
                    Console.WriteLine("Transaction Error : " + response.transactionResponse.errors[0].errorCode + " " + response.transactionResponse.errors[0].errorText);
                }
            }

            return response;
        }
    }
}
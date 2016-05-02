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
using System.Data.Entity;

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
        private void LoadOrderModel(OrderViewModel model)
        {
            model.Years = new List<string>();
            for (int i = DateTime.Today.Year; i <= DateTime.Today.Year + 10; i++)
            {
                model.Years.Add(i.ToString());
            }

            if (model.EventInstanceId != null)
            {
                model.EventInstance = DataContext.EventInstances.Include("Event").Include("Event.Tickets").Include("Event.Place").Single(e => e.Id == (int)model.EventInstanceId);

                var tickets = new List<Ticket>();
                if (model.EventInstance.Event.Tickets.Count() != 0)
                {
                    tickets = model.EventInstance.Event.Tickets.ToList();
                }
                else
                {
                    tickets =
                            (from i in DataContext.EventInstances
                             join c in DataContext.Classes
                             on i.EventId equals c.Id
                             join t in DataContext.Tickets
                             on c.SchoolId equals t.SchoolId
                             where i.Id == model.EventInstanceId
                             select t).Distinct().ToList();
                }
                model.Tickets = tickets;
                model.Type = model.EventInstance.Event is Class ? Enums.EventType.Class : Enums.EventType.Social;
            }
            else
            {
                var tickets = new List<Ticket>();
                tickets = DataContext.Tickets.Where(t => t.SchoolId == model.SchoolId).ToList();

                model.Tickets = tickets;
                model.School = DataContext.Schools.Single(s => s.Id == model.SchoolId);
            }
        }

        [Authorize]
        public ActionResult BuyTicket(int? instanceId, int? schoolId)
        {
            var model = new OrderViewModel();
            model.Order = new Order();
            model.EventInstanceId = instanceId;
            model.SchoolId = schoolId;
            LoadOrderModel(model);
            return View(model);
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

        // GET: School
        [Authorize(Roles = "Teacher,Owner")]
        public ActionResult Confirmation(int id)
        {
            var model = new ConfirmationViewModel();
            model.Order = DataContext.Orders
                                .Include("OrderDetails.Ticket.School")
                                .Include("EventInstance.Place")
                                .Include("User")
                                .Where(o => o.Id == id).FirstOrDefault();
            return View(model);
        }

        //[HttpPost]
        //public ActionResult PostTransaction(OrderViewModel model)
        //{
        //    model.Order.OrderDetails.Add(new OrderDetail() { TicketId = model.TicketId, Quantity = model.Quantity });
        //    Run(model);
        //    return RedirectToAction("Test3", "Home");
        //}

        [Authorize]
        [HttpPost]
        public ActionResult BuyTicket(OrderViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userid = User.Identity.GetUserId();
                //  Create Order and Details
                model.Order.EventInstanceId = model.EventInstanceId;
                model.Order.UserId = userid;
                if (model.Order.Id != 0)
                {
                    DataContext.Entry(model.Order).State = EntityState.Modified;
                    DataContext.SaveChanges();
                    model.Order = DataContext.Orders.Include("OrderDetails.Ticket").Single(o => o.Id == model.Order.Id);
                }
                else
                {
                    DataContext.Orders.Add(model.Order);
                    DataContext.SaveChanges();
                    model.Order.OrderDetails = new List<OrderDetail>();
                    var ticket = DataContext.Tickets.Single(t => t.Id == model.TicketId);
                    model.Order.OrderDetails.Add(new OrderDetail() { Quantity = model.Quantity, TicketId = model.TicketId, UnitPrice = ticket.Price });
                    DataContext.SaveChanges();
                }
                //  Create Order and Details

                //  Post Transaction
                if (PostTransaction(model).messages.resultCode == messageTypeEnum.Ok)
                {
                    //  Create User Ticket record
                    var user = DataContext.Users.Single(u => u.Id == userid);
                    var uticket = new UserTicket() { UserId = userid, TicketId = model.TicketId, Quantity = model.Quantity };
                    DataContext.UserTickets.Add(uticket);
                    DataContext.SaveChanges();
                    //  Create User Ticket record

                    //  Register User for Event
                    if (model.EventInstance != null)
                    {
                        DataContext.EventRegistrations.Add(new EventRegistration() { UserId = userid, EventInstanceId = (int)model.EventInstanceId, UserTicketId = uticket.Id });
                        DataContext.SaveChanges();
                    }
                    //  Register User for Event
                    return RedirectToAction("Confirmation", "Store", new { id = model.Order.Id });
                }
                else
                {
                    DataContext.SaveChanges();
                    LoadOrderModel(model);
                    return View(model);
                }
                //  Post Transaction

            }
            else
            {
                LoadOrderModel(model);
                return View(model);
            }
        }

        [Authorize]
        public static ANetApiResponse PostTransaction(OrderViewModel model)
        {
            //  Console.WriteLine("Charge Credit Card Sample");

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
                cardNumber = model.CCNumber,  //  "4111111111111111",
                expirationDate = model.CCMonth + model.CCYear.Substring(2,2), //   "0718",
                cardCode = model.SecurityCode //   "123"
            };

            var billingAddress = new customerAddressType
            {
                firstName = model.Order.FirstName,  //  "John",
                lastName = model.Order.LastName, //    "Doe",
                address = model.Order.Address, //  "123 My St",
                city = model.Order.City, //  "OurTown",
                zip = model.Order.PostalCode, //  "98004"
            };

            //standard api call to retrieve response
            var paymentType = new paymentType { Item = creditCard };

            // Add line Items
            var lineItems = new List<lineItemType>();
            foreach(var i in model.Order.OrderDetails)
            {
                lineItems.Add( new lineItemType { itemId = i.Id.ToString(), name = i.Ticket.Quantity.ToString() + " tickets for $" + i.Ticket.Price.ToString(), quantity = i.Quantity, unitPrice = i.Ticket.Price });
            }
            //lineItems[0] = new lineItemType { itemId = "1", name = "t-shirt", quantity = 2, unitPrice = new Decimal(15.00) };
            //lineItems[1] = new lineItemType { itemId = "2", name = "snowboard", quantity = 1, unitPrice = new Decimal(450.00) };

            var transactionRequest = new transactionRequestType
            {
                transactionType = transactionTypeEnum.authCaptureTransaction.ToString(),    // charge the card

                amount = model.Order.OrderDetails.Sum(i => i.Ticket.Price),
                payment = paymentType,
                billTo = billingAddress,
                lineItems = lineItems.ToArray()
            };

            var request = new createTransactionRequest { transactionRequest = transactionRequest };

            // instantiate the contoller that will call the service
            var controller = new createTransactionController(request);
            controller.Execute();

            // get the response from the service (errors contained if any)
            var response = controller.GetApiResponse();

            model.Order.OrderTransactions = new List<OrderTransaction>();
            if (response != null && response.messages.resultCode == messageTypeEnum.Ok)
            {
                if (response.transactionResponse != null)
                {
                    model.Order.OrderTransactions.Add(new OrderTransaction() { Success = true, ResponseCode = response.messages.message[0].code, ResponseMessage = response.messages.message[0].text });
                    //  Console.WriteLine("Success, Auth Code : " + response.transactionResponse.authCode);
                }
            }
            else if (response != null)
            {
                // Console.WriteLine("Error: " + response.messages.message[0].code + "  " + response.messages.message[0].text);
                if (response.transactionResponse != null)
                {
                    model.Result = response.messages.message[0].code;
                    model.Message = response.transactionResponse.errors[0].errorText;
                    model.Order.OrderTransactions.Add(new OrderTransaction() { Success = false, ResponseCode = response.messages.message[0].code, ResponseMessage = response.transactionResponse.errors[0].errorText });
                    //  Console.WriteLine("Transaction Error : " + response.transactionResponse.errors[0].errorCode + " " + response.transactionResponse.errors[0].errorText);
                }
            }

            var context = new Data.ApplicationDbContext();
            context.SaveChanges();

            return response;
        }
    }
}
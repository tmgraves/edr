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
using System.Globalization;
using EDR.Enums;

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
            var userid = User.Identity.GetUserId();
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
                    tickets = model.EventInstance.Event.Tickets.Where(t => t.Limit == null || (t.UserTickets != null && t.UserTickets.Where(ut => ut.UserId == userid).Count() < t.Limit)).ToList();
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
                             && (t.Limit == null || (t.UserTickets != null && t.UserTickets.Where(ut => ut.UserId == userid).Count() < t.Limit))
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

        [Authorize]
        public ActionResult Attendees(int instanceId, int attendees)
        {
            var model = new AttendeesViewModel();

            model.EventInstance = DataContext.EventInstances.Include("EventRegistrations").Where(i => i.Id == instanceId).FirstOrDefault();

            var userid = User.Identity.GetUserId();
            var user = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();
            model.Attendees = new List<Attendee>();

            for (int i = 0; i < attendees; i++)
            {
                if (model.EventInstance.EventRegistrations.Where(r => r.UserId == userid).Count() == 0 && i == 0)
                {
                    model.Attendees.Add(new Attendee() { UserId = userid, FirstName = user.FirstName, LastName = user.LastName });
                }
                else
                {
                    model.Attendees.Add(new Attendee());
                }
            }
            return View(model);
        }

        [Authorize]
        [HttpPost]
        public ActionResult Attendees(AttendeesViewModel attendeemodel)
        {
            foreach (var a in attendeemodel.Attendees.Where(a => a.UserId == null))
            {
                a.FirstName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(a.FirstName);
                a.LastName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(a.LastName);
            }

            var userid = User.Identity.GetUserId();
            var user = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();
            var instance = DataContext.EventInstances.Where(i => i.Id == attendeemodel.EventInstance.Id).FirstOrDefault();
            var tix = DataContext.UserTickets
                            .Include("EventRegistrations")
                            .Where(u => u.UserId == userid 
                                    && u.Ticket.EventId == instance.EventId 
                                    && u.Quantity > u.EventRegistrations.Where(r => r.EventInstanceId == instance.Id).Count()
                                    && (u.Ticket.Quantity * u.Quantity > u.EventRegistrations.Count()))
                            .ToList()
                            .Select(u => new { UserTicketId = u.Id, Remaining = u.Quantity - u.EventRegistrations.Where(r => r.EventInstanceId == instance.Id).Count() });
            //  School tickets?
            if (tix.Count() == 0)
            {
                tix = DataContext.UserTickets
                            .Include("EventRegistrations")
                            .Where(u => u.UserId == userid 
                                    && u.Ticket.SchoolId == ((Class)instance.Event).SchoolId
                                    && u.Quantity > u.EventRegistrations.Where(r => r.EventInstanceId == instance.Id).Count()
                                    && (u.Ticket.Quantity * u.Quantity > u.EventRegistrations.Count()))
                            .ToList()
                            .Select(u => new { UserTicketId = u.Id, Remaining = u.Quantity - u.EventRegistrations.Where(r => r.EventInstanceId == instance.Id).Count() });
            }

            var curtic = tix.FirstOrDefault();
            var ids = new System.Collections.ArrayList();
            ids.Add(curtic.UserTicketId);
            var rem = curtic.Remaining;
            foreach (var a in attendeemodel.Attendees)
            {
                if (a.FirstName != null)
                {
                    a.FirstName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(a.FirstName);
                }
                if (a.LastName != null)
                {
                    a.LastName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(a.LastName);
                }

                if (rem == 0)
                {
                    curtic = tix.Where(t => !ids.Contains(t.UserTicketId)).FirstOrDefault();
                    ids.Add(curtic.UserTicketId);
                    rem = curtic.Remaining;
                }

                if (rem >= 1)
                {
                    RegisterDancer(userid, attendeemodel.EventInstance.Id, curtic.UserTicketId, a.UserId == null ? a.FirstName : user.FirstName, a.UserId == null ? a.LastName : user.LastName);
                    //  DataContext.EventRegistrations.Add(new EventRegistration() { UserId = userid, EventInstanceId = attendeemodel.EventInstance.Id, UserTicketId = curtic.UserTicketId, FirstName = a.UserId == null ? a.FirstName : user.FirstName, LastName = a.UserId == null ? a.LastName : user.LastName });
                    //  DataContext.SaveChanges();
                    rem = rem - 1;
                }
            }

            return RedirectToAction("View", "Event", new { id = instance.EventId, eventtype = instance.Event is Class ? EDR.Enums.EventType.Class : EDR.Enums.EventType.Social, instanceId = attendeemodel.EventInstance.Id });
        }

        [Authorize]
        public void RegisterDancer(string userid, int instanceid, int? userTicketId, string firstName, string lastName)
        {
            var registration = new EventRegistration() { UserId = userid, UserTicketId = userTicketId, EventInstanceId = instanceid, FirstName = firstName, LastName = lastName };
            DataContext.EventRegistrations.Add(registration);
            DataContext.SaveChanges();
            EmailProcess.NewEventRegistration(registration.Id);
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

        [Authorize]
        public PartialViewResult GetAttendeeRowPartial()
        {
            return PartialView("~/Views/Shared/EditorTemplates/EventRegistration.cshtml", new EventRegistration());
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
            try
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
                        model.Order = DataContext.Orders.Include("OrderDetails").Single(o => o.Id == model.Order.Id);
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
                        var now = DateTime.Now;
                        //  Create Financial Record
                        var tran = new FinancialTransaction() { Amount = model.Order.OrderDetails.Sum(i => i.Ticket.Price * i.Quantity), OrderId = model.Order.Id, OrderTransactionId = model.Order.OrderTransactionId, PaymentType = PaymentType.CC, TranType = "Purchase Order", Committed = now };
                        var feetran = new FinancialTransaction() { Amount = -tran.Amount * (decimal)GlobalVariables.TicketRate, OrderId = model.Order.Id, PaymentType = PaymentType.Internal, TranType = "Transaction Fee", Committed = now };
                        if (model.SchoolId != null)
                        {
                            tran.SchoolId = model.SchoolId;
                            feetran.SchoolId = model.SchoolId;
                        }
                        else
                        {
                            var evnt = DataContext.Events.Single(e => e.Id == model.EventInstance.EventId);
                            if (evnt is Class)
                            {
                                tran.SchoolId = DataContext.Classes.Single(c => c.Id == model.EventInstance.EventId).SchoolId;
                                feetran.SchoolId = DataContext.Classes.Single(c => c.Id == model.EventInstance.EventId).SchoolId;
                            }
                            else
                            {
                                tran.PromoterGroupId = DataContext.Socials.Single(c => c.Id == model.EventInstance.EventId).PromoterGroupId;
                                feetran.PromoterGroupId = DataContext.Socials.Single(c => c.Id == model.EventInstance.EventId).PromoterGroupId;
                            }
                        }
                        DataContext.FinancialTransactions.Add(tran);
                        DataContext.FinancialTransactions.Add(feetran);
                        //  Create Financial Record

                        //  Create User Ticket record
                        var user = DataContext.Users.Single(u => u.Id == userid);
                        var uticket = new UserTicket() { UserId = userid, TicketId = model.TicketId, Quantity = model.Quantity };
                        DataContext.UserTickets.Add(uticket);
                        DataContext.SaveChanges();
                        //  Create User Ticket record

                        //  Event Registration
                        if (model.EventInstanceId != null)
                        {
                            RegisterDancer(userid, (int)model.EventInstanceId, uticket.Id, user.FirstName, user.LastName);
                            //DataContext.EventRegistrations.Add(new EventRegistration() { UserId = userid, EventInstanceId = (int)model.EventInstanceId, UserTicketId = uticket.Id, FirstName = user.FirstName, LastName = user.LastName });
                            //DataContext.SaveChanges();

                            //  Register User for Event
                            return RedirectToAction("Confirmation", "Store", new { id = model.Order.Id });
                        }
                        //  School Ticket
                        else
                        {
                            return RedirectToAction("Confirmation", "Store", new { id = model.Order.Id });
                        }
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
            catch(Exception ex)
            {
                ViewBag.errorMessage = ex.Message;
                return View("Error");
            }
        }

        //  School Ticket?
        ////  Event Ticket?
        //else
        //{
        //    var tix = DataContext.UserTickets.Include("EventRegistrations").Where(u => u.UserId == userid && u.Ticket.EventId == model.EventInstance.EventId);
        //    var avail = tix.Sum(t => t.Ticket.Quantity * t.Quantity) - tix.Sum(t => t.EventRegistrations.Count());

        //    if (avail > 1)
        //    {
        //        DataContext.EventRegistrations.Add(new EventRegistration() { UserId = userid, EventInstanceId = (int)model.EventInstanceId, UserTicketId = uticket.Id });
        //        DataContext.SaveChanges();

        //        //  Go To Attendees Screen
        //        return RedirectToAction("Attendees", new { orderid = model.Order.Id });
        //    }
        //    else
        //    {
        //        DataContext.EventRegistrations.Add(new EventRegistration() { UserId = userid, EventInstanceId = (int)model.EventInstanceId, UserTicketId = uticket.Id });
        //        DataContext.SaveChanges();

        //        //  Register User for Event
        //        return RedirectToAction("Confirmation", "Store", new { id = model.Order.Id });
        //    }
        //}
        //  Event Ticket?

        [Authorize]
        public static ANetApiResponse PostTransaction(OrderViewModel model)
        {
            //  Console.WriteLine("Charge Credit Card Sample");

            if (ConfigurationManager.AppSettings["AuthorizeEnvironment"] == "Production")
            {
                ApiOperationBase<ANetApiRequest, ANetApiResponse>.RunEnvironment = AuthorizeNet.Environment.PRODUCTION;
            }
            else
            {
                ApiOperationBase<ANetApiRequest, ANetApiResponse>.RunEnvironment = AuthorizeNet.Environment.SANDBOX;
            }

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

            var order = new orderType { invoiceNumber = model.Order.Id.ToString(), description = "Ticket Order" };

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

                amount = model.Order.OrderDetails.Sum(i => i.Ticket.Price * i.Quantity),
                payment = paymentType,
                billTo = billingAddress,
                lineItems = lineItems.ToArray(),
                order = order,
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
                    model.Order.OrderTransactionId = response.transactionResponse.transId;
                    model.Order.OrderTransactions.Add(new OrderTransaction() { Success = true, ResponseCode = response.messages.message[0].code, ResponseMessage = response.messages.message[0].text, TransactionId = response.transactionResponse.transId });
                    //  Console.WriteLine("Success, Auth Code : " + response.transactionResponse.authCode);
                    //  Send a Confirmation
                    EmailProcess.OrderConfirmation(model.Order.Id);
                }
            }
            else if (response != null)
            {
                // Console.WriteLine("Error: " + response.messages.message[0].code + "  " + response.messages.message[0].text);
                if (response.transactionResponse != null)
                {
                    model.Result = response.messages.message[0].code;
                    model.Message = response.messages.message[0].text;
                    model.Order.OrderTransactions.Add(new OrderTransaction() { Success = false, ResponseCode = response.messages.message[0].code, ResponseMessage = response.messages.message[0].text });
                    //  Console.WriteLine("Transaction Error : " + response.transactionResponse.errors[0].errorCode + " " + response.transactionResponse.errors[0].errorText);
                }
            }

            var context = new Data.ApplicationDbContext();
            context.SaveChanges();

            return response;
        }
    }
}
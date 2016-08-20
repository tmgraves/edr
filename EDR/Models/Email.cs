using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDR.Data;
using System.Configuration;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;

namespace EDR.Models
{
    public class ForgotPasswordEmail
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public string ResetLink { get; set; }
    }

    public class Email : Entity
    {
        [Required(ErrorMessage = "Contact Email is required")]
        [Display(Name = "Contact Email")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string ToEmail { get; set; }
        [Display(Name = "Email Display")]
        public string Subject { get; set; }
        public string Body { get; set; }
        private DateTime _date = DateTime.Now;
        [Display(Name = "Email Date")]
        public DateTime EmailDate
        {
            get { return _date; }
            set { _date = value; }
        }
        public DateTime? Sent { get; set; }
        public string Status { get; set; }
    }

    public partial class EmailProcess
    {
        public static string OrderConfirmation(int orderId)
        {
            try
            {
                ApplicationDbContext context = new ApplicationDbContext();
                var requestContext = HttpContext.Current.Request.RequestContext;

                var order = context.Orders.Include("OrderDetails").Where(o => o.Id == orderId).FirstOrDefault();

                var orderdetails = "<table>" +
                                "<thead>" +
                                    "<tr>" +
                                        "<th>Event/School</th>" +
                                        "<th>Ticket Description</th>" +
                                        "<th>Quantity</th>" +
                                        "<th>Amount</th>" +
                                    "</tr>" +
                                "</thead>";

                foreach (var d in order.OrderDetails)
                {
                    orderdetails += "<tr><td>" + (d.Ticket.School != null ? d.Ticket.School.Name : d.Ticket.Event.Name) + "</td><td>" + d.Ticket.Quantity + " credits for " + d.Ticket.Price.ToString("C") + "</td><td>" + d.Quantity.ToString("N0") + "</td><td>" + (d.UnitPrice * d.Quantity).ToString("C") + "</td></tr>";
                }
                orderdetails += "<tfoot>" +
                            "<tr>" +
                                "<td colspan='3'>" +
                                    "Total:" +
                                "</td>" +
                                "<td>" +
                                    order.OrderDetails.Sum(d => d.Quantity * d.UnitPrice).ToString("C") +
                                "</td>" +
                            "</tr>" +
                        "</tfoot>";
                orderdetails += "</table>";

                var dancerpage = new UrlHelper(requestContext).Action("Manage", "Dancer", new { Area = "" }, protocol: HttpContext.Current.Request.Url.Scheme);
                var messagebody = LoadTemplate("OrderConfirmation.txt", order.User.FullName, orderdetails, dancerpage, dancerpage, order.Id, order.OrderDate.ToString());
                context.Emails.Add(new Email() { ToEmail = order.User.Email, Subject = "Thank you for your Order", Body = messagebody });
                //  var result = LoadTemplateSend("NewRoleUser.txt", user.Email, "You've been added to the " + role + " role", user.FullName, role, rolepage, rolepage );
                context.SaveChanges();

                return "success";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        //  Finished 8/6/16
        public static string SendConfirmEmails()
        {
            try
            {
                ApplicationDbContext context = new ApplicationDbContext();
                //  var helper = new UrlHelper(HttpContext.Current.Request.RequestContext);
                var date = DateTime.Today.AddDays(7);

                var requestContext = HttpContext.Current.Request.RequestContext;
                //  new UrlHelper(requestContext).Action("MainPage", "Index");

                //  Change >= to == for launch
                var classes = context.Classes.Include("Teachers").Where(e => e.EventInstances.Any(i => i.DateTime == date)).Select(c => new { Class = c, Instance = c.EventInstances.Where(i => i.DateTime == date).OrderBy(ei => ei.DateTime).FirstOrDefault() }).ToList();
                foreach (var c in classes)
                {
                    if (c.Class.Teachers != null)
                    {
                        foreach (var t in c.Class.Teachers)
                        {
                            var confirmstring = new UrlHelper(requestContext).Action("ConfirmInstance", "Event", new { id = c.Instance.Id, eventType = EDR.Enums.EventType.Class, Area = "" }, protocol: HttpContext.Current.Request.Url.Scheme);
                            var cancelstring = new UrlHelper(requestContext).Action("CancelInstance", "Event", new { id = c.Instance.Id, eventType = EDR.Enums.EventType.Class, Area = "" }, protocol: HttpContext.Current.Request.Url.Scheme);
                            var messagebody = LoadTemplate("ConfirmEventInstance.txt", t.ApplicationUser.FullName, "Class", c.Instance.DateTime.ToLongDateString(), confirmstring, cancelstring, confirmstring, cancelstring);
                            context.Emails.Add(new Email() { ToEmail = t.ApplicationUser.Email, Subject = "Confirm your Class", Body = messagebody });
                            //  var result = LoadTemplateSend("ConfirmEventInstance.txt", t.ApplicationUser.Email, "Confirm your Class", t.ApplicationUser.FullName, "Class", c.Instance.DateTime.ToLongDateString(), confirmstring, cancelstring, confirmstring, cancelstring);
                        }
                        context.SaveChanges();
                    }
                }

                var socials = context.Socials.Include("Promoters").Where(e => e.EventInstances.Any(i => i.DateTime == date)).Select(s => new { Social = s, Instance = s.EventInstances.Where(i => i.DateTime == date).OrderBy(ei => ei.DateTime).FirstOrDefault() }).ToList();
                foreach (var s in socials)
                {
                    if (s.Social.Promoters != null)
                    {
                        foreach (var p in s.Social.Promoters)
                        {
                            var confirmstring = new UrlHelper(requestContext).Action("ConfirmInstance", "Event", new { id = s.Instance.Id, eventType = EDR.Enums.EventType.Social, Area = "" }, protocol: HttpContext.Current.Request.Url.Scheme);
                            var cancelstring = new UrlHelper(requestContext).Action("CancelInstance", "Event", new { id = s.Instance.Id, eventType = EDR.Enums.EventType.Social, Area = "" }, protocol: HttpContext.Current.Request.Url.Scheme);
                            var messagebody = LoadTemplate("ConfirmEventInstance.txt", p.ApplicationUser.FullName, "Class", s.Instance.DateTime.ToLongDateString(), confirmstring, cancelstring, confirmstring, cancelstring);
                            context.Emails.Add(new Email() { ToEmail = p.ApplicationUser.Email, Subject = "Confirm your Class", Body = messagebody });
                            //  var result = LoadTemplateSend("ConfirmEventInstance.txt", p.ApplicationUser.Email, "Confirm your Dance Event", p.ApplicationUser.FullName, "Social", s.Instance.DateTime.ToLongDateString(), confirmstring, cancelstring, confirmstring, cancelstring);
                        }
                        context.SaveChanges();
                    }
                }

                return "success";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        //  Finished 8/6/16
        public static string NewRoleuser(string id, string role)
        {
            try
            {
                ApplicationDbContext context = new ApplicationDbContext();
                var requestContext = HttpContext.Current.Request.RequestContext;

                var user = context.Users.Single(u => u.Id == id);

                var rolepage = new UrlHelper(requestContext).Action("Manage", role, new { Area = "" }, protocol: HttpContext.Current.Request.Url.Scheme);
                var messagebody = LoadTemplate("NewRoleUser.txt", user.FullName, role, rolepage, rolepage);
                context.Emails.Add(new Email() { ToEmail = user.Email, Subject = "You've been added to the " + role + " role", Body = messagebody });
                //  var result = LoadTemplateSend("NewRoleUser.txt", user.Email, "You've been added to the " + role + " role", user.FullName, role, rolepage, rolepage );
                context.SaveChanges();

                return "success";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        //  Finished 8/6/16
        public static string NewEventRegistration(int id)
        {
            try
            {
                ApplicationDbContext context = new ApplicationDbContext();
                var requestContext = HttpContext.Current.Request.RequestContext;

                var registration = context.EventRegistrations.Single(r => r.Id == id);

                if (registration.Instance.Event is Class)
                {
                    var teachers = context.Teachers.Where(t => t.Classes.Any(c => c.Id == registration.Instance.EventId)).ToList();
                    var skilllevel = string.Format("<p><b>Skill Level:</b> {0} years</ p>", registration.User.Experience.ToString());

                    foreach (var t in teachers)
                    {
                        var messagebody = LoadTemplate("NewEventRegistration.txt", t.ApplicationUser.FirstName, registration.Instance.Event.Name, registration.Instance.DateTime.ToLongDateString(), registration.User.FullName, skilllevel);
                        context.Emails.Add(new Email() { ToEmail = t.ContactEmail, Subject = "You have a new registration for " + registration.Instance.Event.Name, Body = messagebody });
                        //  var result = LoadTemplateSend("NewEventRegistration.txt", t.ContactEmail, "You have a new registration for " + registration.Instance.Event.Name, t.ApplicationUser.FirstName, registration.Instance.Event.Name, registration.Instance.DateTime.ToLongDateString(), registration.User.FullName, skilllevel);
                    }
                    context.SaveChanges();
                }
                else
                {
                    var promoters = context.Promoters.Where(t => t.Socials.Any(c => c.Id == registration.Instance.EventId)).ToList();
                    var skilllevel = "";

                    foreach (var p in promoters)
                    {
                        var messagebody = LoadTemplate("NewEventRegistration.txt", p.ApplicationUser.FirstName, registration.Instance.Event.Name, registration.Instance.DateTime.ToLongDateString(), registration.User.FullName, skilllevel);
                        context.Emails.Add(new Email() { ToEmail = p.ContactEmail, Subject = "You have a new registration for " + registration.Instance.Event.Name, Body = messagebody });
                        //  var result = LoadTemplateSend("NewEventRegistration.txt", p.ContactEmail, "You have a new registration for " + registration.Instance.Event.Name, p.ApplicationUser.FirstName, registration.Instance.Event.Name, registration.Instance.DateTime.ToLongDateString(), registration.FirstName + " " + registration.LastName, skilllevel);
                    }
                    context.SaveChanges();
                }

                return "success";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        //  DAILY SCHEDULED
        public static string ExpiringEvents()
        {
            try
            {
                ApplicationDbContext context = new ApplicationDbContext();
                var date = DateTime.Today.AddDays(7);

                var requestContext = HttpContext.Current.Request.RequestContext;

                var classes = context.Classes.Where(e => e.EventInstances.Max(i => i.DateTime) == date && e.Recurring).ToList();
                foreach (var c in classes)
                {
                    if (c.Teachers != null)
                    {
                        foreach (var t in c.Teachers)
                        {
                            var eventstring = new UrlHelper(requestContext).Action("Manage", "Event", new { id = c.Id, eventType = EDR.Enums.EventType.Class, Area = "" }, protocol: HttpContext.Current.Request.Url.Scheme);
                            var messagebody = LoadTemplate("ExpiringEvent.txt", t.ApplicationUser.FullName, "Class", c.Name, c.EventInstances.Max(i => i.DateTime).ToLongDateString(), eventstring, eventstring);
                            context.Emails.Add(new Email() { ToEmail = t.ApplicationUser.Email, Subject = "Your Class is about to Expire", Body = messagebody });
                            //  var result = LoadTemplateSend("ExpiringEvent.txt", t.ApplicationUser.Email, "Your Class is about to Expire", t.ApplicationUser.FullName, "Class", c.Name, c.EventInstances.Max(i => i.DateTime).ToLongDateString(), eventstring, eventstring);
                        }
                        context.SaveChanges();
                    }
                }

                var socials = context.Socials.Where(e => e.EventInstances.Max(i => i.DateTime) == date && e.Recurring).ToList();
                foreach (var s in socials)
                {
                    if (s.Promoters != null)
                    {
                        foreach (var p in s.Promoters)
                        {
                            var eventstring = new UrlHelper(requestContext).Action("Manage", "Event", new { id = s.Id, eventType = EDR.Enums.EventType.Social, Area = "" }, protocol: HttpContext.Current.Request.Url.Scheme);
                            var messagebody = LoadTemplate("ExpiringEvent.txt", p.ApplicationUser.FullName, "Social", s.Name, s.EventInstances.Max(i => i.DateTime).ToLongDateString(), eventstring, eventstring);
                            context.Emails.Add(new Email() { ToEmail = p.ApplicationUser.Email, Subject = "Your Social is about to Expire", Body = messagebody });
                            //  var result = LoadTemplateSend("ExpiringEvent.txt", p.ApplicationUser.Email, "Your Social is about to Expire", p.ApplicationUser.FullName, "Social", s.Name, s.EventInstances.Max(i => i.DateTime).ToLongDateString(), eventstring, eventstring);
                        }
                        context.SaveChanges();
                    }
                }

                var rehearsals = context.Rehearsals.Where(e => e.EventInstances.Max(i => i.DateTime) == date).ToList();
                foreach (var r in rehearsals)
                {
                    if (r.Team.Teachers != null)
                    {
                        foreach (var t in r.Team.Teachers)
                        {
                            var eventstring = new UrlHelper(requestContext).Action("Manage", "Team", new { id = r.TeamId, Area = "" }, protocol: HttpContext.Current.Request.Url.Scheme);
                            var messagebody = LoadTemplate("ExpiringEvent.txt", t.ApplicationUser.FullName, "Rehearsal", "for " + r.Team.Name, r.EventInstances.Max(i => i.DateTime).ToLongDateString(), eventstring, eventstring);
                            context.Emails.Add(new Email() { ToEmail = t.ApplicationUser.Email, Subject = "Your Rehearsal is about to Expire", Body = messagebody });
                            //  var result = LoadTemplateSend("ExpiringEvent.txt", t.ApplicationUser.Email, "Your Rehearsal is about to Expire", t.ApplicationUser.FullName, "Rehearsal", "for " + r.Team.Name, r.EventInstances.Max(i => i.DateTime).ToLongDateString(), eventstring, eventstring);
                        }
                        context.SaveChanges();
                    }
                }

                return "success";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        //  DAILY SCHEDULED
        public static string DailyDancerSummaries()
        {
            try
            {
                ApplicationDbContext context = new ApplicationDbContext();
                var requestContext = HttpContext.Current.Request.RequestContext;
                var today = DateTime.Today;

                var users = context.Users.Include("EventRegistrations").Where(u => u.EventRegistrations.Any(r => r.Instance.DateTime == today)).ToList();

                foreach(var u in users)
                {
                    var evntlist = "<table>" +
                                    "<thead>" +
                                        "<tr>" +
                                            "<th>Event</th>" +
                                            "<th>Date/Time</th>" +
                                            "<th>Place</th>" +
                                        "</tr>" +
                                    "</thead>";

                    foreach (var reg in u.EventRegistrations.Where(r => r.Instance.DateTime == today))
                    {
                        evntlist += "<tr><td>" + reg.Instance.Event.Name + "</td><td>" + reg.Instance.DateTime.ToLongDateString() + " " + ((DateTime)reg.Instance.StartTime).ToLongTimeString() + "</td><td>" + reg.Instance.Event.Place.Name + "<br/>" + reg.Instance.Event.Place.Address + "<br/>" + reg.Instance.Place.City + ", " + reg.Instance.Place.State + " " + reg.Instance.Place.Zip + "</td></tr>";
                    }

                    evntlist += "</table>";

                    var userpage = new UrlHelper(requestContext).Action("Manage", "Dancer", new { Area = "" }, protocol: HttpContext.Current.Request.Url.Scheme);
                    var messagebody = LoadTemplate("DailyDancerSummary.txt", u.FullName, today.ToLongDateString(), evntlist, userpage, userpage);
                    context.Emails.Add(new Email() { ToEmail = u.Email, Subject = "Your Daily Events", Body = messagebody });
                }

                context.SaveChanges();

                return "success";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        //  DAILY SCHEDULED
        public static string EventSummaries()
        {
            try
            {
                ApplicationDbContext context = new ApplicationDbContext();
                var requestContext = HttpContext.Current.Request.RequestContext;
                var date = DateTime.Today;

                var instances =
                    (from i in context.EventInstances.Include("EventRegistrations")
                    join c in context.Classes
                    on i.EventId equals c.Id
                    into cres
                    from cls in cres.DefaultIfEmpty()
                    join s in context.Socials
                    on i.EventId equals s.Id
                    into sres
                    from soc in sres.DefaultIfEmpty()
                    where i.DateTime <= DateTime.Today
                    select new
                    {
                        i,
                        Registrations=i.EventRegistrations,
                        cls,
                        soc
                    }).ToList();

                //  Get Classes
                foreach (var c in instances.Where(i => i.cls != null))
                {
                    if (c.cls.Teachers != null)
                    {
                        foreach (var t in c.cls.Teachers)
                        {
                            var eventstring = new UrlHelper(requestContext).Action("Manage", "Event", new { id = c.cls.Id, eventType = EDR.Enums.EventType.Class, Area = "" }, protocol: HttpContext.Current.Request.Url.Scheme);

                            var attendees = "<table>" +
                                            "<thead>" +
                                                "<tr>" +
                                                    "<th>Attendee Name</th>" +
                                                    "<th>Experience</th>" +
                                                "</tr>" +
                                            "</thead>";

                            foreach (var r in c.Registrations)
                            {
                                attendees += "<tr><td>" + r.FirstName + " " + r.LastName + "</td><td>" + (r.User != null ? r.User.Experience.ToString() : "0") + " years</td></tr>";
                            }
                            attendees += "</table>";

                            var messagebody = LoadTemplate("EventSummary.txt", t.ApplicationUser.FullName, "Class", c.cls.Name, c.i.DateTime.ToLongDateString(), attendees, eventstring, eventstring);
                            context.Emails.Add(new Email() { ToEmail = t.ApplicationUser.Email, Subject = "Your Class Summary for Today", Body = messagebody });
                        }
                        context.SaveChanges();
                    }
                }

                //  Get Socials
                foreach (var soc in instances.Where(i => i.soc != null))
                {
                    if (soc.soc.Promoters != null)
                    {
                        foreach (var pro in soc.soc.Promoters)
                        {
                            var eventstring = new UrlHelper(requestContext).Action("Manage", "Event", new { id = soc.soc.Id, eventType = EDR.Enums.EventType.Social, Area = "" }, protocol: HttpContext.Current.Request.Url.Scheme);

                            var attendees = "<thead>" +
                                                "<tr>" +
                                                    "<th>Attendee Name</th>" +
                                                "</tr>" +
                                            "</thead>";
                            foreach (var r in soc.Registrations)
                            {
                                attendees += "<tr><td>" + r.FirstName + " " + r.LastName + "</td></tr>";
                            }
                            attendees += "</table>";

                            var messagebody = LoadTemplate("EventSummary.txt", pro.ApplicationUser.FullName, "Social", soc.soc.Name, soc.i.DateTime.ToLongDateString(), attendees, eventstring, eventstring);
                            context.Emails.Add(new Email() { ToEmail = pro.ApplicationUser.Email, Subject = "Your Social Summary for Today", Body = messagebody });
                        }
                        context.SaveChanges();
                    }
                }

                return "success";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public static void ProcessEmails()
        {
            ApplicationDbContext context = new ApplicationDbContext();

            var emails = context.Emails.Where(e => e.Sent == null).OrderBy(e => e.EmailDate).Take(50);

            string result = "";
            foreach (var e in emails)
            {
                try
                {
                    result = SendEmail(e);
                    e.Status = result;

                    if (result == "Success")
                    {
                        e.Sent = DateTime.Now;
                        context.Entry(e).State = EntityState.Modified;
                    }
                    else
                    {
                        context.Entry(e).State = EntityState.Modified;
                    }
                }
                catch (Exception ex)
                {
                    e.Status = ex.Message;
                    context.Entry(e).State = EntityState.Modified;
                }
            }
            context.SaveChanges();
        }

        //  Template to Load and Send Emails
        public static string LoadTemplate(string template, params object[] args)
        {
            ApplicationDbContext context = new ApplicationDbContext();

            string body;
            //Read template file from the App_Data folder
            using (var sr = new StreamReader(HttpContext.Current.Server.MapPath("\\App_Data\\Templates\\" + template)))
            {
                body = sr.ReadToEnd();
            }

            try
            {
                string messageBody = string.Format(body, args);

                return messageBody;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        //  Send Email Function
        public static string SendEmail(Email email)
        {
            try
            {
                var MailHelper = new EDR.Utilities.MailHelper
                {
                    //  Sender = sender, //email.Sender,
                    Recipient = email.ToEmail,
                    RecipientCC = null,
                    Subject = email.Subject,
                    Body = email.Body
                };
                return MailHelper.Send();
            }
            catch (Exception ex)
            {
                return "Failed: " + ex.Message;
            }
        }

        ////  Template to Load and Send Emails
        //public static bool LoadTemplateSend(string template, string email, string subject, params object[] args)
        //{
        //    string body;
        //    //Read template file from the App_Data folder
        //    using (var sr = new StreamReader(HttpContext.Current.Server.MapPath("\\App_Data\\Templates\\" + template)))
        //    {
        //        body = sr.ReadToEnd();
        //    }

        //    try
        //    {
        //        //add email logic here to email the customer that their invoice has been voided
        //        //Username: {1}
        //        //  string name = HttpUtility.UrlEncode(user.FullName);
        //        //  string formattedcallback = HttpUtility.UrlEncode(callbackUrl);
        //        //  string emailSubject = @"Eat. Dance. Repeat. - Reset Password";

        //        string messageBody = string.Format(body, args);
        //        //  string messageBody = string.Format(body, "test", "test");

        //        var MailHelper = new EDR.Utilities.MailHelper
        //        {
        //            //  Sender = sender, //email.Sender,
        //            Recipient = email,
        //            RecipientCC = null,
        //            Subject = subject,
        //            Body = messageBody
        //        };
        //        MailHelper.Send();

        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        return false;
        //    }
        //}
    }
    }

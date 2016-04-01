using EDR.Models;
using EDR.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;

namespace EDR.Controllers
{
    public class SchoolController : BaseController
    {
        // GET: School
        public ActionResult List()
        {
            var model = DataContext.Schools.ToList();
            return View(model);
        }

        // GET: School
        public ActionResult Create()
        {
            var model = new School();
            return View(model);
        }

        // GET: School
        [HttpPost]
        public ActionResult Create(School school)
        {
            if (ModelState.IsValid)
            {
                var model = new School();
                TryUpdateModel(model);

                var userid = User.Identity.GetUserId();

                model.Members = new List<OrganizationMember>();
                model.Members.Add(new OrganizationMember() { UserId = userid, Admin = true });

                //Save School
                DataContext.Schools.Add(model);
                DataContext.SaveChanges();

                return RedirectToAction("View", new { id = model.Id });
            }
            else
            {
                return View(school);
            }
        }

        // GET: School
        public ActionResult View(int id)
        {
            var model = DataContext.Schools
                        .Where(s => s.Id == id)
                        .Include("Members")
                        .Include("Members.User")
                        .Include("Classes")
                        .Include("Tickets")
                        .Include("Tickets.EventTickets")
                        .FirstOrDefault();
            return View(model);
        }

        // GET: School
        public ActionResult Delete(int id)
        {
            var model = DataContext.Schools.Where(s => s.Id == id).FirstOrDefault();
            return View(model);
        }

        // GET: School
        [HttpPost]
        public ActionResult Delete(School school)
        {
            //Save School
            DataContext.Schools.Remove(DataContext.Schools.Single(s => s.Id == school.Id));
            DataContext.SaveChanges();

            return RedirectToAction("List");
        }

        // GET: School
        public ActionResult Edit(int id)
        {
            var model = DataContext.Schools.Where(s => s.Id == id).FirstOrDefault();
            return View(model);
        }

        // GET: School
        [HttpPost]
        public ActionResult Edit(School school)
        {
            if (ModelState.IsValid)
            {
                //Save Order
                DataContext.Entry(school).State = EntityState.Modified;
                DataContext.SaveChanges();

                return RedirectToAction("View", new { id = school.Id });
            }
            else
            {
                return View(school);
            }
        }

        // GET: School
        public ActionResult AddTicket(int id)
        {
            var ticket = new Ticket();
            ticket.SchoolId = id;
            return View(ticket);
        }

        // POST: School
        [HttpPost]
        public ActionResult AddTicket(Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                //Save Ticket
                DataContext.Tickets.Add(ticket);
                DataContext.SaveChanges();

                return RedirectToAction("View", new { id = ticket.SchoolId });
            }
            else
            {
                return View(ticket);
            }
        }

        // GET: School
        public ActionResult ViewTicket(int id)
        {
            var model = new ViewTicketViewModel();
            model.Ticket = DataContext.Tickets
                                .Where(t => t.Id == id)
                                .Include("EventTickets")
                                .Include("EventTickets.Event")
                                .FirstOrDefault();
            //model.AvailableClasses = DataContext.Classes.Where(c => c.SchoolId == model.Ticket.SchoolId).Select(c => new ListItem() { Id = c.Id, Name = c.Name });
            //model.SelectedClasses = model.Ticket.EventTickets.Select(t => new ListItem() { Id = t.EventId, Name = t.Event.Name, IsSelected = true });
            var eventTickets = model.Ticket.EventTickets.Where(et => et.TicketId == id);
            var classes = DataContext.Classes.Where(c => c.SchoolId == model.Ticket.SchoolId);
            var eids = eventTickets.Select(et => et.EventId).ToArray();
            model.EventTickets = new List<EventTicketPlaceholder>();
            foreach(var c in classes)
            {
                model.EventTickets.Add(new EventTicketPlaceholder() { Connect = eids.Contains(c.Id), EventTicket = new EventTicket() { Event = c, TicketId = id } });
            }
            return View(model);
        }

        // POST: School
        [HttpPost]
        public ActionResult UpdateMembers(School model)
        {
            foreach(var m in model.Members)
            {
                var mem = DataContext.OrganizationMembers.Where(om => om.Id == m.Id).FirstOrDefault();
                mem.Admin = m.Admin;
            }
            DataContext.SaveChanges();
            return RedirectToAction("View", new { id = model.Id });
            //var members = school.Members;
            //var schoolId = members.FirstOrDefault().OrganizationId;
            //foreach(var mbr in members)
            //{
            //    var member = DataContext.OrganizationMembers.Single(m => m.Id == mbr.Id);
            //    member.Admin = mbr.Admin;
            //}

            //DataContext.SaveChanges();

            //return RedirectToAction("View", new { id = schoolId });
        }

        // POST: School
        [HttpPost]
        public ActionResult UpdateTickets(ViewTicketViewModel model)
        {
            DataContext.EventTickets.RemoveRange(DataContext.EventTickets.Where(et => et.TicketId == model.Ticket.Id));
            //  DataContext.Tickets.Where(t => t.Id == model.Ticket.Id).Include("EventTickets").FirstOrDefault().EventTickets.Clear();
            DataContext.EventTickets.AddRange(model.EventTickets.Where(et => et.Connect).Select(et => new EventTicket() { EventId = et.EventTicket.Event.Id, TicketId = model.Ticket.Id }));
            DataContext.SaveChanges();
            return RedirectToAction("ViewTicket", new { id = model.Ticket.Id });
            //var members = school.Members;
            //var schoolId = members.FirstOrDefault().OrganizationId;
            //foreach(var mbr in members)
            //{
            //    var member = DataContext.OrganizationMembers.Single(m => m.Id == mbr.Id);
            //    member.Admin = mbr.Admin;
            //}

            //DataContext.SaveChanges();

            //return RedirectToAction("View", new { id = schoolId });
        }
    }
}
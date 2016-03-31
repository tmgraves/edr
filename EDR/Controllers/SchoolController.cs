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
            model.AvailableClasses = DataContext.Classes.Where(c => c.SchoolId == model.Ticket.SchoolId).Select(c => new ListItem() { Id = c.Id, Name = c.Name });
            model.SelectedClasses = model.Ticket.EventTickets.Select(t => new ListItem() { Id = t.EventId, Name = t.Event.Name, IsSelected = true });
            return View(model);
        }

        // POST: School
        [HttpPost]
        public ActionResult UpdateMembers(FormCollection values)
        {
            var schoolId = values["Id"];
            var ids = values["userids"];

            return RedirectToAction("View", new { id = schoolId });
        }
    }
}
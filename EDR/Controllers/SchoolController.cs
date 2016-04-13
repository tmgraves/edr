using EDR.Models;
using EDR.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using EDR.Enums;

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
        [Authorize(Roles = "Teacher,Owner")]
        public ActionResult Create(RoleName? role = null)
        {
            var model = new CreateSchoolViewModel(role);
            return View(model);
        }

        // GET: School
        [HttpPost]
        [Authorize(Roles = "Teacher,Owner")]
        public ActionResult Create(CreateSchoolViewModel model)
        {
            if (ModelState.IsValid)
            {
                var school = new School();
                TryUpdateModel(school, "School");

                var userid = User.Identity.GetUserId();

                if (model.Role == RoleName.Teacher)
                {
                    var teacher = DataContext.Teachers.Where(t => t.ApplicationUser.Id == userid).FirstOrDefault();
                    if (teacher != null)
                    {
                        school.Teachers.Add(teacher);
                    }
                }

                if (model.Role == RoleName.Owner)
                {
                    var owner = DataContext.Owners.Where(o => o.ApplicationUser.Id == userid).FirstOrDefault();
                    if (owner != null)
                    {
                        school.Owners.Add(owner);
                    }
                }

                school.Members.Add(new OrganizationMember() { UserId = userid, Admin = true });

                //Save School
                DataContext.Schools.Add(school);
                DataContext.SaveChanges();

                return RedirectToAction("View", new { id = school.Id });
            }
            else
            {
                return View(model);
            }
        }

        // GET: School
        public ActionResult View(int id)
        {
            var userid = User.Identity.GetUserId();
            var model = new ViewSchoolViewModel(
                        DataContext.Schools
                        .Where(s => s.Id == id)
                        .Include("Members")
                        .Include("Members.User")
                        .Include("Classes")
                        .Include("Tickets")
                        .Include("Tickets.Event")
                        .Include("Owners")
                        .Include("Owners.ApplicationUser")
                        .Include("Teachers")
                        .Include("Teachers.ApplicationUser")
                        .FirstOrDefault());
            model.Member = DataContext.OrganizationMembers.Where(m => m.OrganizationId == id && m.UserId == userid && m.Admin).FirstOrDefault();
            return View(model);
        }

        [Authorize(Roles = "Teacher,Owner")]
        // GET: School
        public ActionResult Manage(int id)
        {
            var userid = User.Identity.GetUserId();
            var admin = DataContext.OrganizationMembers.Where(m => m.OrganizationId == id && m.UserId == userid && m.Admin).Count() != 0 ? true : false;
            if (admin)
            {
                var model = new ManageSchoolViewModel(
                            DataContext.Schools
                            .Where(s => s.Id == id)
                            .Include("Members")
                            .Include("Members.User")
                            .Include("Classes")
                            .Include("Tickets")
                            .Include("Tickets.Event")
                            .Include("Owners")
                            .Include("Owners.ApplicationUser")
                            .Include("Teachers")
                            .Include("Teachers.ApplicationUser")
                            .FirstOrDefault());
                return View(model);
            }
            else
            {
                return RedirectToAction("View", new { id = id });
            }
        }

        // GET: School
        [Authorize(Roles = "Teacher,Owner")]
        public ActionResult Delete(int id)
        {
            var model = DataContext.Schools.Where(s => s.Id == id).FirstOrDefault();
            return View(model);
        }

        // GET: School
        [Authorize(Roles = "Teacher,Owner")]
        [HttpPost]
        public ActionResult Delete(School school)
        {
            //Save School
            DataContext.Schools.Remove(DataContext.Schools.Single(s => s.Id == school.Id));
            DataContext.SaveChanges();

            return RedirectToAction("List");
        }

        // GET: School
        [Authorize(Roles = "Teacher,Owner")]
        public ActionResult Edit(int id)
        {
            var model = DataContext.Schools.Where(s => s.Id == id).FirstOrDefault();
            return View(model);
        }

        // GET: School
        [HttpPost]
        [Authorize(Roles = "Teacher,Owner")]
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
        [Authorize(Roles = "Teacher,Owner")]
        public ActionResult AddTicket(int id)
        {
            var ticket = new Ticket();
            ticket.SchoolId = id;
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

                return RedirectToAction("View", new { id = ticket.SchoolId });
            }
            else
            {
                return View(ticket);
            }
        }

        // POST: School
        [HttpPost]
        [Authorize(Roles = "Teacher,Owner")]
        public ActionResult UpdateMembers(School model)
        {
            foreach (var m in model.Members)
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

        public ActionResult Join(int id)
        {
            var userid = User.Identity.GetUserId();
            if (DataContext.OrganizationMembers.Where(m => m.OrganizationId == id && m.UserId == userid).Count() == 0)
            {
                DataContext.OrganizationMembers.Add(new OrganizationMember() { OrganizationId = id, UserId = userid, Admin = false });
                DataContext.SaveChanges();
            }
            return RedirectToAction("View", new { id = id });
        }

        //// GET: School
        //public ActionResult ViewTicket(int id)
        //{
        //    var model = new ViewTicketViewModel();
        //    model.Ticket = DataContext.Tickets
        //                        .Where(t => t.Id == id)
        //                        .Include("EventTickets")
        //                        .Include("EventTickets.Event")
        //                        .FirstOrDefault();
        //    //model.AvailableClasses = DataContext.Classes.Where(c => c.SchoolId == model.Ticket.SchoolId).Select(c => new ListItem() { Id = c.Id, Name = c.Name });
        //    //model.SelectedClasses = model.Ticket.EventTickets.Select(t => new ListItem() { Id = t.EventId, Name = t.Event.Name, IsSelected = true });
        //    var eventTickets = model.Ticket.EventTickets.Where(et => et.TicketId == id);
        //    var classes = DataContext.Classes.Where(c => c.SchoolId == model.Ticket.SchoolId);
        //    var eids = eventTickets.Select(et => et.EventId).ToArray();
        //    model.EventTickets = new List<EventTicketPlaceholder>();
        //    foreach(var c in classes)
        //    {
        //        model.EventTickets.Add(new EventTicketPlaceholder() { Connect = eids.Contains(c.Id), EventTicket = new EventTicket() { Event = c, TicketId = id } });
        //    }
        //    return View(model);
        //}

        //// POST: School
        //[HttpPost]
        //public ActionResult UpdateTickets(ViewTicketViewModel model)
        //{
        //    DataContext.EventTickets.RemoveRange(DataContext.EventTickets.Where(et => et.TicketId == model.Ticket.Id));
        //    //  DataContext.Tickets.Where(t => t.Id == model.Ticket.Id).Include("EventTickets").FirstOrDefault().EventTickets.Clear();
        //    DataContext.EventTickets.AddRange(model.EventTickets.Where(et => et.Connect).Select(et => new EventTicket() { EventId = et.EventTicket.Event.Id, TicketId = model.Ticket.Id }));
        //    DataContext.SaveChanges();
        //    return RedirectToAction("ViewTicket", new { id = model.Ticket.Id });
        //    //var members = school.Members;
        //    //var schoolId = members.FirstOrDefault().OrganizationId;
        //    //foreach(var mbr in members)
        //    //{
        //    //    var member = DataContext.OrganizationMembers.Single(m => m.Id == mbr.Id);
        //    //    member.Admin = mbr.Admin;
        //    //}

        //    //DataContext.SaveChanges();

        //    //return RedirectToAction("View", new { id = schoolId });
        //}
    }
}
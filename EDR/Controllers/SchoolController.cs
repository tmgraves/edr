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
using System.Text.RegularExpressions;

namespace EDR.Controllers
{
    public class SchoolController : BaseController
    {
        // GET: School
        [Route("Schools")]
        public ActionResult List(ListSchoolViewModel model)
        {
            model.DanceStyles = DataContext.DanceStyles.Select(d => d.Name).ToArray();
            model.Schools = DataContext.Schools
                                .Include("Teachers.ApplicationUser")
                                .Include("Classes.DanceStyles")
                                .Include("Reviews")
                                .AsQueryable();
            if (model.DanceStyleId != null)
            {
                model.Schools = model.Schools.Where(s => s.Classes.Any(c => c.DanceStyles.Select(st => st.Id).Contains((int)model.DanceStyleId)));
            }
            if (model.TeacherId != null)
            {
                model.Schools = model.Schools.Where(s => s.Teachers.Select(t => t.ApplicationUser.Id).Contains(model.TeacherId));
            }

            if (model.NELat != null && model.SWLng != null)
            {
                model.Schools = model.Schools.Where(c => c.Longitude >= model.SWLng && c.Longitude <= model.NELng && c.Latitude >= model.SWLat && c.Latitude <= model.NELat);
            }

            model.Schools = model.Schools.ToList().Take(100);
            if (HttpContext.Request.Browser.IsMobileDevice)
            {
                return View("Mobile/List", model);
            }
            else
            {
                return View(model);
            }
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
        [Route("School/{id}/{school}/{location}")]
        public ActionResult View(int? id)
        {
            if (!id.HasValue)
            {
                return RedirectToAction("List");
            }

            var userid = User.Identity.GetUserId();
            var model = new ViewSchoolViewModel(
                        DataContext.Schools
                        .Where(s => s.Id == id)
                        .Include("Members.User")
                        .Include("DanceStyles")
                        .Include("Tickets.Event")
                        .Include("Owners.ApplicationUser")
                        .Include("Teachers.ApplicationUser")
                        .Include("Teams.DanceStyles")
                        .Include("Classes.Reviews")
                        .FirstOrDefault());
            model.Member = DataContext.OrganizationMembers
                                        .Where(m => m.OrganizationId == id && m.UserId == userid && m.Admin).FirstOrDefault();
            model.UserTickets = DataContext.UserTickets
                                        .Include("Ticket")
                                        .Include("EventRegistrations")
                                        .Where(t => t.Ticket.SchoolId == id && t.UserId == userid).ToList();
            model.School.DanceStyles = DataContext.DanceStyles.Where(s => s.Events.OfType<Class>().Any(c => c.SchoolId == id)).ToList();
            if (HttpContext.Request.Browser.IsMobileDevice)
            {
                return View("Mobile/View", model);
            }
            else
            {
                return View(model);
            }
        }

        // GET: School
        [Authorize]
        public ActionResult Manage(int? id)
        {
            if (!id.HasValue)
            {
                return RedirectToAction("List");
            }

            var userid = User.Identity.GetUserId();
            var admin = User.IsInRole("Admin");
            var schooladmin = DataContext.OrganizationMembers.Where(m => m.OrganizationId == id && m.UserId == userid && m.Admin).Count() != 0 ? true : false;
            if (schooladmin || admin)
            {
                var model = new ManageSchoolViewModel(
                            DataContext.Schools
                            .Where(s => s.Id == id)
                            .Include("Members")
                            .Include("Members.User")
                            .Include("Classes.EventInstances.EventRegistrations")
                            .Include("Tickets")
                            .Include("Tickets.Event")
                            .Include("Owners")
                            .Include("Owners.ApplicationUser")
                            .Include("Teachers")
                            .Include("Teachers.ApplicationUser")
                            .Include("Teams")
                            .Include("FinancialTransactions")
                            .FirstOrDefault());
                return View(model);
            }
            else
            {
                return RedirectToAction("View", new { id = id });
            }
        }

        [Authorize(Roles = "Teacher,Owner")]
        [HttpPost]
        // POST: School
        public ActionResult Save(ManageSchoolViewModel model)
        {
            var sch = DataContext.Schools.Single(m => m.Id == model.School.Id);
            TryUpdateModel(sch, "School");
            DataContext.Entry(sch).State = EntityState.Modified;
            DataContext.SaveChanges();
            return RedirectToAction("Manage", new { id = model.School.Id });
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

        // POST: School
        [HttpPost]
        [Authorize(Roles = "Teacher,Owner")]
        public ActionResult UpdateMembers(ManageSchoolViewModel model)
        {
            foreach (var m in model.School.Members)
            {
                var mem = DataContext.OrganizationMembers.Where(om => om.Id == m.Id).FirstOrDefault();
                mem.Admin = m.Admin;
            }
            DataContext.SaveChanges();
            return RedirectToAction("Manage", new { id = model.School.Id });
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

        [Authorize]
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

        [Authorize(Roles = "Teacher,Owner")]
        [HttpPost]
        public ActionResult AddMember(ManageSchoolViewModel model)
        {
            if (DataContext.OrganizationMembers.Where(m => m.OrganizationId == model.School.Id && m.UserId == model.NewMemberId).Count() == 0)
            {
                DataContext.OrganizationMembers.Add(new OrganizationMember() { OrganizationId = model.School.Id, UserId = model.NewMemberId, Admin = false });
                DataContext.SaveChanges();
            }
            return RedirectToAction("Manage", new { id = model.School.Id });
        }

        [Authorize(Roles = "Teacher,Owner")]
        [HttpPost]
        public ActionResult AddTeacher(FormCollection formCollection)
        {
            int id = Convert.ToInt32(formCollection["id"]);
            string teacherid = formCollection["teacherid"];
            if (DataContext.Schools.Where(s => s.Id == id && s.Teachers.Any(t => t.ApplicationUser.Id == teacherid)).Count() == 0)
            {
                DataContext.Schools.Single(s => s.Id == id).Teachers.Add(DataContext.Teachers.Single(t => t.ApplicationUser.Id == teacherid));
                DataContext.SaveChanges();
            }
            return RedirectToAction("Manage", new { id = id });
        }

        [Authorize(Roles = "Teacher,Owner")]
        public ActionResult RemoveTeacher(int id, int teacherid)
        {
            DataContext.Schools.Where(s => s.Id == id).Include("Teachers").FirstOrDefault().Teachers.Remove(DataContext.Teachers.Single(t => t.Id == teacherid));
            DataContext.SaveChanges();
            return RedirectToAction("Manage", new { id = id });
        }

        [Authorize(Roles = "Teacher,Owner")]
        public ActionResult RemoveMember(int id, string memberid)
        {
            DataContext.OrganizationMembers.Remove(DataContext.OrganizationMembers.Single(m => m.OrganizationId == id && m.UserId == memberid));
            DataContext.SaveChanges();
            return RedirectToAction("Manage", new { id = id });
        }

        [Authorize(Roles = "Teacher,Owner")]
        [HttpPost]
        public ActionResult AddOwner(FormCollection formCollection)
        {
            int id = Convert.ToInt32(formCollection["id"]);
            string ownerid = formCollection["ownerid"];
            if (DataContext.Schools.Where(s => s.Id == id && s.Owners.Any(t => t.ApplicationUser.Id == ownerid)).Count() == 0)
            {
                DataContext.Schools.Single(s => s.Id == id).Owners.Add(DataContext.Owners.Single(t => t.ApplicationUser.Id == ownerid));
                DataContext.SaveChanges();
            }
            return RedirectToAction("Manage", new { id = id });
        }

        [Authorize(Roles = "Teacher,Owner")]
        public ActionResult RemoveOwner(int id, int ownerid)
        {
            DataContext.Schools.Where(s => s.Id == id).Include("Owners").FirstOrDefault().Owners.Remove(DataContext.Owners.Single(t => t.Id == ownerid));
            DataContext.SaveChanges();
            return RedirectToAction("Manage", new { id = id });
        }

        [Authorize]
        [HttpPost]
        public JsonResult UploadImageAsync(string imageData, int id)
        {
            var newFile = new UploadFile();
            if (string.IsNullOrEmpty(imageData))
                newFile.UploadStatus = "Failed";

            Match imageMatch = Regex.Match(imageData, @"^data:(?<mimetype>[^;]+);base64,(?<data>.+)$");
            if (!imageMatch.Success)
                newFile.UploadStatus = "Failed";

            string mimeType = imageMatch.Groups["mimetype"].Value;
            Match imageType = Regex.Match(mimeType, @"^[^/]+/(?<type>.+?)$");
            if (!imageType.Success)
                newFile.UploadStatus = "Failed";

            string fileExtension = imageType.Groups["type"].Value;
            byte[] data2 = Convert.FromBase64String(imageMatch.Groups["data"].Value);

            if (newFile.UploadStatus != "Failed")
            {
                newFile = EDR.Utilities.ApplicationUtility.UploadFromPath(imageData);
                if (newFile.UploadStatus == "Success")
                {
                    var school = DataContext.Schools.Single(s => s.Id == id);
                    EDR.Utilities.ApplicationUtility.DeletePicture(new Picture() { Filename = school.PhotoUrl });
                    school.PhotoUrl = newFile.FilePath;
                    DataContext.SaveChanges();
                }
            }
            var objUpload = new { FilePath = Url.Content(newFile.FilePath), UploadStatus = newFile.UploadStatus };
            return Json(objUpload, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public virtual ActionResult GetClassesPartial(int id)
        {
            var start = DateTime.Today;
            var classes = DataContext.Classes
                                .Include("EventInstances.EventRegistrations")
                                .Include("Reviews")
                                .Where(c => c.EventInstances.Any(i => i.DateTime >= start)
                                        && c.SchoolId == id);
            return PartialView("~/Views/Shared/_EventsPartial.cshtml", classes);
        }

        [HttpGet]
        public virtual ActionResult GetPerformancesPartial(int id)
        {
            var start = DateTime.Today;
            var performances = DataContext.Performances
                                .Where(c => c.EventInstances.Any(i => i.DateTime >= start)
                                        && c.Team.SchoolId == id);
            return PartialView("~/Views/Shared/_EventsPartial.cshtml", performances);
        }

        [HttpGet]
        public virtual ActionResult GetAuditionsPartial(int id)
        {
            var start = DateTime.Today;
            var performances = DataContext.Auditions
                                .Where(c => c.EventInstances.Any(i => i.DateTime >= start)
                                        && c.Team.SchoolId == id);
            return PartialView("~/Views/Shared/_EventsPartial.cshtml", performances);
        }

        [HttpGet]
        public virtual ActionResult GetEventsPartial(int id)
        {
            var start = DateTime.Today;
            var events = new List<Event>();

            events.AddRange(DataContext.Classes
                                .Include("DanceStyles")
                                .Include("Place")
                                .Include("EventInstances.EventRegistrations")
                                .Include("Reviews")
                                .Include("Teachers.ApplicationUser")
                                .Where(c => c.EventInstances.Any(i => i.DateTime >= start)
                                        && c.SchoolId == id).AsEnumerable());

            events.AddRange(DataContext.Auditions
                                .Where(a => a.Team.SchoolId == id && a.EventInstances.Any(i => i.DateTime >= start)).AsEnumerable());
            events.AddRange(DataContext.Performances
                                .Where(a => a.Team.SchoolId == id && a.EventInstances.Any(i => i.DateTime >= start)).AsEnumerable());
            events.AddRange(DataContext.Rehearsals
                                .Where(a => a.Team.SchoolId == id && a.EventInstances.Any(i => i.DateTime >= start)).AsEnumerable());

            return PartialView("~/Views/Shared/_EventsPartial.cshtml", events);
        }

        public JsonResult GetClasses(DateTime start, DateTime end, int? schoolId)
        {
            var instances = DataContext.Classes
                                    .Where(c => c.SchoolId == schoolId)
                                    .SelectMany(c => c.EventInstances)
                                    .Where(i => i.DateTime >= start && i.DateTime <= end).ToList();

            return Json(instances.AsEnumerable().Select(s =>
                        new {
                            id = s.EventId,
                            title = s.Event.Name,
                            start = s.StartTime.Value.ToString("o"),
                            end = s.EndTime.Value.ToString("o"),
                            lat = s.Event.Place.Latitude,
                            lng = s.Event.Place.Longitude,
                            url = Url.Action("View", "Event", new { id = s.EventId, eventType = EventType.Class })
                        }), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetAuditions(int? schoolId, DateTime start, DateTime end)
        {
            var auditions = DataContext.Auditions
                                .Include("EventInstances.EventRegistrations")
                                .Where(a => a.Team.SchoolId == schoolId)
                                .Where(a => a.StartDate >= start && a.StartDate <= end);

            var auditionlist = auditions.ToList().Take(100);

            var userid = User.Identity.GetUserId();

            return Json(auditionlist.AsEnumerable().SelectMany(a => a.EventInstances.Where(i => i.DateTime >= start && i.DateTime <= end).Select(s =>
                        new {
                            id = (s.Event as Audition).TeamId,
                            title = s.Event.Name,
                            start = s.StartTime.Value.ToString("o"),
                            end = s.EndTime.Value.ToString("o"),
                            lat = s.Event.Place.Latitude,
                            lng = s.Event.Place.Longitude,
                            color = "#428bca",
                            url = Url.Action("View", "Team", new { id = (s.Event as Audition).TeamId }),
                        })
                        ), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetPerformances(int? schoolId, DateTime start, DateTime end)
        {
            var auditions = DataContext.Performances
                                .Include("EventInstances.EventRegistrations")
                                .Where(a => a.Team.SchoolId == schoolId)
                                .Where(a => a.StartDate >= start && a.StartDate <= end);

            var auditionlist = auditions.ToList().Take(100);

            var userid = User.Identity.GetUserId();

            return Json(auditionlist.AsEnumerable().SelectMany(a => a.EventInstances.Where(i => i.DateTime >= start && i.DateTime <= end).Select(s =>
                        new {
                            id = (s.Event as Performance).TeamId,
                            title = s.Event.Name,
                            start = s.StartTime.Value.ToString("o"),
                            end = s.EndTime.Value.ToString("o"),
                            lat = s.Event.Place.Latitude,
                            lng = s.Event.Place.Longitude,
                            color = "#f0ad4e",
                            url = Url.Action("View", "Team", new { id = (s.Event as Performance).TeamId }),
                        })
                        ), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetRehearsals(int? schoolId, DateTime start, DateTime end)
        {
            var auditions = DataContext.Rehearsals
                                .Include("EventInstances.EventRegistrations")
                                .Where(a => a.Team.SchoolId == schoolId)
                                .Where(a => a.StartDate >= start && a.StartDate <= end);

            var auditionlist = auditions.ToList().Take(100);

            var userid = User.Identity.GetUserId();

            return Json(auditionlist.AsEnumerable().SelectMany(a => a.EventInstances.Where(i => i.DateTime >= start && i.DateTime <= end).Select(s =>
                        new {
                            id = (s.Event as Rehearsal).TeamId,
                            title = s.Event.Name,
                            start = s.StartTime.Value.ToString("o"),
                            end = s.EndTime.Value.ToString("o"),
                            lat = s.Event.Place.Latitude,
                            lng = s.Event.Place.Longitude,
                            color = "#d9534f",
                            url = Url.Action("View", "Team", new { id = (s.Event as Rehearsal).TeamId }),
                        })
                        ), JsonRequestBehavior.AllowGet);
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
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using EDR.Data;
using EDR.Models;
using EDR.Models.ViewModels;
using System.Data.Entity.Validation;
using Microsoft.AspNet.Identity;
using System.Text.RegularExpressions;
using EDR.Utilities;

namespace EDR.Controllers
{
    public class TeamController : BaseController
    {
        // GET: Team
        //  [Route("Teams")]
        public ActionResult Index(TeamIndexViewModel model)
        {
            model.DanceStyles = DataContext.DanceStyles.Select(s => s.Name).ToArray();
            model.Teams = DataContext.Teams
                                .Include("Teachers.ApplicationUser")
                                .Include("DanceStyles")
                                .Include("Reviews")
                                .Include("Auditions")
                                .AsQueryable();
            if (model.DanceStyleId != null)
            {
                model.Teams = model.Teams.Where(c => c.DanceStyles.Select(st => st.Id).Contains((int)model.DanceStyleId));
            }

            if (model.NELat != null && model.SWLng != null)
            {
                model.Teams = model.Teams.Where(c => c.Longitude >= model.SWLng && c.Longitude <= model.NELng && c.Latitude >= model.SWLat && c.Latitude <= model.NELat);
            }
            if (model.SkillLevel != null)
            {
                model.Teams = model.Teams.Where(x => x.SkillLevel == model.SkillLevel);
            }
            if (model.TeacherId != null)
            {
                model.Teams = model.Teams.Where(c => c.Teachers.Select(t => t.ApplicationUser.Id).Contains(model.TeacherId));
            }

            model.Teams = model.Teams.ToList().Take(100);

            if (HttpContext.Request.Browser.IsMobileDevice)
            {
                return View("Mobile/Index", model);
            }
            else
            {
                return View(model);
            }
        }

        // GET: Team/Details/5
        [Route("Team/{id}/{team}/{location}")]
        public ActionResult View(int? id)
        {
            var userid = User.Identity.GetUserId();

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var model = new TeamViewViewModel();
            model.Team = DataContext.Teams
                                .Include("School")
                                .Include("Members.User")
                                .Include("DanceStyles")
                                .Include("Teachers.ApplicationUser")
                                .Single(t => t.Id == id);
            model.Member = model.Team.Members.Where(m => m.User.Id == userid).FirstOrDefault();
            if (model.Team == null)
            {
                return HttpNotFound();
            }
            if (HttpContext.Request.Browser.IsMobileDevice)
            {
                return View("Mobile/View", model);
            }
            else
            {
                return View(model);
            }
        }

        [Route("Team/Manage")]
        [Authorize(Roles = "Teacher")]
        public ActionResult Manage(int? id)
        {
            if (!id.HasValue)
            {
                ViewBag.errorMessage = "Invalid Id";
                return View("Error");
            }

            var model = new TeamManageViewModel();
            model.Team = DataContext.Teams
                            .Include("Members.User")
                            .Include("Rehearsals.Place")
                            .Include("Auditions.Place")
                            .Include("Performances.Place")
                            .Include("Performances.Event.Place")
                            .Include("DanceStyles")
                            .Include("School")
                            .Include("Videos")
                            .Include("Playlists")
                            .Single(e => e.Id == id);
            if (model.Team == null)
            {
                return HttpNotFound();
            }
            return View(model);
        }

        [Route("Team/AddMember")]
        [Authorize(Roles = "Teacher,Owner")]
        [HttpPost]
        public ActionResult AddMember(TeamManageViewModel model)
        {
            if (DataContext.OrganizationMembers.Where(m => m.OrganizationId == model.Team.Id && m.UserId == model.NewMemberId).Count() == 0)
            {
                DataContext.OrganizationMembers.Add(new OrganizationMember() { OrganizationId = model.Team.Id, UserId = model.NewMemberId, Admin = false });
                DataContext.SaveChanges();
            }
            return RedirectToAction("Manage", new { id = model.Team.Id });
        }

        [Route("Team/Manage")]
        [Authorize(Roles = "Teacher")]
        [HttpPost]
        public ActionResult Manage(TeamManageViewModel model)
        {
            var t = ModelState.IsValidField("Team");
            if (ModelState.IsValidField("Team"))
            {
                var team = DataContext.Teams.Single(m => m.Id == model.Team.Id);
                TryUpdateModel(team, "Team");
                DataContext.Entry(team).State = EntityState.Modified;
                DataContext.SaveChanges();
                return RedirectToAction("Manage", new { id = model.Team.Id });
            }
            return View(model);
        }

        // GET: Team/Create
        [Route("Team/Create")]
        [Authorize(Roles = "Teacher")]
        public ActionResult Create(int? schoolId)
        {
            var userid = User.Identity.GetUserId();
            var model = new TeamCreateViewModel();
            model.Team.SchoolId = schoolId;
            model.Schools = DataContext.Schools.Where(g => g.Teachers.Any(p => p.ApplicationUser.Id == userid) || g.Owners.Any(p => p.ApplicationUser.Id == userid) || g.Members.Any(p => p.UserId == userid)).ToList();
            return View(model);
        }

        [Route("Team/Delete")]
        [Authorize(Roles ="Teacher, Admin")]
        public ActionResult Delete(int id)
        {
            var userid = User.Identity.GetUserId();
            var user = UserManager.FindById(userid);

            var tm = DataContext.Teams
                        .Include("Members")
                        .Include("Reviews")
                        .Include("Auditions.EventInstances.EventRegistrations")
                        .Include("Performances.EventInstances.EventRegistrations")
                        .Include("Rehearsals.EventInstances.EventRegistrations")
                        .Where(t => t.Id == id).FirstOrDefault();
            var schoolid = tm.SchoolId;

            DataContext.OrganizationMembers.RemoveRange(tm.Members);
            DataContext.Reviews.RemoveRange(tm.Reviews);
            tm.DanceStyles.Clear();
            DataContext.Feeds.RemoveRange(tm.Feeds);
            DataContext.EventRegistrations.RemoveRange(tm.Performances.SelectMany(a => a.EventInstances.SelectMany(i => i.EventRegistrations)));
            DataContext.Performances.RemoveRange(tm.Performances);
            DataContext.EventRegistrations.RemoveRange(tm.Auditions.SelectMany(a => a.EventInstances.SelectMany(i => i.EventRegistrations)));
            DataContext.Auditions.RemoveRange(tm.Auditions);
            DataContext.EventRegistrations.RemoveRange(tm.Rehearsals.SelectMany(a => a.EventInstances.SelectMany(i => i.EventRegistrations)));
            DataContext.Rehearsals.RemoveRange(tm.Rehearsals);
            tm.Teachers.Clear();
            DataContext.Teams.Remove(tm);
            DataContext.SaveChanges();

            return RedirectToAction("Manage", "School", new { id = schoolid });
        }

        // POST: Team/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Route("Team/Create")]
        [Authorize(Roles = "Teacher")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(TeamCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                model.Team.DateStarted = DateTime.Now;
                var userid = User.Identity.GetUserId();
                model.Team.Teachers = new List<Teacher>();
                model.Team.Teachers.Add(DataContext.Teachers.Single(t => t.ApplicationUser.Id == userid));
                model.Team.Members = new List<OrganizationMember>();
                model.Team.Members.Add(new OrganizationMember() { UserId = userid, Admin = true, OrganizationId = model.Team.Id });
                DataContext.Teams.Add(model.Team);
                DataContext.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(model);
        }

        // GET: Team/Edit/5
        [Route("Team/Edit")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Team team = DataContext.Teams.Find(id);
            if (team == null)
            {
                return HttpNotFound();
            }
            return View(team);
        }

        // POST: Team/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Route("Team/Edit")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Description,FacebookLink,Public,DateStarted,Address,Address2,City,State,Zip,Country,Latitude,Longitude")] Team team)
        {
            if (ModelState.IsValid)
            {
                DataContext.Entry(team).State = EntityState.Modified;
                DataContext.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(team);
        }

        //// GET: Team/Delete/5
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    var model = new TeamDeleteViewModel();
        //    model.Team = DataContext.Teams.Find(id);
        //    if (model.Team == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(model);
        //}

        // POST: Team/Delete/5
        [Route("Team/DeleteConfirmed")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Team team = DataContext.Teams.Find(id);
            DataContext.Teams.Remove(team);
            DataContext.SaveChanges();
            return RedirectToAction("Index");
        }

        [Route("Team/AddTeacher")]
        [Authorize(Roles = "Teacher,Owner")]
        [HttpPost]
        public ActionResult AddTeacher(FormCollection formCollection)
        {
            int id = Convert.ToInt32(formCollection["id"]);
            string teacherid = formCollection["teacherid"];
            if (DataContext.Teams.Where(s => s.Id == id && s.Teachers.Any(t => t.ApplicationUser.Id == teacherid)).Count() == 0)
            {
                DataContext.Teams.Single(s => s.Id == id).Teachers.Add(DataContext.Teachers.Single(t => t.ApplicationUser.Id == teacherid));
                DataContext.SaveChanges();
            }
            return RedirectToAction("Manage", new { id = id });
        }

        [Route("Team/RemoveTeacher")]
        [Authorize(Roles = "Teacher,Owner")]
        public ActionResult RemoveTeacher(int id, int teacherid)
        {
            var team = DataContext.Teams.Where(s => s.Id == id).Include("Teachers").FirstOrDefault();
            if (team.Teachers.Count() > 1)
            {
                team.Teachers.Remove(team.Teachers.Single(t => t.Id == teacherid));
                DataContext.SaveChanges();
            }
            return RedirectToAction("Manage", new { id = id });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DataContext.Dispose();
            }
            base.Dispose(disposing);
        }

        [Route("Team/AddRehearsal")]
        [Authorize(Roles = "Teacher")]
        [HttpPost]
        public ActionResult AddRehearsal(TeamManageViewModel model)
        {
            if (ModelState.IsValidField("NewRehearsal"))
            {
                model.NewRehearsal.Free = true;
                model.NewRehearsal.Recurring = true;
                model.NewRehearsal.Frequency = Enums.Frequency.Weekly;
                model.NewRehearsal.Interval = 1;
                var reh = new Rehearsal();
                UpdateModel(reh, "NewRehearsal");
                reh.EventInstances = new List<EventInstance>();
                reh.EventInstances.Add(new EventInstance() { DateTime = model.NewRehearsal.StartDate, EndDate = Convert.ToDateTime(model.NewRehearsal.EndDate), StartTime = model.NewRehearsal.StartTime, EndTime = model.NewRehearsal.EndTime, PlaceId = model.NewRehearsal.PlaceId });
                DataContext.Rehearsals.Add(reh);
                DataContext.SaveChanges();
                return RedirectToAction("Manage", new { id = model.NewRehearsal.TeamId });
            }
            else
            {
                return View(model);
            }
        }

        [Route("Team/DeleteRehearsal")]
        [Authorize(Roles = "Teacher")]
        public ActionResult DeleteRehearsal(int id)
        {
            var rehearsal = DataContext.Rehearsals.Include("EventInstances").Single(s => s.Id == id);
            var teamId = rehearsal.TeamId;
            if (rehearsal.EventInstances != null)
            {
                DataContext.EventInstances.RemoveRange(rehearsal.EventInstances);
            }
            DataContext.Rehearsals.Remove(rehearsal);
            DataContext.SaveChanges();
            return RedirectToAction("Manage", new { id = teamId });
        }

        [Route("Team/AddAudition")]
        [Authorize(Roles = "Teacher")]
        [HttpPost]
        public ActionResult AddAudition(TeamManageViewModel model)
        {
            try
            {
                model.NewAudition.Free = true;
                model.NewAudition.EventInstances = new List<EventInstance>();
                model.NewAudition.EventInstances.Add(new EventInstance() { DateTime = model.NewAudition.StartDate, EndDate = Convert.ToDateTime(model.NewAudition.EndDate), StartTime = model.NewAudition.StartTime, EndTime = model.NewAudition.EndTime, PlaceId = model.NewAudition.PlaceId });
                DataContext.Auditions.Add(model.NewAudition);
                DataContext.SaveChanges();
                return RedirectToAction("Manage", new { id = model.NewAudition.TeamId });
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }
                throw;
            }
            //if (ModelState.IsValidField("NewAudition"))
            //{
            //    DataContext.Auditions.Add(model.NewAudition);
            //    DataContext.SaveChanges();
            //    return RedirectToAction("Manage", new { id = model.NewAudition.TeamId });
            //}
            //else
            //{
            //    return View(model);
            //}
            //if (TryValidateModel(audition))
            //{
            //}
        }

        [Route("Team/ImportAudition")]
        [Authorize(Roles = "Teacher")]
        [HttpPost]
        public ActionResult ImportAudition(TeamManageViewModel model)
        {
            try
            {
                var userid = User.Identity.GetUserId();
                var token = DataContext.Users.Single(u => u.Id == userid).FacebookToken;

                if (token != null)
                {
                    var fb = Utilities.FacebookHelper.GetEvent(model.NewFacebookEventId, token);
                    var gadd = Utilities.Geolocation.ParseAddress(fb.Address.Street + " " + fb.Address.City + ", " + fb.Address.State + " " + fb.Address.ZipCode);
                    fb.Address.GooglePlaceId = gadd.GooglePlaceId;
                    fb.Address.Street = gadd.Street;
                    fb.Address.City = gadd.City;
                    fb.Address.State = gadd.State;
                    fb.Address.ZipCode = gadd.ZipCode;

                    var pl = DataContext.Places.Where(p => p.GooglePlaceId == fb.Address.GooglePlaceId).FirstOrDefault();
                    if (pl == null)
                    {
                        var Place = new Place() { Name = fb.Address.Location, Address = fb.Address.Street, City = fb.Address.City, StateName = fb.Address.State ?? "CA", Zip = fb.Address.ZipCode, Country = fb.Address.Country, Latitude = fb.Address.Latitude, Longitude = fb.Address.Longitude, FacebookId = fb.Address.FacebookId, PlaceType = Utilities.FacebookHelper.ParsePlaceType(fb.Address.Categories), Public = true, Website = Uri.IsWellFormedUriString(fb.Address.WebsiteUrl, UriKind.RelativeOrAbsolute) ? fb.Address.WebsiteUrl : null, FacebookLink = fb.Address.FacebookUrl, Filename = fb.Address.CoverPhotoUrl, ThumbnailFilename = fb.Address.ThumbnailUrl, GooglePlaceId = fb.Address.GooglePlaceId };
                        pl = DataContext.Places.Add(Place);
                        DataContext.SaveChanges();
                    }
                    var audition = new Audition()
                    {
                        TeamId = model.Team.Id,
                        Name = fb.Name,
                        StartDate = fb.StartTime,
                        StartTime = fb.StartTime,
                        EndDate = fb.EndTime != null ? fb.EndTime : fb.StartTime,
                        EndTime = fb.EndTime != null ? fb.EndTime : fb.StartTime,
                        FacebookId = fb.Id,
                        PlaceId = pl.Id,
                        EventInstances = new List<EventInstance>() { new EventInstance() { DateTime = fb.StartTime, StartTime = fb.StartTime, EndDate = fb.EndTime != null ? Convert.ToDateTime(fb.EndTime) : fb.StartTime, EndTime = fb.EndTime, PlaceId = pl.Id } }
                    };
                    DataContext.Auditions.Add(audition);
                    DataContext.SaveChanges();
                }
                return RedirectToAction("Manage", new { id = model.Team.Id });
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }
                throw;
            }
            //if (ModelState.IsValidField("NewAudition"))
            //{
            //    DataContext.Auditions.Add(model.NewAudition);
            //    DataContext.SaveChanges();
            //    return RedirectToAction("Manage", new { id = model.NewAudition.TeamId });
            //}
            //else
            //{
            //    return View(model);
            //}
            //if (TryValidateModel(audition))
            //{
            //}
        }

        [Route("Team/DeleteAudition")]
        [Authorize(Roles = "Teacher")]
        public ActionResult DeleteAudition(int id)
        {
            var audition = DataContext.Auditions.Include("EventInstances").Single(s => s.Id == id);
            var teamId = audition.TeamId;
            if(audition.EventInstances != null)
            {
                DataContext.EventInstances.RemoveRange(audition.EventInstances);
            }
            DataContext.Auditions.Remove(audition);
            DataContext.SaveChanges();
            return RedirectToAction("Manage", new { id = teamId });
        }

        [Route("Team/AddPerformance")]
        [Authorize(Roles = "Teacher")]
        [HttpPost]
        public ActionResult AddPerformance(TeamManageViewModel model)
        {
            try
            {
                model.NewPerformance.Free = true;
                model.NewPerformance.EventInstances = new List<EventInstance>();
                model.NewPerformance.EventInstances.Add(new EventInstance() { DateTime = model.NewPerformance.StartDate, EndDate = Convert.ToDateTime(model.NewPerformance.EndDate), StartTime = model.NewPerformance.StartTime, EndTime = model.NewPerformance.EndTime, PlaceId = model.NewPerformance.PlaceId });
                DataContext.Performances.Add(model.NewPerformance);
                DataContext.SaveChanges();
                return RedirectToAction("Manage", new { id = model.NewPerformance.TeamId });
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }
                throw;
            }
        }

        [Route("Team/DeletePerformance")]
        [Authorize(Roles = "Teacher")]
        public ActionResult DeletePerformance(int id)
        {
            var performance = DataContext.Performances.Include("EventInstances").Single(s => s.Id == id);
            var teamId = performance.TeamId;
            if (performance.EventInstances != null)
            {
                DataContext.EventInstances.RemoveRange(performance.EventInstances);
            }
            DataContext.Performances.Remove(performance);
            DataContext.SaveChanges();
            return RedirectToAction("Manage", new { id = teamId });
        }

        [Route("Team/AddStyle")]
        [Authorize(Roles = "Owner,Teacher")]
        public PartialViewResult AddStyle(TeamManageViewModel model)
        {
            var team = DataContext.Teams.Include("DanceStyles").Single(e => e.Id == model.Team.Id);
            if (team.DanceStyles.Where(s => s.Id == model.NewStyleId).Count() == 0)
            {
                team.DanceStyles.Add(DataContext.DanceStyles.Single(s => s.Id == model.NewStyleId));
                DataContext.SaveChanges();
            }
            return PartialView("~/Views/Team/Partial/_DanceStylesPartial.cshtml", new TeamDanceStylesPartialViewModel() { DanceStyles = team.DanceStyles, TeamId = team.Id });
        }

        [Route("Team/DeleteStyle")]
        [Authorize(Roles = "Owner,Promoter,Teacher")]
        public ActionResult DeleteStyle(int id, int styleId)
        {
            DataContext.Teams.Include("DanceStyles").Single(e => e.Id == id).DanceStyles.Remove(DataContext.DanceStyles.Single(s => s.Id == styleId));
            DataContext.SaveChanges();
            return RedirectToAction("Manage", new { id = id });
        }

        [Route("Team/GetAuditionsPartial")]
        [HttpGet]
        public virtual ActionResult GetAuditionsPartial(int id)
        {
            var start = DateTime.Today;
            var auditions = DataContext.Auditions
                                .Include("Place")
                                .Include("EventInstances")
                                .Include("EventInstances.EventRegistrations")
                                .Where(a => a.StartDate >= start && a.TeamId == id);
            //  return PartialView("~/Views/Shared/DisplayTemplates/Auditions.cshtml", auditions);
            return PartialView("~/Views/Shared/_EventsPartial.cshtml", auditions);
        }

        [Route("Team/GetPerformancesPartial")]
        [HttpGet]
        public virtual ActionResult GetPerformancesPartial(int id)
        {
            var start = DateTime.Today;
            var performances = DataContext.Performances
                                .Include("Place")
                                .Include("EventInstances")
                                .Include("EventInstances.EventRegistrations")
                                .Where(p => p.TeamId == id && p.StartDate >= start);
            //  return PartialView("~/Views/Shared/DisplayTemplates/Performances.cshtml", performances);
            return PartialView("~/Views/Shared/_EventsPartial.cshtml", performances);
        }

        [Route("Team/GetRehearsalsPartial")]
        [HttpGet]
        public virtual ActionResult GetRehearsalsPartial(int id)
        {
            var start = DateTime.Today;
            var rehearsals = DataContext.Rehearsals
                                .Include("Place")
                                .Include("EventInstances")
                                .Include("EventInstances.EventRegistrations")
                                .Where(a => a.EventInstances.Any(i => i.DateTime >= start) && a.TeamId == id);
            return PartialView("~/Views/Shared/_EventsPartial.cshtml", rehearsals);
        }

        [Route("Team/GetRehearsalDatesPartial")]
        [HttpGet]
        public virtual ActionResult GetRehearsalDatesPartial(int id)
        {
            var start = DateTime.Today;
            var reh = DataContext.Rehearsals
                                .Include("Place")
                                .Include("EventInstances")
                                .Where(r => r.Id == id).FirstOrDefault();

            if (reh.EventInstances.Where(i => i.DateTime >= start).Count() > 0)
            {
                var instances = DataContext.Rehearsals
                                    .Include("Place")
                                    .Include("EventInstances")
                                    .Where(r => r.EventInstances.Any(i => i.DateTime >= start) && r.Id == id).FirstOrDefault().EventInstances;
                return PartialView("~/Views/Shared/_EventInstancesPartial.cshtml", instances);
            }
            else
            {
                return PartialView("~/Views/Shared/_EventInstancesPartial.cshtml", new List<EventInstance>());
            }
        }

        [Route("Team/AddRehearsalDates")]
        [Authorize(Roles = "Owner,Teacher")]
        [HttpPost]
        public virtual ActionResult AddRehearsalDates(int rehearsalId, int eventCount)
        {
            var reh = DataContext.Rehearsals.Include("EventInstances").Single(e => e.Id == rehearsalId);

            if (DataContext.EventInstances.Where(i => i.EventId == rehearsalId && i.DateTime >= DateTime.Today).Count() < 20)
            {
                var sdate = reh.StartDate;
                if (DataContext.EventInstances.Where(i => i.EventId == rehearsalId).Count() > 0)
                {
                    sdate = DataContext.EventInstances.Where(i => i.EventId == rehearsalId).Max(i => i.DateTime).AddDays(1);
                }
                var daylength = (Convert.ToDateTime(reh.EndDate) - reh.StartDate).TotalDays;
                sdate = ApplicationUtility.GetNextDate(reh.StartDate, Enums.Frequency.Weekly, 1, reh.Day, sdate, null);

                for (int i = 1; i <= eventCount; i++)
                {
                    reh.EventInstances.Add(new EventInstance() { DateTime = sdate, EndDate = sdate.AddDays(daylength), PlaceId = reh.PlaceId, StartTime = Convert.ToDateTime(sdate.ToShortDateString() + " " + ((DateTime)reh.StartTime).ToShortTimeString()), EndTime = Convert.ToDateTime(sdate.AddDays(daylength).ToShortDateString() + " " + ((DateTime)reh.EndTime).ToShortTimeString()) });
                    sdate = ApplicationUtility.GetNextDate(sdate, Enums.Frequency.Weekly, 1, reh.Day, sdate.AddDays(1), null);
                }
                DataContext.SaveChanges();
            }

            return RedirectToAction("Manage", new { id = reh.TeamId });
        }

        [Route("Team/AddTeamAJAX")]
        [Authorize(Roles = "Owner,Teacher")]
        [HttpPost]
        public ActionResult AddTeamAJAX(TeamCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userid = User.Identity.GetUserId();
                model.Team.DanceStyles = DataContext.DanceStyles.Where(s => model.DanceStyleId.Contains(s.Id.ToString())).ToList();
                model.Team.Teachers = new List<Teacher>();
                model.Team.Teachers.Add(DataContext.Teachers.Single(t => t.ApplicationUser.Id == userid));
                model.Team.Members = new List<OrganizationMember>();
                model.Team.Members.Add(new OrganizationMember() { UserId = userid, Admin = true, OrganizationId = model.Team.Id });
                DataContext.Teams.Add(model.Team);
                DataContext.SaveChanges();

                var teams = new List<Team>();
                if (model.SchoolId != null)
                {
                    teams = DataContext.Teams.Where(t => t.SchoolId == model.SchoolId).ToList();
                }
                else
                {
                    teams = DataContext.Teams.Where(t => t.Teachers.Any(te => te.Id == model.TeacherId)).ToList();
                }

                return PartialView("~/Views/Shared/_TeamsPartial.cshtml", teams);
            }
            else
            {
                return null;
            }

        }

        [Route("Team/SearchGroupJSON")]
        [Authorize(Roles = "Owner,Promoter,Teacher")]
        public JsonResult SearchGroupJSON(Uri search)
        {
            if (search.Host.Contains("facebook.com"))
            {
                var type = search.Segments[1].Replace("/", "");
                var id = search.Segments[2].Replace("/", "");
                long fbid;
                dynamic obj;

                if (long.TryParse(id, out fbid))
                {
                    obj = FacebookHelper.Get(FacebookHelper.GetToken(), fbid.ToString());
                }
                else
                {
                    obj = FacebookHelper.Search(FacebookHelper.GetToken(), id, "group");
                }
                return Json(obj, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return null;
            }
        }

        [Route("Team/Search")]
        [Authorize]
        public JsonResult Search(string searchString)
        {
            var users = DataContext.Users.Where(u => (u.FirstName + " " + u.LastName).Contains(searchString)).Select(s => new { Id = s.Id, Name = s.FirstName + " " + s.LastName }).ToList();
            return Json(users, JsonRequestBehavior.AllowGet);
        }

        //[Authorize(Roles = "Owner,Teacher")]
        //[HttpPost]
        //public ActionResult ImportTeamAJAX(TeamCreateViewModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var fbgroup = FacebookHelper.GetGroup()
        //        model.Team.DanceStyles = DataContext.DanceStyles.Where(s => model.DanceStyleId.Contains(s.Id.ToString())).ToList();
        //        DataContext.Teams.Add(model.Team);
        //        DataContext.SaveChanges();

        //        var teams = new List<Team>();
        //        if (model.SchoolId != null)
        //        {
        //            teams = DataContext.Teams.Where(t => t.SchoolId == model.SchoolId).ToList();
        //        }
        //        else
        //        {
        //            teams = DataContext.Teams.Where(t => t.Teachers.Any(te => te.Id == model.TeacherId)).ToList();
        //        }

        //        return PartialView("~/Views/Shared/_TeamsPartial.cshtml", teams);
        //    }
        //    else
        //    {
        //        return null;
        //    }

        //}

        [Route("Team/GetEventInstances")]
        public JsonResult GetEventInstances(DateTime start, DateTime end, int teamId)
        {
            var instances = new List<EventInstance>();
            var userid = User.Identity.GetUserId();
            var team = DataContext.Teams
                                .Include("Auditions.EventInstances.EventRegistrations")
                                .Include("Performances.EventInstances.EventRegistrations")
                                .Include("Rehearsals.EventInstances.EventRegistrations")
                                .Where(t => t.Id == teamId).FirstOrDefault();
            instances.AddRange(team.Auditions.SelectMany(a => a.EventInstances).ToList());
            instances.AddRange(team.Performances.SelectMany(a => a.EventInstances).ToList());
            instances.AddRange(team.Rehearsals.SelectMany(a => a.EventInstances).ToList());

            return Json(instances.AsEnumerable().Select(s =>
                        new {
                            id = s.EventId,
                            instanceid = s.Id,
                            title = s.Event.Name,
                            start = s.StartTime.Value.ToString("o"),
                            end = s.EndTime.Value.ToString("o"),
                            starttext = ((DateTime)s.StartTime).ToLongDateString(),
                            starttimetext = ((DateTime)s.StartTime).ToShortTimeString(),
                            endtimetext = ((DateTime)s.EndTime).ToShortTimeString(),
                            place = s.Event.Place.Name,
                            address = s.Event.Place.Address,
                            city = s.Event.Place.City,
                            state = s.Event.Place.State,
                            zip = s.Event.Place.Zip,
                            lat = s.Event.Place.Latitude,
                            lng = s.Event.Place.Longitude,
                            reg = s.EventRegistrations.Where(r => r.UserId == userid).Count() > 0 ? true : false,
                            color = s.Event is Class ? "#65AE25" : s.Event is Social ? "#006A90" : s.Event is Performance ? "#f0ad4e" : s.Event is Rehearsal ? "#d9534f" : "#428bca",
                            url = Url.Action("View", "Team", new { id = teamId })
                        }), JsonRequestBehavior.AllowGet);
        }

        [Route("Team/GetAuditionInstances")]
        public JsonResult GetAuditionInstances(double? neLat, double? neLng, double? swLat, double? swLng, int? styleId, string teacherId, int[] skillLevel, DateTime start, DateTime end)
        {
            var auditions = DataContext.Auditions
                                .Include("EventInstances.EventRegistrations")
                                .Where(a => a.StartDate >= start && a.StartDate <= end);
            if (styleId != null)
            {
                auditions = auditions.Where(c => c.Team.DanceStyles.Select(st => st.Id).Contains((int)styleId));
            }
            if (teacherId != null && teacherId != "")
            {
                auditions = auditions.Where(c => c.Team.Teachers.Select(t => t.ApplicationUser.Id).Contains(teacherId));
            }

            if (neLat != null && swLng != null)
            {
                auditions = auditions.Where(c => c.Team.Longitude >= swLng && c.Team.Longitude <= neLng && c.Team.Latitude >= swLat && c.Team.Latitude <= neLat);
            }
            if (skillLevel != null)
            {
                auditions = auditions.Where(x => skillLevel.Contains(x.Team.SkillLevel));
            }

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

        [Route("Team/GetPerformanceInstances")]
        public JsonResult GetPerformanceInstances(double? neLat, double? neLng, double? swLat, double? swLng, int? styleId, string teacherId, int[] skillLevel, DateTime start, DateTime end)
        {
            var performances = DataContext.Performances
                                .Include("EventInstances.EventRegistrations")
                                .Where(a => a.StartDate >= start && a.StartDate <= end);
            if (styleId != null)
            {
                performances = performances.Where(c => c.Team.DanceStyles.Select(st => st.Id).Contains((int)styleId));
            }
            if (teacherId != null && teacherId != "")
            {
                performances = performances.Where(c => c.Team.Teachers.Select(t => t.ApplicationUser.Id).Contains(teacherId));
            }

            if (neLat != null && swLng != null)
            {
                performances = performances.Where(c => c.Team.Longitude >= swLng && c.Team.Longitude <= neLng && c.Team.Latitude >= swLat && c.Team.Latitude <= neLat);
            }
            if (skillLevel != null)
            {
                performances = performances.Where(x => skillLevel.Contains(x.Team.SkillLevel));
            }

            var performancelist = performances.ToList().Take(100);

            var userid = User.Identity.GetUserId();

            return Json(performancelist.AsEnumerable().SelectMany(a => a.EventInstances.Where(i => i.DateTime >= start && i.DateTime <= end).Select(s =>
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

        // POST: Team
        [Route("Team/UpdateMembers")]
        [HttpPost]
        [Authorize]
        public ActionResult UpdateMembers(TeamManageViewModel model)
        {
            if (model.Team.Members.Where(m => m.Admin).Count() > 0)
            {
                foreach (var m in model.Team.Members)
                {
                    var mem = DataContext.OrganizationMembers.Where(om => om.Id == m.Id).FirstOrDefault();
                    mem.Admin = m.Admin;
                }
                DataContext.SaveChanges();
            }
            return RedirectToAction("Manage", new { id = model.Team.Id });
        }

        [Route("Team/RemoveMember")]
        [Authorize(Roles = "Teacher")]
        public ActionResult RemoveMember(int id, string memberid)
        {
            DataContext.OrganizationMembers.Remove(DataContext.OrganizationMembers.Single(m => m.OrganizationId == id && m.UserId == memberid));
            DataContext.SaveChanges();
            return RedirectToAction("Manage", new { id = id });
        }

        [Route("Team/UploadImageAsync")]
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
                    var team = DataContext.Teams.Single(s => s.Id == id);
                    EDR.Utilities.ApplicationUtility.DeletePicture(new Picture() { Filename = team.PhotoUrl });
                    team.PhotoUrl = newFile.FilePath;
                    DataContext.SaveChanges();
                }
            }
            var objUpload = new { FilePath = Url.Content(newFile.FilePath), UploadStatus = newFile.UploadStatus };
            return Json(objUpload, JsonRequestBehavior.AllowGet);
        }

        [Route("Team/AddYouTubePlaylist")]
        [Authorize(Roles = "Teacher")]
        public PartialViewResult AddYouTubePlaylist(TeamManageViewModel model)
        {
            var ytPlaylist = YouTubeHelper.GetPlaylist(model.NewYoutubePlayList.ToString());
            var userid = User.Identity.GetUserId();
            var auth = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();
            var team = DataContext.Teams.Where(t => t.Id == model.Team.Id).Include("Playlists").FirstOrDefault();
            //  var ePlaylist = new EventPlaylist() { Title = ytPlaylist.Name, PublishDate = ytPlaylist.PubDate, YouTubeId = ytPlaylist.Id, Author = auth, YouTubeUrl = ytPlaylist.Url, CoverPhoto = ytPlaylist.ThumbnailUrl, MediaSource = MediaSource.YouTube };

            if (team.Playlists.Where(p => p.YouTubeId == ytPlaylist.Id).Count() == 0)
            {
                team.Playlists.Add(new OrganizationPlaylist() { Title = ytPlaylist.Name, PublishDate = ytPlaylist.PubDate, YouTubeId = ytPlaylist.Id, Author = auth, YouTubeUrl = ytPlaylist.Url.ToString(), CoverPhoto = ytPlaylist.ThumbnailUrl.ToString(), MediaSource = EDR.Enums.MediaSource.YouTube, UpdatedDate = ytPlaylist.PubDate, VideoCount = ytPlaylist.VideoCount });
                DataContext.Entry(team).State = EntityState.Modified;
                DataContext.SaveChanges();
                ViewBag.Message = "Playlist was imported";
            }
            return PartialView("~/Views/Shared/Team/_ManagePlaylistsPartial.cshtml", team.Playlists);
        }

        [Route("Team/DeletePlaylist")]
        [Authorize(Roles = "Teacher")]
        public PartialViewResult DeletePlaylist(int id, int playListId)
        {
            var playlists = DataContext.Teams.Include("Playlists").Single(e => e.Id == id).Playlists;
            playlists.Remove(playlists.Single(l => l.Id == playListId));
            DataContext.SaveChanges();
            ViewBag.Message = "Playlist was removed";
            return PartialView("~/Views/Shared/Team/_ManagePlaylistsPartial.cshtml", playlists);
        }

        [Route("Team/AddYouTubeVideo")]
        [Authorize(Roles = "Teacher")]
        public PartialViewResult AddYouTubeVideo(TeamManageViewModel model)
        {
            var ytVideo = YouTubeHelper.GetVideo(model.NewYouTubeVideo.ToString());
            var userid = User.Identity.GetUserId();
            var auth = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();
            var team = DataContext.Teams.Where(e => e.Id == model.Team.Id).Include("Videos").FirstOrDefault();
            //  var ePlaylist = new EventPlaylist() { Title = ytPlaylist.Name, PublishDate = ytPlaylist.PubDate, YouTubeId = ytPlaylist.Id, Author = auth, YouTubeUrl = ytPlaylist.Url, CoverPhoto = ytPlaylist.ThumbnailUrl, MediaSource = MediaSource.YouTube };

            if (team.Videos.Where(p => p.YoutubeId == ytVideo.Id).Count() == 0)
            {
                team.Videos.Add(new OrganizationVideo() { Title = ytVideo.Title, PublishDate = ytVideo.PubDate, YoutubeId = ytVideo.Id, Author = auth, YoutubeUrl = ytVideo.VideoLink.ToString(), PhotoUrl = ytVideo.Thumbnail.ToString(), MediaSource = EDR.Enums.MediaSource.YouTube });
                DataContext.Entry(team).State = EntityState.Modified;
                DataContext.SaveChanges();
                ViewBag.Message = "Video was imported";
            }
            return PartialView("~/Views/Shared/Team/_ManageVideosPartial.cshtml", team.Videos);
        }

        [Route("Team/DeleteVideo")]
        [Authorize(Roles = "Teacher")]
        public PartialViewResult DeleteVideo(int id, int videoId)
        {
            var videos = DataContext.Teams.Include("Videos").Single(e => e.Id == id).Videos;
            videos.Remove(videos.Single(l => l.Id == videoId));
            DataContext.SaveChanges();
            ViewBag.Message = "Video was removed";
            return PartialView("~/Views/Shared/Team/_ManageVideosPartial.cshtml", videos);
        }

        private List<OrganizationVideo> GetVideos(int id)
        {
            var team = DataContext.Teams
                            .Include("Videos.Author")
                            .Include("Playlists.Author")
                            .Single(t => t.Id == id);
            var videos = new List<OrganizationVideo>();
            videos = team.Videos.ToList();

            //  Extract YouTube Playlists
            if (team.Playlists != null)
            {
                foreach (var lst in team.Playlists)
                {
                    var ytids = videos.Select(v => v.YoutubeId).ToArray();
                    videos.AddRange(YouTubeHelper.GetPlaylistVideos(lst.YouTubeId).Where(v => !ytids.Contains(v.Id)).Select(v => new OrganizationVideo() { Author = lst.Author, MediaSource = EDR.Enums.MediaSource.YouTube, YoutubeId = v.Id, PhotoUrl = v.Thumbnail.ToString(), PublishDate = v.PubDate, Title = v.Title, VideoUrl = v.VideoLink.ToString(), YouTubePlaylistTitle = lst.Title, YouTubePlaylistUrl = lst.YouTubeUrl, YoutubeThumbnail = v.Thumbnail.ToString(), YoutubeUrl = v.VideoLink.ToString() }));
                    //  lstMedia.AddRange(videos.Select(v => new EventMedia() { Event = evt, SourceName = v.Title, SourceLink = v.VideoLink.ToString(), MediaDate = v.PubDate, MediaType = Enums.MediaType.Video, PhotoUrl = v.Thumbnail.ToString(), MediaUrl = v.VideoLink.ToString(), Title = v.Title, MediaSource = MediaSource.YouTube, Target = target, Playlist = lst, Author = lst.Author }).ToList());
                }
            }

            return videos;
        }

        [Route("Team/GetVideosJSON")]
        public JsonResult GetVideosJSON(int id)
        {
            //  Get Linked Objects Videos
            //  Get Facebook Group/Event/Page Feed Videos

            //  Get YouTube Playlist Videos
            //  Add Videos
            var videos = GetVideos(id);
            return Json(videos.OrderByDescending(v => v.PublishDate).Select(v => new { PhotoUrl = v.PhotoUrl, VideoUrl = v.VideoUrl, Title = v.Title, Id = v.YoutubeId }), JsonRequestBehavior.AllowGet);
        }
    }
}

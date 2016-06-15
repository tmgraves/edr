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

namespace EDR.Controllers
{
    public class TeamController : BaseController
    {
        // GET: Team
        public ActionResult Index(TeamIndexViewModel model)
        {
            model.Teams = DataContext.Teams
                                .Include("Teachers.ApplicationUser")
                                .Include("DanceStyles")
                                .Include("Reviews")
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
            return View(model);
        }

        // GET: Team/Details/5
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
            return View(model);
        }

        [Authorize(Roles = "Teacher")]
        public ActionResult Manage(int id)
        {
            var model = new TeamManageViewModel();
            model.Team = DataContext.Teams
                            .Include("Members.User")
                            .Include("Rehearsals.Place")
                            .Include("Auditions.Place")
                            .Include("Performances.Place")
                            .Include("Performances.Event.Place")
                            .Include("DanceStyles")
                            .Include("School")
                            .Single(e => e.Id == id);
            if (model.Team == null)
            {
                return HttpNotFound();
            }
            return View(model);
        }

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
        [Authorize(Roles = "Teacher")]
        public ActionResult Create(int? schoolId)
        {
            var model = new TeamCreateViewModel();
            model.Team.SchoolId = schoolId;
            return View(model);
        }

        // POST: Team/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
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

        // GET: Team/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var model = new TeamDeleteViewModel();
            model.Team = DataContext.Teams.Find(id);
            if (model.Team == null)
            {
                return HttpNotFound();
            }
            return View(model);
        }

        // POST: Team/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Team team = DataContext.Teams.Find(id);
            DataContext.Teams.Remove(team);
            DataContext.SaveChanges();
            return RedirectToAction("Index");
        }

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

        [Authorize(Roles = "Teacher,Owner")]
        public ActionResult RemoveTeacher(int id, int teacherid)
        {
            DataContext.Teams.Where(s => s.Id == id).Include("Teachers").FirstOrDefault().Teachers.Remove(DataContext.Teachers.Single(t => t.Id == teacherid));
            DataContext.SaveChanges();
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

        [Authorize(Roles = "Teacher")]
        [HttpPost]
        public ActionResult AddRehearsal(TeamManageViewModel model)
        {
            if (ModelState.IsValidField("NewRehearsal"))
            {
                DataContext.Rehearsals.Add(model.NewRehearsal);
                DataContext.SaveChanges();
                return RedirectToAction("Manage", new { id = model.NewRehearsal.TeamId });
            }
            else
            {
                return View(model);
            }
        }

        [Authorize(Roles = "Teacher")]
        public ActionResult DeleteRehearsal(int id)
        {
            var rehearsal = DataContext.Rehearsals.Single(s => s.Id == id);
            var teamId = rehearsal.TeamId;
            DataContext.Rehearsals.Remove(rehearsal);
            DataContext.SaveChanges();
            return RedirectToAction("Manage", new { id = teamId });
        }

        [Authorize(Roles = "Teacher")]
        [HttpPost]
        public ActionResult AddAudition(TeamManageViewModel model)
        {
            try
            {
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

        [Authorize(Roles = "Teacher")]
        public ActionResult DeleteAudition(int id)
        {
            var audition = DataContext.Auditions.Single(s => s.Id == id);
            var teamId = audition.TeamId;
            DataContext.Auditions.Remove(audition);
            DataContext.SaveChanges();
            return RedirectToAction("Manage", new { id = teamId });
        }

        [Authorize(Roles = "Teacher")]
        [HttpPost]
        public ActionResult AddPerformance(TeamManageViewModel model)
        {
            try
            {
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

        [Authorize(Roles = "Teacher")]
        public ActionResult DeletePerformance(int id)
        {
            var performance = DataContext.Performances.Single(s => s.Id == id);
            var teamId = performance.TeamId;
            DataContext.Performances.Remove(performance);
            DataContext.SaveChanges();
            return RedirectToAction("Manage", new { id = teamId });
        }

        [Authorize(Roles = "Owner,Promoter,Teacher")]
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

        [Authorize(Roles = "Owner,Promoter,Teacher")]
        public ActionResult DeleteStyle(int id, int styleId)
        {
            DataContext.Teams.Include("DanceStyles").Single(e => e.Id == id).DanceStyles.Remove(DataContext.DanceStyles.Single(s => s.Id == styleId));
            DataContext.SaveChanges();
            return RedirectToAction("Manage", new { id = id });
        }

        [HttpGet]
        public virtual ActionResult GetAuditionsPartial(int id)
        {
            var start = DateTime.Today;
            var auditions = DataContext.Auditions
                                .Include("Place")
                                .Where(a => a.StartDate >= start && a.TeamId == id);
            return PartialView("~/Views/Shared/DisplayTemplates/Auditions.cshtml", auditions);
        }

        [HttpGet]
        public virtual ActionResult GetPerformancesPartial(int id)
        {
            var start = DateTime.Today;
            var performances = DataContext.Performances
                                .Include("Place")
                                .Where(p => p.TeamId == id);
            return PartialView("~/Views/Shared/DisplayTemplates/Performances.cshtml", performances);
        }

        // POST: Team
        [HttpPost]
        [Authorize]
        public ActionResult UpdateMembers(TeamManageViewModel model)
        {
            foreach (var m in model.Team.Members)
            {
                var mem = DataContext.OrganizationMembers.Where(om => om.Id == m.Id).FirstOrDefault();
                mem.Admin = m.Admin;
            }
            DataContext.SaveChanges();
            return RedirectToAction("Manage", new { id = model.Team.Id });
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
                    var team = DataContext.Teams.Single(s => s.Id == id);
                    EDR.Utilities.ApplicationUtility.DeletePicture(new Picture() { Filename = team.PhotoUrl });
                    team.PhotoUrl = newFile.FilePath;
                    DataContext.SaveChanges();
                }
            }
            var objUpload = new { FilePath = Url.Content(newFile.FilePath), UploadStatus = newFile.UploadStatus };
            return Json(objUpload, JsonRequestBehavior.AllowGet);
        }
    }
}

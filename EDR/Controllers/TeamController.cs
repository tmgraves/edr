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

namespace EDR.Controllers
{
    public class TeamController : BaseController
    {
        // GET: Team
        public ActionResult Index()
        {
            var model = new TeamIndexViewModel();
            model.Teams = DataContext.Teams.ToList();
            return View(model);
        }

        // GET: Team/Details/5
        public ActionResult View(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var model = new TeamViewViewModel();
            model.Team = DataContext.Teams.Find(id);
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
                            .Single(e => e.Id == id);
            if (model.Team == null)
            {
                return HttpNotFound();
            }
            return View(model);
        }

        [Authorize(Roles = "Teacher")]
        [HttpPost]
        public ActionResult Save(TeamManageViewModel model)
        {
            var t = ModelState.IsValidField("Team");
            if (ModelState.IsValidField("Team"))
            {
                DataContext.Entry(model.Team).State = EntityState.Modified;
                DataContext.SaveChanges();
                return RedirectToAction("Manage", new { id = model.Team.Id });
            }
            return View(model);
        }

        // GET: Team/Create
        public ActionResult Create(int? schoolId)
        {
            var model = new TeamCreateViewModel();
            model.Team.SchoolId = schoolId;
            return View(model);
        }

        // POST: Team/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(TeamCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
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
    }
}

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
        public ActionResult Manage(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var model = new TeamManageViewModel();
            model.Team = DataContext.Teams
                            .Include("Members.User")
                            .Include("Rehearsals.Place")
                            .Include("Auditions.Place")
                            .Include("Performances.Place")
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
            if (ModelState.IsValid)
            {
                DataContext.Entry(model.Team).State = EntityState.Modified;
                DataContext.SaveChanges();
                return RedirectToAction("View", new { id = model.Team.Id });
            }
            return View(model);
        }

        // GET: Team/Create
        public ActionResult Create()
        {
            return View();
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
    }
}

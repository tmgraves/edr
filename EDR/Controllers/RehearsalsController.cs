using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using EDR.Models;

namespace EDR.Controllers
{
    public class RehearsalsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Rehearsals
        public ActionResult Index()
        {
            return View(db.Rehearsals.ToList());
        }

        // GET: Rehearsals/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Rehearsal rehearsal = db.Rehearsals.Find(id);
            if (rehearsal == null)
            {
                return HttpNotFound();
            }
            return View(rehearsal);
        }

        // GET: Rehearsals/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Rehearsals/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,IsPublic,Name,Summary,Description,IsAvailable")] Rehearsal rehearsal)
        {
            if (ModelState.IsValid)
            {
                db.Rehearsals.Add(rehearsal);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(rehearsal);
        }

        // GET: Rehearsals/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Rehearsal rehearsal = db.Rehearsals.Find(id);
            if (rehearsal == null)
            {
                return HttpNotFound();
            }
            return View(rehearsal);
        }

        // POST: Rehearsals/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,IsPublic,Name,Summary,Description,IsAvailable")] Rehearsal rehearsal)
        {
            if (ModelState.IsValid)
            {
                db.Entry(rehearsal).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(rehearsal);
        }

        // GET: Rehearsals/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Rehearsal rehearsal = db.Rehearsals.Find(id);
            if (rehearsal == null)
            {
                return HttpNotFound();
            }
            return View(rehearsal);
        }

        // POST: Rehearsals/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Rehearsal rehearsal = db.Rehearsals.Find(id);
            db.Rehearsals.Remove(rehearsal);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

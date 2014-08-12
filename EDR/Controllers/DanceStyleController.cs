﻿using System;
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
    public class DanceStyleController : BaseController
    {
        // GET: DanceStyle/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var viewModel = new DanceStyleDetailViewModel();
            viewModel.DanceStyle = DataContext.DanceStyles.Find(id);
            viewModel.Classes = DataContext.Events.Include("Teachers").Include("DanceStyles").Include("Users").OfType<Class>().Where(c => c.DanceStyles.Any(s => s.Id == id)).Where(x => x.IsAvailable == true).Where(y => y.StartDate >= DateTime.Now).OrderBy(z => z.StartDate).ToList();
            viewModel.Socials = DataContext.Events.Include("DanceStyles").Include("Users").OfType<Social>().Where(c => c.DanceStyles.Any(s => s.Id == id)).Where(x => x.IsAvailable == true).Where(y => y.StartDate >= DateTime.Now).OrderBy(z => z.StartDate).ToList();
            viewModel.Concerts = DataContext.Events.Include("DanceStyles").Include("Users").OfType<Concert>().Where(c => c.DanceStyles.Any(s => s.Id == id)).Where(x => x.IsAvailable == true).Where(y => y.StartDate >= DateTime.Now).OrderBy(z => z.StartDate).ToList();
            viewModel.Conferences = DataContext.Events.Include("DanceStyles").Include("Users").OfType<Conference>().Where(c => c.DanceStyles.Any(s => s.Id == id)).Where(x => x.IsAvailable == true).Where(y => y.StartDate >= DateTime.Now).OrderBy(z => z.StartDate).ToList();
            viewModel.Parties = DataContext.Events.Include("DanceStyles").Include("Users").OfType<Party>().Where(c => c.DanceStyles.Any(s => s.Id == id)).Where(x => x.IsAvailable == true).Where(y => y.StartDate >= DateTime.Now).OrderBy(z => z.StartDate).ToList();
            viewModel.OpenHouses = DataContext.Events.Include("DanceStyles").Include("Users").OfType<OpenHouse>().Where(c => c.DanceStyles.Any(s => s.Id == id)).Where(x => x.IsAvailable == true).Where(y => y.StartDate >= DateTime.Now).OrderBy(z => z.StartDate).ToList();

            return View(viewModel);
        }

        // GET: DanceStyle/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: DanceStyle/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,Description,YouTubeVideoID")] DanceStyle danceStyle)
        {
            if (ModelState.IsValid)
            {
                DataContext.DanceStyles.Add(danceStyle);
                DataContext.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(danceStyle);
        }

        // GET: DanceStyle/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DanceStyle danceStyle = DataContext.DanceStyles.Find(id);
            if (danceStyle == null)
            {
                return HttpNotFound();
            }
            return View(danceStyle);
        }

        // POST: DanceStyle/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Description,YouTubeVideoID")] DanceStyle danceStyle)
        {
            if (ModelState.IsValid)
            {
                DataContext.Entry(danceStyle).State = EntityState.Modified;
                DataContext.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(danceStyle);
        }

        // GET: DanceStyle/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DanceStyle danceStyle = DataContext.DanceStyles.Find(id);
            if (danceStyle == null)
            {
                return HttpNotFound();
            }
            return View(danceStyle);
        }

        // POST: DanceStyle/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            DanceStyle danceStyle = DataContext.DanceStyles.Find(id);
            DataContext.DanceStyles.Remove(danceStyle);
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

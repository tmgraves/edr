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
    public class DanceStyleController : BaseController
    {
        [Route("Styles")]
        public ActionResult List()
        {
            var viewModel = new HomeExploreViewModel();
            viewModel.DanceStyles = DataContext.DanceStyles
                                        .Include("Dancers")
                                        .Include("Teachers")
                                        .Include("Events")
                                        .ToList();
            return View(viewModel);
        }

        // GET: DanceStyle/Details/5
        [Route("Style/{styleName}")]
        public ActionResult Details(string styleName)
        {
            styleName = styleName.Replace('_', ' ');

            var viewModel = DataContext.DanceStyles.Include("Teachers").Include("Videos").Where(x => x.Name == styleName).FirstOrDefault();

            if (viewModel == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            return View(viewModel);
        }

        // GET: DanceStyle/Create
        [Route("Style/Create")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: DanceStyle/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Route("Style/Create")]
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
        [Route("Style/Edit")]
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
        [Route("Style/Edit")]
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
        [Route("Style/Delete")]
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
        [Route("Style/Delete")]
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

        [Route("Style/Index")]
        public ActionResult Index()
        {
            NewStyleViewModel viewModel = new NewStyleViewModel
            {
                NewDanceStyle = new DanceStyle(),
                DanceStyles = DataContext.DanceStyles
            };
            return View(viewModel);
        }

        [Route("Style/Index_AddItem")]
        public ActionResult Index_AddItem(NewStyleViewModel viewModel)
        {
            var x = ModelState.IsValid;
            DataContext.DanceStyles.Add(viewModel.NewDanceStyle);
            DataContext.SaveChanges();

            return PartialView("_DanceStyleListPartial", DataContext.DanceStyles);
        }

        [Route("Style/Search")]
        public JsonResult Search(string searchString)
        {
            var styles = DataContext.DanceStyles.Where(s => s.Name.Contains(searchString)).Select(s => new { Id = s.Id, Name = s.Name }).ToList();
            return Json(styles, JsonRequestBehavior.AllowGet);
        }

        [Route("Style/GetTeachersPartial")]
        [HttpGet]
        public virtual ActionResult GetTeachersPartial(int id)
        {
            var teachers = DataContext.Teachers.Where(t => t.DanceStyles.Any(s => s.Id == id)).Take(20).ToList();
            return PartialView("~/Views/Shared/DisplayTemplates/TeacherThumbLinks.cshtml", teachers);
        }

        [Route("Style/GetSchoolsPartial")]
        [HttpGet]
        public virtual ActionResult GetSchoolsPartial(int id)
        {
            var schools = DataContext.Schools.Where(s => s.Classes.Any(c => c.DanceStyles.Any(st => st.Id == id))).Take(20).ToList();
            return PartialView("~/Views/Shared/DisplayTemplates/SchoolLinks.cshtml", schools);
        }

        [Route("Style/GetTeamsPartial")]
        [HttpGet]
        public virtual ActionResult GetTeamsPartial(int id)
        {
            var teams = DataContext.Teams.Where(t => t.DanceStyles.Any(s => s.Id == id)).Take(20).ToList();
            return PartialView("~/Views/Shared/DisplayTemplates/TeamLinks.cshtml", teams);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EDR.Controllers;
using System.Data.Entity;
using EDR.Utilities;
using System.IO;
using EDR.Models;

namespace EDR.Areas.Admin.Controllers
{
    public class DanceStyleController : BaseController
    {
        // GET: Admin/DanceStyle
        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            var styles = DataContext.DanceStyles;
            return View(styles);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Details(int id)
        {
            var style = DataContext.DanceStyles.Where(s => s.Id == id).Include("Videos").FirstOrDefault();
            return View(style);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult UploadPicture(HttpPostedFileBase file, int id)
        {
            UploadFile newFile = ApplicationUtility.LoadPicture(file);

            if (newFile.UploadStatus == "Success")
            {
                DataContext.DanceStyles.Where(s => s.Id == id).FirstOrDefault().PhotoUrl = newFile.FilePath;
                DataContext.SaveChanges();
                return RedirectToAction("Details", "DanceStyle", new { id = id } );
            }
            else
            {
                ViewBag.Message = newFile.UploadStatus;
                return RedirectToAction("Details", "DanceStyle", new { id = id });
            }
        }
    }
}
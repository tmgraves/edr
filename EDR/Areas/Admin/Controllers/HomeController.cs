using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EDR.Areas.Admin.Models.ViewModels;
using EDR.Controllers;
using EDR.Models;
using EDR.Models.ViewModels;
using System.Data.Entity;
using System.Drawing;

namespace EDR.Areas.Admin.Controllers
{
    public class HomeController : BaseController
    {
        // GET: Admin/Home
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Photos()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        public ActionResult ResizePictures()
        {
            var users = DataContext.Users;

            foreach(var u in users)
            {
                if (u.PhotoUrl != null)
                {
                    try
                    {
                        var oldpath = Server.MapPath(u.PhotoUrl);

                        if (oldpath.Contains("MyUploads"))
                        {
                            var newpath = Server.MapPath("~/MyUploads/newfile");
                            var image = Image.FromFile(oldpath);
                            EDR.Utilities.ApplicationUtility.CompressImage(image, 20, newpath);
                            image.Dispose();
                            System.IO.File.Delete(oldpath);
                            System.IO.File.Move(newpath, oldpath);
                        }
                    }
                    catch(Exception ex)
                    {
                        var msg = ex.Message;
                    }
                }
            }
            return RedirectToAction("Photos");
        }
    }
}
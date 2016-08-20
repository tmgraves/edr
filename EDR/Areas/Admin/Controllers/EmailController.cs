using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EDR.Models;

namespace EDR.Areas.Admin.Controllers
{
    public class EmailController : Controller
    {
        // GET: Admin/Email
        [Authorize(Roles = "Admin")]
        public ActionResult DashBoard()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        public ActionResult SendConfirmations()
        {
            var result = EmailProcess.SendConfirmEmails();
            return RedirectToAction("Dashboard");
        }

        [Authorize(Roles = "Admin")]
        public ActionResult ExpiringEvents()
        {
            var result = EmailProcess.ExpiringEvents();
            return RedirectToAction("Dashboard");
        }

        [Authorize(Roles = "Admin")]
        public ActionResult ProcessEmails()
        {
            EmailProcess.ProcessEmails();
            return RedirectToAction("Dashboard");
        }

        [Authorize(Roles = "Admin")]
        public ActionResult EventSummaries()
        {
            var result = EmailProcess.EventSummaries();
            return RedirectToAction("Dashboard");
        }

        [Authorize(Roles = "Admin")]
        public ActionResult DailyDancerSummaries()
        {
            var result = EmailProcess.DailyDancerSummaries();
            return RedirectToAction("Dashboard");
        }
    }
}
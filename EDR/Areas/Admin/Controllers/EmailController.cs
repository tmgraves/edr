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
        public ActionResult DashBoard()
        {
            return View();
        }

        public ActionResult SendConfirmations()
        {
            var result = EmailProcess.SendConfirmEmails();
            return RedirectToAction("Dashboard");
        }

        public ActionResult ExpiringEvents()
        {
            var result = EmailProcess.ExpiringEvents();
            return RedirectToAction("Dashboard");
        }

        public ActionResult ProcessEmails()
        {
            EmailProcess.ProcessEmails();
            return RedirectToAction("Dashboard");
        }

        public ActionResult EventSummaries()
        {
            var result = EmailProcess.EventSummaries();
            return RedirectToAction("Dashboard");
        }

        public ActionResult DailyDancerSummaries()
        {
            var result = EmailProcess.DailyDancerSummaries();
            return RedirectToAction("Dashboard");
        }
    }
}
using EDR.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EDR.Models.ViewModels;
using System.Threading.Tasks;
using System.Data.Entity.Validation;

namespace EDR.Controllers
{
    public class AdminController : BaseController
    {
        // GET: Admin
        [Authorize(Roles="Admin")]
        public ActionResult Approvals()
        {
            var viewModel = new AdminApprovalsViewModel();
            viewModel.Teachers = DataContext.Teachers.Include("ApplicationUser").Where(x => x.Approved == null);
            viewModel.Owners = DataContext.Owners.Include("ApplicationUser").Where(x => x.Approved == null);
            viewModel.Promoters = DataContext.Promoters.Include("ApplicationUser").Where(x => x.Approved == null);
            return View(viewModel);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult ApproveTeacher(string teacherId, bool approved)
        {
            var teacher = DataContext.Teachers.Where(x => x.ApplicationUser.Id == teacherId).FirstOrDefault();
            teacher.Approved = approved;
            teacher.ApproveDate = DateTime.Today;
            if (ModelState.IsValid)
            {
                try
                {
                    DataContext.Entry(teacher).State = EntityState.Modified;
                    DataContext.SaveChanges();
                    return RedirectToAction("Approvals");
                }
                catch (DbEntityValidationException ex)
                {
                    return View();
                }
            }
            return View();
        }

        [Authorize(Roles = "Admin")]
        public ActionResult ApproveOwner(string ownerId, bool approved)
        {
            var owner = DataContext.Owners.Where(x => x.ApplicationUser.Id == ownerId).FirstOrDefault();
            owner.Approved = approved;
            owner.ApproveDate = DateTime.Today;
            if (ModelState.IsValid)
            {
                try
                {
                    DataContext.Entry(owner).State = EntityState.Modified;
                    DataContext.SaveChanges();
                    return RedirectToAction("Approvals");
                }
                catch (DbEntityValidationException ex)
                {
                    return View();
                }
            }
            return View();
        }

        [Authorize(Roles = "Admin")]
        public ActionResult ApprovePromoter(string promoterId, bool approved)
        {
            var promoter = DataContext.Promoters.Where(x => x.ApplicationUser.Id == promoterId).FirstOrDefault();
            promoter.Approved = approved;
            promoter.ApproveDate = DateTime.Today;
            if (ModelState.IsValid)
            {
                try
                {
                    DataContext.Entry(promoter).State = EntityState.Modified;
                    DataContext.SaveChanges();
                    return RedirectToAction("Approvals");
                }
                catch (DbEntityValidationException ex)
                {
                    return View();
                }
            }
            return View();
        }
    }
}
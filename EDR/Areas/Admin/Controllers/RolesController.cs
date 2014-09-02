﻿using EDR.Controllers;
using EDR.Models;
using EDR.Models.ViewModels;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;

namespace EDR.Areas.Admin.Controllers
{
    public class RolesController : BaseController
    {
        // GET: Admin/Roles
        public ActionResult Index()
        {
            return View();
        }

        // GET: Admin
        [Authorize(Roles = "Admin")]
        public ActionResult Approvals()
        {
            var viewModel = new RoleApprovalsViewModel();
            viewModel.Teachers = DataContext.Teachers.Include("ApplicationUser").Where(x => x.Approved == null);
            viewModel.Owners = DataContext.Owners.Include("ApplicationUser").Where(x => x.Approved == null);
            viewModel.Promoters = DataContext.Promoters.Include("ApplicationUser").Where(x => x.Approved == null);
            return View(viewModel);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult ApproveTeacher(string teacherId, bool approved)
        {
            var teacher = DataContext.Teachers.Where(x => x.ApplicationUser.Id == teacherId).Include("ApplicationUser").FirstOrDefault();
            teacher.Approved = approved;
            teacher.ApproveDate = DateTime.Today;
            var userManager = new ApplicationUserManager(new UserStore<ApplicationUser>(DataContext));

            if (ModelState.IsValid)
            {
                try
                {
                    DataContext.Entry(teacher).State = EntityState.Modified;
                    DataContext.SaveChanges();
                    if (!UserManager.IsInRole(teacher.ApplicationUser.Id, "Teacher"))
                    {
                        UserManager.AddToRole(teacher.ApplicationUser.Id, "Teacher");
                    }
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
            var owner = DataContext.Owners.Where(x => x.ApplicationUser.Id == ownerId).Include("ApplicationUser").FirstOrDefault();
            owner.Approved = approved;
            owner.ApproveDate = DateTime.Today;
            if (ModelState.IsValid)
            {
                try
                {
                    DataContext.Entry(owner).State = EntityState.Modified;
                    DataContext.SaveChanges();
                    if (!UserManager.IsInRole(owner.ApplicationUser.Id, "Owner"))
                    {
                        UserManager.AddToRole(owner.ApplicationUser.Id, "Owner");
                    }
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
            var promoter = DataContext.Promoters.Where(x => x.ApplicationUser.Id == promoterId).Include("ApplicationUser").FirstOrDefault();
            promoter.Approved = approved;
            promoter.ApproveDate = DateTime.Today;
            if (ModelState.IsValid)
            {
                try
                {
                    DataContext.Entry(promoter).State = EntityState.Modified;
                    DataContext.SaveChanges();
                    if (!UserManager.IsInRole(promoter.ApplicationUser.Id, "Promoter"))
                    {
                        UserManager.AddToRole(promoter.ApplicationUser.Id, "Promoter");
                    }
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
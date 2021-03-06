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
using EDR.Areas.Admin.Models.ViewModels;

namespace EDR.Areas.Admin.Controllers
{
    public class RolesController : BaseController
    {
        // GET: Admin/Roles
        [Authorize(Roles = "Admin")]
        public ActionResult Index(string message)
        {
            var model = new RolesViewModel();
            model.Role = new IdentityRole();
            model.Roles = DataContext.Roles.ToList();

            ViewBag.Message = message;

            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult Index(RolesViewModel model)
        {
            model.Roles = DataContext.Roles.ToList();

            if (model.Role != null)
            {
                model.Users = DataContext.Users.Where(u => u.Roles.Any(r => r.RoleId == model.Role.Id));

                if (model.UserId != null)
                {
                    model.Users = model.Users.Where(u => u.Id == model.UserId);
                }
            }
            model.UserId = null;
            ModelState.Clear();
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult AddUser(string roleId, string message)
        {
            var model = new AddUserViewModel();
            model.Roles = DataContext.Roles.ToList();

            ViewBag.Message = message;
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult AddUser(AddUserViewModel model)
        {
            model.Roles = DataContext.Roles.ToList();
            model.SearchUsers = DataContext.Users.Include("UserPictures").Where(u => u.Id == model.UserId);

            return View(model);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult PostRoleUser(string id, string role)
        {
            try
            {
                var user = DataContext.Users.Where(u => u.Id == id).FirstOrDefault();

                if (user != null)
                {
                    if (role == "Teacher")
                    {
                        if (DataContext.Teachers.Where(t => t.ApplicationUser.Id == id).Count() == 0)
                        {
                            DataContext.Teachers.Add(new Teacher() { ApplicationUser = user, Approved = true, ApproveDate = DateTime.Today, ContactEmail = user.Email, Phone = user.PhoneNumber ?? "9999999999" });
                            DataContext.SaveChanges();
                        }

                        if (!UserManager.IsInRole(id, "Teacher"))
                        {
                            UserManager.AddToRole(id, "Teacher");
                        }
                        //  Notify User
                        EmailProcess.NewRoleuser(id, role);

                        return RedirectToAction("Index", new { message = user.FullName + " was added to Teacher Role" });
                    }
                    else if (role == "Promoter")
                    {
                        if (DataContext.Promoters.Where(t => t.ApplicationUser.Id == id).Count() == 0)
                        {
                            DataContext.Promoters.Add(new Promoter() { ApplicationUser = user, Approved = true, ApproveDate = DateTime.Today, ContactEmail = user.Email, Phone = user.PhoneNumber ?? "9999999999" });
                            DataContext.SaveChanges();
                        }

                        if (!UserManager.IsInRole(id, "Promoter"))
                        {
                            UserManager.AddToRole(id, "Promoter");
                        }
                        //  Notify User
                        EmailProcess.NewRoleuser(id, role);

                        return RedirectToAction("Index", new { message = user.FullName + " was added to Promoter Role" });
                    }
                    else if (role == "Owner")
                    {
                        if (DataContext.Owners.Where(t => t.ApplicationUser.Id == id).Count() == 0)
                        {
                            DataContext.Owners.Add(new Owner() { ApplicationUser = user, Approved = true, ApproveDate = DateTime.Today, ContactEmail = user.Email, Phone = user.PhoneNumber ?? "9999999999" });
                            DataContext.SaveChanges();
                        }

                        if (!UserManager.IsInRole(id, "Owner"))
                        {
                            UserManager.AddToRole(id, "Owner");
                        }
                        //  Notify User
                        EmailProcess.NewRoleuser(id, role);

                        return RedirectToAction("Index", new { message = user.FullName + " was added to Owner Role" });
                    }
                    else if (role == "Admin")
                    {
                        if (!UserManager.IsInRole(id, "Admin"))
                        {
                            UserManager.AddToRole(id, "Admin");
                        }
                        //  Notify User
                        EmailProcess.NewRoleuser(id, role);

                        return RedirectToAction("Index", new { message = user.FullName + " was added to Admin Role" });
                    }
                    else if (role == "Blogger")
                    {
                        if (!UserManager.IsInRole(id, "Blogger"))
                        {
                            UserManager.AddToRole(id, "Blogger");
                        }
                        //  Notify User
                        EmailProcess.NewRoleuser(id, role);

                        return RedirectToAction("Index", new { message = user.FullName + " was added to Blogger Role" });
                    }
                }
            }
            catch (DbEntityValidationException e)
            {
                var msg = "";
                foreach (var eve in e.EntityValidationErrors)
                {
                    msg = eve.Entry.Entity.GetType().Name + " " + eve.Entry.State;
                    foreach (var ve in eve.ValidationErrors)
                    {
                        msg = ve.PropertyName + " " + ve.ErrorMessage;
                    }
                }

                return RedirectToAction("Index", new { message = msg });
            }

            return RedirectToAction("AddUser");
        }

        [Authorize(Roles = "Admin")]
        public ActionResult RemoveRoleUser(string id, string role)
        {
            try
            {
                var userManager = new ApplicationUserManager(new UserStore<ApplicationUser>(DataContext));
                var user = DataContext.Users.Where(u => u.Id == id).FirstOrDefault();

                if (user != null)
                {
                    if (UserManager.IsInRole(id, role))
                    {
                        userManager.RemoveFromRole(id, role);
                    }

                    if (role == "Teacher")
                    {
                        var teacher = DataContext.Teachers.Where(t => t.ApplicationUser.Id == id).FirstOrDefault();
                        teacher.Approved = false;
                        teacher.ApproveDate = null;
                        DataContext.Entry(teacher).State = EntityState.Modified;
                        DataContext.SaveChanges();
                    }
                    else if (role == "Promoter")
                    {
                        var promoter = DataContext.Promoters.Where(t => t.ApplicationUser.Id == id).FirstOrDefault();
                        promoter.Approved = false;
                        promoter.ApproveDate = null;
                        DataContext.Entry(promoter).State = EntityState.Modified;
                        DataContext.SaveChanges();
                    }
                    else if (role == "Owner")
                    {
                        var owner = DataContext.Owners.Where(t => t.ApplicationUser.Id == id).FirstOrDefault();
                        owner.Approved = false;
                        owner.ApproveDate = null;
                        DataContext.Entry(owner).State = EntityState.Modified;
                        DataContext.SaveChanges();
                    }
                    return RedirectToAction("Index");
                }
            }
            catch (DbEntityValidationException ex)
            {
                return View();
            }


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

        [Authorize(Roles = "Admin")]
        public ActionResult CheckRoles()
        {
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(DataContext));

            if (!roleManager.RoleExists("Blogger"))
            {
                // first we create Admin rool   
                var role = new Microsoft.AspNet.Identity.EntityFramework.IdentityRole();
                role.Name = "Blogger";
                roleManager.Create(role);
            }

            return View();
        }
    }
}
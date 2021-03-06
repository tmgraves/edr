﻿using EDR.Models;
using EDR.Models.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using EDR.Utilities;
using System.Security.Claims;
using DayPilot.Web.Mvc;
using DayPilot.Web.Mvc.Enums;
using DayPilot.Web.Mvc.Events.Month;
using EDR.Data;
using System;
using System.Web.Security;
using Microsoft.AspNet.Identity.EntityFramework;
using Facebook;
using Newtonsoft.Json;
using System.Text;
using EDR.Enums;
using System.Configuration;
using System.IO;

namespace EDR.Controllers
{
    [Authorize]
    public class AccountController : BaseController
    {
        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            if (returnUrl == null)
            {
                returnUrl = Request.UrlReferrer.PathAndQuery.ToString();
            }
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                //  var user = await UserManager.FindAsync(model.Email, model.Password);
                var user = await UserManager.FindByNameOrEmailAsync(model.Email, model.Password);

                if (user != null)
                {
                    if (!await UserManager.IsEmailConfirmedAsync(user.Id))
                    {
                        ViewBag.errorMessage = "You must have a confirmed email to log on.";
                        string callbackUrl = SendEmailConfirmationToken(user.Id);
                        //  string callbackUrl = await SendEmailConfirmationTokenAsync(user.Id, "Eat. Dance. Repeat. - Confirm your account");
                        ViewBag.Message = "Check your email and confirm your account, you must be confirmed "
                                                 + "before you can log in.";

                        return View("Info"); 
                    }
                    else if (User.Identity.GetNewPassword())
                    {
                        await SignInAsync(user, model.RememberMe);
                        return RedirectToAction("ChangePassword");
                    }

                    await SignInAsync(user, model.RememberMe);

                    //  Migrate the Shopping Cart
                    MigrateShoppingCart(model.Email); 
                    if (user.CurrentRole != null)
                    {
                        Session["CurrentRole"] = DataContext.Roles.Where(r => r.Id == user.CurrentRole.Id).FirstOrDefault().Name;
                    }
                    returnUrl = returnUrl != null ? returnUrl.Replace("NotLoggedIn", user.UserName) : null;
                    return RedirectToLocal(returnUrl);
                }
                else
                {
                    ModelState.AddModelError("", "Invalid username or password.");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                //  Create User
                var user = new ApplicationUser() { UserName = model.UserName, Email = model.Email, FirstName = model.FirstName, LastName = model.LastName, Location = model.Location, Latitude = model.Latitude, Longitude = model.Longitude };
                IdentityResult result = await UserManager.CreateAsync(user, model.Password);
                //  Create User

                //  Migrate the Shopping Cart
                MigrateShoppingCart(model.UserName);
                if (result.Succeeded)
                {
                    //  await SignInAsync(user, isPersistent: false);

                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    //string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    //var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    //await UserManager.SendEmailAsync(user.Id, "Eat. Dance. Repeat. - Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");
                    //  string callbackUrl = await SendEmailConfirmationTokenAsync(user.Id, "Eat. Dance. Repeat. - Confirm your account");
                    string callbackUrl = SendEmailConfirmationToken(user.Id);

                    // Uncomment to debug locally 
                    // TempData["ViewBagLink"] = callbackUrl;

                    ViewBag.Message = "Check your email and confirm your account, you must be confirmed "
                                    + "before you can log in.";

                    return View("Info");
                    //return RedirectToAction("Index", "Home");
                }
                else
                {
                    AddErrors(result);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        public string CreateUser(ApplicationUser user, string password)
        {
            IdentityResult result = UserManager.Create(user, password);
            string callbackUrl = SendEmailConfirmationToken(user.Id);
            return user.Id;
        }

        ////
        //// GET: /Account/ConfirmEmail
        //[AllowAnonymous]
        //public async Task<ActionResult> ConfirmEmailAsync(string userId, string code)
        //{
        //    if (userId == null || code == null) 
        //    {
        //        return View("Error");
        //    }

        //    IdentityResult result = await UserManager.ConfirmEmailAsync(userId, code);
        //    if (result.Succeeded)
        //    {
        //        return View("ConfirmEmail");
        //    }
        //    else
        //    {
        //        AddErrors(result);
        //        return View();
        //    }
        //}

        [AllowAnonymous]
        public ActionResult ConfirmEmail(string userId, string code)
        {
            var model = new ConfirmEmailViewModel();
            model.Code = code;
            model.UserId = userId;

            return View(model);
        }

        //[AllowAnonymous]
        //[HttpPost]
        //public ActionResult ConfirmEmail(ConfirmEmailViewModel model)
        //{
        //    if (model.UserId == null || model.Code == null)
        //    {
        //        return View("Error");
        //    }

        //    IdentityResult result = UserManager.ConfirmEmail(model.UserId, model.Code);
        //    if (result.Succeeded)
        //    {
        //        ViewBag.Message = "Success";
        //        return View("EmailConfirmed");
        //    }
        //    else
        //    {
        //        ViewBag.Message = "Failure";
        //        return View("ConfirmEmail");
        //    }
        //}

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> ConfirmEmailAsync(ConfirmEmailViewModel model)
        {
            if (model.UserId == null || model.Code == null)
            {
                return View("Error");
            }

            IdentityResult result = await UserManager.ConfirmEmailAsync(model.UserId, model.Code);
            if (result.Succeeded)
            {
                var user = UserManager.FindById(model.UserId);
                await SignInAsync(user, isPersistent: false);
                ViewBag.Message = "Success";
                return View("EmailConfirmed");
            }
            else
            {
                ViewBag.Message = "Failure";
                return View("ConfirmEmail");
            }
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    ModelState.AddModelError("", "The user either does not exist.");
                    return View();
                }
                else if (!await UserManager.IsEmailConfirmedAsync(user.Id))
                {
                    //  string callbackUrl = await SendEmailConfirmationTokenAsync(user.Id, "Eat. Dance. Repeat. - Confirm your account");
                    string callbackUrl = SendEmailConfirmationToken(user.Id);
                    ViewBag.Message = "Check your email and confirm your account, you must be confirmed "
                                             + "before you can log in.";

                    return View("Info");
                }
                else
                {
                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                    var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);

                    string body;
                    //Read template file from the App_Data folder
                    using (var sr = new StreamReader(Server.MapPath("\\App_Data\\Templates\\ForgotPassword.txt")))
                    {
                        body = sr.ReadToEnd();
                    }

                    try
                    {
                        //add email logic here to email the customer that their invoice has been voided
                        //Username: {1}
                        string name = HttpUtility.UrlEncode(user.FullName);
                        //  string formattedcallback = HttpUtility.UrlEncode(callbackUrl);
                        string emailSubject = @"Eat. Dance. Repeat. - Reset Password";

                        string messageBody = string.Format(body, user.FullName, callbackUrl, callbackUrl);
                        //  string messageBody = string.Format(body, "test", "test");

                        var MailHelper = new MailHelper
                        {
                            //  Sender = sender, //email.Sender,
                            Recipient = user.Email,
                            RecipientCC = null,
                            Subject = emailSubject,
                            Body = messageBody
                        };
                        MailHelper.Send();

                        //  await UserManager.SendEmailAsync(user.Id, emailSubject, messageBody);
                        return RedirectToAction("ForgotPasswordConfirmation", "Account");
                    }
                    catch(Exception ex)
                    {
                        ViewBag.Message = ex.Message;
                        return View();
                    }
                }

                //UserManager.SendEmail(user.Id, "Test Email", "Test Body");

                //System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage();

                //message.To.Add(new System.Net.Mail.MailAddress("tadashigraves@gmail.com"));

                //message.IsBodyHtml = true;
                //message.BodyEncoding = Encoding.UTF8;
                //message.Subject = "this is a test subject for " + user.FullName;
                //message.Body = "Here is the test email message";

                //System.Net.Mail.SmtpClient client = new System.Net.Mail.SmtpClient();
                //client.Send(message);
                
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }
	
        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            if (code == null) 
            {
                return View("Error");
            }
            return View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    ModelState.AddModelError("", "No user found.");
                    return View();
                }
                IdentityResult result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
                if (result.Succeeded)
                {
                    return RedirectToAction("ResetPasswordConfirmation", "Account");
                }
                else
                {
                    AddErrors(result);
                    return View();
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        // GET: /Account/ResetPassword
        public async Task<ActionResult> ChangePassword()
        {
            var model = new ChangePasswordViewModel();
            if (User.Identity.GetNewPassword())
            {
                var userid = User.Identity.GetUserId();
                string code = await UserManager.GeneratePasswordResetTokenAsync(userid);
                model.ResetPasswordCode = code;
            }
            return View(model);
        }

        // POST: /Account/ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userid = User.Identity.GetUserId();
                var user = await UserManager.FindByIdAsync(userid);
                if (user == null)
                {
                    ViewBag.Message = "No user found.";
                    return View();
                }

                IdentityResult result = new IdentityResult();
                //  New Password
                if (User.Identity.GetNewPassword())
                {
                    result = await UserManager.ResetPasswordAsync(userid, model.ResetPasswordCode, model.NewPassword);
                    DataContext.Users.Single(u => u.Id == userid).NewPassword = false;
                    await DataContext.SaveChangesAsync();
                }
                //  Change Password
                else
                {
                    result = await UserManager.ChangePasswordAsync(userid, model.OldPassword, model.NewPassword);
                }

                if (result.Succeeded)
                {
                    return RedirectToAction("ResetPasswordConfirmation", "Account");
                    //  return RedirectToAction("Manage", new { Message = ManageMessageId.ChangePasswordSuccess });
                }
                else
                {
                    ViewBag.Message = result.Errors.FirstOrDefault();
                    return View();
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // POST: /Account/Disassociate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Disassociate(string loginProvider, string providerKey)
        {
            ManageMessageId? message = null;
            IdentityResult result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(), new UserLoginInfo(loginProvider, providerKey));
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                await SignInAsync(user, isPersistent: false);
                message = ManageMessageId.RemoveLoginSuccess;
            }
            else
            {
                message = ManageMessageId.Error;
            }
            return RedirectToAction("Manage", new { Message = message });
        }

        //
        // GET: /Account/Manage
        public ActionResult Manage(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : message == ManageMessageId.Error ? "An error has occurred."
                : "";
            ViewBag.HasLocalPassword = HasPassword();
            ViewBag.ReturnUrl = Url.Action("Manage");
            return View();
        }

        //
        // POST: /Account/Manage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Manage(ManageUserViewModel model)
        {
            bool hasPassword = HasPassword();
            ViewBag.HasLocalPassword = hasPassword;
            ViewBag.ReturnUrl = Url.Action("Manage");
            if (hasPassword)
            {
                if (ModelState.IsValid)
                {
                    IdentityResult result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
                    if (result.Succeeded)
                    {
                        var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                        await SignInAsync(user, isPersistent: false);
                        return RedirectToAction("Manage", new { Message = ManageMessageId.ChangePasswordSuccess });
                    }
                    else
                    {
                        AddErrors(result);
                    }
                }
            }
            else
            {
                // User does not have a password so remove any validation errors caused by a missing OldPassword field
                ModelState state = ModelState["OldPassword"];
                if (state != null)
                {
                    state.Errors.Clear();
                }

                if (ModelState.IsValid)
                {
                    IdentityResult result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Manage", new { Message = ManageMessageId.SetPasswordSuccess });
                    }
                    else
                    {
                        AddErrors(result);
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var user = await UserManager.FindAsync(loginInfo.Login);
            if (user != null)
            {
                //Save the FacebookToken in the database if not already there
                await StoreFacebookAuthToken(user);
                await SignInAsync(user, isPersistent: false);
                if (user.CurrentRole != null)
                {
                    Session["CurrentRole"] = DataContext.Roles.Where(r => r.Id == user.CurrentRole.Id).FirstOrDefault().Name;
                }

                //  Migrate the Shopping Cart
                MigrateShoppingCart(user.UserName);

                returnUrl = returnUrl != null ? returnUrl.Replace("NotLoggedIn", user.UserName) : null;
                return RedirectToLocal(returnUrl);
            }
            else
            {
                // If the user does not have an account, then prompt the user to create an account
                ViewBag.ReturnUrl = returnUrl;
                ViewBag.LoginProvider = loginInfo.Login.LoginProvider;

                var model = new ExternalLoginConfirmationViewModel { UserName = loginInfo.DefaultUserName };
                var selectedStyles = new List<DanceStyleListItem>();
                model.SelectedStyles = selectedStyles;
                var styles = new List<DanceStyleListItem>();
                foreach (DanceStyle s in DataContext.DanceStyles)
                {
                    styles.Add(new DanceStyleListItem { Id = s.Id, Name = s.Name });
                }
                model.AvailableStyles = styles.OrderBy(x => x.Name); 
                return View("ExternalLoginConfirmation", model);
            }
        }

        private async Task StoreFacebookAuthToken(ApplicationUser user)
        {
            var claimsIdentity = await AuthenticationManager.GetExternalIdentityAsync(DefaultAuthenticationTypes.ExternalCookie);
            if (claimsIdentity != null)
            {
                try
                {
                    // Retrieve the existing claims for the user and add the FacebookAccessTokenClaim
                    var currentClaims = await UserManager.GetClaimsAsync(user.Id);
                    var facebookAccessToken = claimsIdentity.FindAll("urn:facebook:access_token").First();

                    var dbuser = DataContext.Users.Where(u => u.Id == user.Id).FirstOrDefault();

                    if (dbuser != null)
                    {
                        user.FacebookToken = facebookAccessToken.Value;
                        DataContext.Entry(dbuser).State = EntityState.Modified;
                        DataContext.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    var err = false;
                }
            }
        }
        
        //
        // POST: /Account/LinkLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LinkLogin(string provider)
        {
            // Request a redirect to the external login provider to link a login for the current user
            return new ChallengeResult(provider, Url.Action("LinkLoginCallback", "Account"), User.Identity.GetUserId());
        }

        //
        // GET: /Account/LinkLoginCallback
        public async Task<ActionResult> LinkLoginCallback()
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, User.Identity.GetUserId());
            if (loginInfo == null)
            {
                return RedirectToAction("Manage", new { Message = ManageMessageId.Error });
            }
            IdentityResult result = await UserManager.AddLoginAsync(User.Identity.GetUserId(), loginInfo.Login);
            if (result.Succeeded)
            {
                return RedirectToAction("Manage");
            }
            return RedirectToAction("Manage", new { Message = ManageMessageId.Error });
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var firstNameClaim = info.ExternalIdentity.Claims.First(c => c.Type == "urn:facebook:first_name");
                var firstName = firstNameClaim != null ? firstNameClaim.Value : null;
                var lastNameClaim = info.ExternalIdentity.Claims.First(c => c.Type == "urn:facebook:last_name");
                var lastName = lastNameClaim != null ? lastNameClaim.Value : null;
                var emailClaim = info.ExternalIdentity.Claims.First(c => c.Type == "urn:facebook:email");
                var email = emailClaim != null ? emailClaim.Value : null;
                var usernameClaim = info.ExternalIdentity.Claims.First(c => c.Type == "urn:facebook:id");
                var username = usernameClaim != null ? usernameClaim.Value : null;
                var tokenClaim = info.ExternalIdentity.Claims.First(c => c.Type == "urn:facebook:access_token");
                var token = tokenClaim != null ? tokenClaim.Value : null;

                var user = new ApplicationUser();
                IdentityResult result = new IdentityResult();

                ////  Get User's Location
                //var address = new Address();
                //var locationData = FacebookHelper.GetData(token, "me?fields=location");
                //if (locationData.location != null)
                //{
                //    var addressData = locationData.location;
                //    if (locationData.location.name != null)
                //    {
                //        address = Geolocation.ParseAddress(addressData.name);
                //    }
                //}
                //else
                //{
                //    address = Geolocation.ParseAddress(model.Zipcode);
                //}
                ////  Get User's Location


                if (DataContext.Users.Where(x => x.Email == email).Count() == 1)
                {
                    user = DataContext.Users.Where(x => x.Email == email).FirstOrDefault();
                    user.FacebookToken = token;
                    user.FacebookUsername = username;
                    user.StartDate = model.StartDate;
                    user.City = model.City;
                    user.State = model.State;
                    user.ZipCode = model.ZipCode;
                    user.Country = model.Country;
                    user.Latitude = model.Latitude;
                    user.Longitude = model.Longitude;
                    user.EmailConfirmed = true;
                    DataContext.SaveChanges();
                }
                else
                {
                    user = new ApplicationUser() {
                        UserName = model.UserName,
                        Email = email,
                        FirstName = firstName,
                        LastName = lastName,
                        FacebookToken = token,
                        FacebookUsername = username,
                        StartDate = model.StartDate,
                        City = model.City,
                        State = model.State,
                        ZipCode = model.ZipCode,
                        Country = model.Country,
                        Latitude = model.Latitude,
                        Longitude = model.Longitude,
                        EmailConfirmed = true
                    };
                    result = await UserManager.CreateAsync(user);
                    if (!result.Succeeded)
                    {
                        AddErrors(result);
                    }
                }

                //  Add Profile Pictures
                var picture = FacebookHelper.GetPhotos(token).Where(x => x.Album == "Profile Pictures").FirstOrDefault();
                if (picture != null)
                {
                    user.UserPictures = new List<UserPicture>();
                    user.UserPictures.Add(new UserPicture() { Filename = picture.LargeSource, ProfilePicture = true, Title = "Profile Picture", ThumbnailFilename = picture.LargeSource, PhotoDate = picture.PhotoDate });
                    DataContext.SaveChanges();
                }

                //  Add Dance Styles
                var styleids = model.StyleIds.Split('-');
                if (styleids.Length != 0)
                {
                    user.DanceStyles = DataContext.DanceStyles.Where(x => styleids.Contains(x.Id.ToString())).ToList();
                    DataContext.SaveChanges();
                }

                result = await UserManager.AddLoginAsync(user.Id, info.Login);
                if (result.Succeeded)
                {
                    await SignInAsync(user, isPersistent: false);
                        
                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // SendEmail(user.Email, callbackUrl, "Confirm your account", "Please confirm your account by clicking this link");

                    returnUrl = returnUrl != null ? returnUrl.Replace("NotLoggedIn", user.UserName) : null;
                    return RedirectToLocal(returnUrl);
                }
            }

            //  Migrate the Shopping Cart
            MigrateShoppingCart(model.UserName);
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut();
            var returnUrl = Request.UrlReferrer.PathAndQuery.ToString();
            if (returnUrl != null)
            {
                return RedirectToLocal(returnUrl);
            }
            else
            {
            return RedirectToAction("Index", "Home");
            }
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        [ChildActionOnly]
        public ActionResult RemoveAccountList()
        {
            var linkedAccounts = UserManager.GetLogins(User.Identity.GetUserId());
            ViewBag.ShowRemoveButton = HasPassword() || linkedAccounts.Count > 1;
            return (ActionResult)PartialView("_RemoveAccountPartial", linkedAccounts);
        }

        // GET: /Account/Manage
        public ActionResult TeacherProfile()
        {
            var user = User.Identity.GetUserId();
            var viewModel = DataContext.Teachers.Where(x => x.ApplicationUser.Id == user).FirstOrDefault();
            if (viewModel == null)
            {
                var teacher = new Teacher { ApplicationUser = DataContext.Users.Find(User.Identity.GetUserId()) };
                DataContext.Teachers.Add(teacher);
                DataContext.SaveChanges();
                viewModel = DataContext.Teachers.Where(x => x.ApplicationUser.Id == user).FirstOrDefault();
            }
            return View(viewModel);
        }

        //
        // GET: /Account/ForgotPassword
        [Authorize]
        public ActionResult SwitchRole(string id)
        {
            var userid = User.Identity.GetUserId();
            var role = DataContext.Roles.Where(r => r.Name == id).FirstOrDefault();
            var user = DataContext.Users.Where(u => u.Id == userid).Include("CurrentRole").FirstOrDefault();
            Session["CurrentRole"] = role != null ? role.Name : null;
           
            user.CurrentRole = role != null ? role : null;
            DataContext.Entry(user).State = EntityState.Modified;
            DataContext.SaveChanges();
            return RedirectToAction("Home", role == null ? "Dancer" : role.Name, new { username = User.Identity.Name });
        }

        //
        // POST: /Account/Manage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TeacherProfile([Bind(Include = "ApplicationUser,Experience,Resume, FacebookLink, Website")] Teacher teacher)
        {
            if (ModelState.IsValid)
            {
                DataContext.Entry(teacher).State = EntityState.Modified;
                DataContext.SaveChanges();
                return RedirectToAction("TeacherProfile");
            }
            return View(teacher);
        }

        public async Task<string> SendEmailConfirmationTokenAsync(string userID)
        {
            string code = await UserManager.GenerateEmailConfirmationTokenAsync(userID);
            var result = EmailProcess.NewAccount(userID, code);
            return result;
            //var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = userID, code = code }, protocol: Request.Url.Scheme);
            //await UserManager.SendEmailAsync(userID, subject, "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

            //return callbackUrl;
        }

        public string SendEmailConfirmationToken(string userID)
        {
            string code = UserManager.GenerateEmailConfirmationToken(userID);
            //  var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = userID, code = code }, protocol: Request.Url.Scheme);
            //  UserManager.SendEmail(userID, subject, "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");
            var result = EmailProcess.NewAccount(userID, code);

            return result;
        }

        private void MigrateShoppingCart(string UserName)
        {
            // Associate shopping cart items with logged-in user
            var cart = ShoppingCart.GetCart(this.HttpContext);

            cart.MigrateCart(UserName);
            Session[ShoppingCart.CartSessionKey] = UserName;
        }
        
        //public ActionResult Backend()
        //{
        //    return new Dpm().CallBack(this);
        //}

        //class Dpm : DayPilotMonth
        //{

        //    protected override void OnInit(InitArgs e)
        //    {
        //        var db = new ApplicationDbContext();
        //        Events = db.Events.Where(x => x.StartDate > DateTime.Today && x.EndDate > DateTime.Today).ToList();

        //        DataIdField = "Id";
        //        DataTextField = "Name";
        //        DataStartField = "StartDate";
        //        DataEndField = "EndDate";

        //        Update();
        //    }
        //}

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private async Task SignInAsync(ApplicationUser user, bool isPersistent)
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            AuthenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = isPersistent }, await user.GenerateUserIdentityAsync(UserManager));
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        private void SendEmail(string email, string callbackUrl, string subject, string message)
        {
            // For information on sending mail, please visit http://go.microsoft.com/fwlink/?LinkID=320771
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            Error
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        private class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri) : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties() { RedirectUri = RedirectUri, AllowRefresh = true };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }

}
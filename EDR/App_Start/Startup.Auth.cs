﻿using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.Security.Google;
using Owin;
using System;
using EDR.Models;
using EDR.Data;
using Microsoft.Owin.Security.Facebook;
using System.Configuration;
using Hangfire;
using EDR.Utilities;
using EDR.Models;

namespace EDR
{
    public partial class Startup
    {
        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            // Configure the db context and user manager to use a single instance per request
            app.CreatePerOwinContext(ApplicationDbContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);

            // Enable the application to use a cookie to store information for the signed in user
            // and to use a cookie to temporarily store information about a user logging in with a third party login provider
            // Configure the sign in cookie
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login"),
                Provider = new CookieAuthenticationProvider
                {
                    OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, ApplicationUser>(
                        validateInterval: TimeSpan.FromMinutes(30),
                        regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(manager))
                }
            });
            
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            // Uncomment the following lines to enable logging in with third party login providers
            //app.UseMicrosoftAccountAuthentication(
            //    clientId: "",
            //    clientSecret: "");

            //app.UseTwitterAuthentication(
            //   consumerKey: "",
            //   consumerSecret: "");

            var x = new FacebookAuthenticationOptions();
            x.Scope.Add("email");
            x.Scope.Add("user_photos");
            x.Scope.Add("user_friends");
            x.Scope.Add("user_events");
            x.Scope.Add("user_videos");
            //  x.Scope.Add("user_groups");     -- Removed on 5/9/16 because it was causing Facebook auth to fail.  Check Facebook Developer Console to make sure Approved Items matches this list
            //  x.Scope.Add("user_location");
            //x.Scope.Add("first_name");
            //x.Scope.Add("last_name");
            //  x.Scope.Add("link");
            x.AppId = ConfigurationManager.AppSettings["FacebookAppId"];
            x.AppSecret = ConfigurationManager.AppSettings["FacebookAppSecret"];
            x.Provider = new FacebookAuthenticationProvider()
            {
                OnAuthenticated = async context =>
                {
                    //  context.Identity.AddClaim(new System.Security.Claims.Claim("FacebookAccessToken", context.AccessToken));
                    context.Identity.AddClaim(new System.Security.Claims.Claim("urn:facebook:access_token", context.AccessToken));
                    context.Identity.AddClaim(new System.Security.Claims.Claim("urn:facebook:email", context.Email));
                    foreach (var claim in context.User)
                    {
                        var claimType = string.Format("urn:facebook:{0}", claim.Key);
                        string claimValue = claim.Value.ToString();
                        if (!context.Identity.HasClaim(claimType, claimValue))
                            context.Identity.AddClaim(new System.Security.Claims.Claim(claimType, claimValue, "XmlSchemaString", "Facebook"));

                    }

                }
            };

            x.SignInAsAuthenticationType = DefaultAuthenticationTypes.ExternalCookie;
            app.UseFacebookAuthentication(x);
            //app.UseFacebookAuthentication(
            //   appId: "1634016200156058",
            //   appSecret: "17e338c89fc606e10ab6ec541671f58c");

            //app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions()
            //{
            //    ClientId = "",
            //    ClientSecret = ""
            //});

            GlobalConfiguration.Configuration.UseSqlServerStorage("DefaultConnection");

            app.UseHangfireDashboard();
            app.UseHangfireServer();

            try
            {
                ////  Recurring Job to refresh Facebook events
                //RecurringJob.AddOrUpdate(() => FacebookHelper.RefreshEvents(), Cron.Daily);

                //  Recurring Job to send emails in queue
                RecurringJob.AddOrUpdate(() => EmailProcess.ProcessEmails(), Cron.Hourly);

                //  Recurring Job - Expiring Events
                RecurringJob.AddOrUpdate(() => EmailProcess.ExpiringEvents(), Cron.Daily);

                //  Recurring Job - Confirm Events
                RecurringJob.AddOrUpdate(() => EmailProcess.SendConfirmEmails(), Cron.Daily);

                //  Recurring Job - Daily Dancer Summary
                RecurringJob.AddOrUpdate(() => EmailProcess.DailyDancerSummaries(), Cron.Daily);

                //  Recurring Job - Daily Dancer Summary
                RecurringJob.AddOrUpdate(() => EmailProcess.EventSummaries(), Cron.Daily);

                //  Recurring Job - Batch Summaries
                RecurringJob.AddOrUpdate(() => SettlementBatch.GetBatches(), Cron.Daily);
            }
            catch(Exception ex)
            {
                var message = ex.Message;
            }
        }
    }
}
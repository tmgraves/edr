using EDR.Data;
using EDR.Models;
using EDR.Types;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Caching;
using System.Net;
using EDR.Utilities;

namespace EDR
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private static CacheItemRemovedCallback OnCacheRemove = null;

        protected void Application_Start()
        {
            Database.SetInitializer<ApplicationDbContext>(new ApplicationDbInitializer());
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            ModelBinders.Binders.Add(typeof(DateTime), new DateTimeBinder());
            ModelBinders.Binders.Add(typeof(DateTime?), new NullableDateTimeBinder());
        }

        protected void Session_Start(object sender, EventArgs e)
        {
            var context = new ApplicationDbContext();

            if (User.Identity.IsAuthenticated)
            {
                var user = context.Users.Where(u => u.UserName == User.Identity.Name).Include("CurrentRole").FirstOrDefault();
                if (user != null && user.CurrentRole != null)
                {
                    HttpContext.Current.Session["CurrentRole"] = user.CurrentRole.Name;
                }
            }
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            AddRecurringTask("RefreshFacebookEvents", 60);
        }

        private void AddRecurringTask(string name, int seconds)
        {
            OnCacheRemove = new CacheItemRemovedCallback(CacheItemRemoved);
            HttpRuntime.Cache.Insert(name, seconds, null,
                DateTime.Now.AddSeconds(seconds), Cache.NoSlidingExpiration,
                CacheItemPriority.NotRemovable, OnCacheRemove);
        }

        public void CacheItemRemoved(string task, object sec, CacheItemRemovedReason reason)
        {
            // do stuff here if it matches our taskname, like WebRequest
            // re-add our task so it recurs
            RefreshFacebookEvents();
            AddRecurringTask(task, Convert.ToInt32(sec));
        }

        private static void RefreshFacebookEvents()
        {
            var context = new ApplicationDbContext();
            var events = context.Events.Where(e => e.FacebookId != null);
            var fbids = events.Select(ev => ev.FacebookId).ToArray();
            var par = String.Join(",", fbids);
            var evts = FacebookHelper.GetData(FacebookHelper.GetGlobalToken(), "?ids=" + par);
        }
    }
}

﻿using EDR.Data;
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
using EDR.Attributes;

namespace EDR
{
    public class MvcApplication : System.Web.HttpApplication
    {
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

        public override void Init()
        {
            base.Init();
            this.AcquireRequestState += showRouteValues;
        }

        protected void showRouteValues(object sender, EventArgs e)
        {
            var context = HttpContext.Current;
            if (context == null)
                return;
            var routeData = RouteTable.Routes.GetRouteData(new HttpContextWrapper(context));
        }
    }
}

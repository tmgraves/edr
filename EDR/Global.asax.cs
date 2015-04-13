using EDR.Data;
using EDR.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

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
    }
}

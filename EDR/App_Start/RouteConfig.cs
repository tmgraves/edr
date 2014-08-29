using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace EDR
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Dancer",
                url: "Dancer/{username}",
                defaults: new { controller = "Profiles", action = "Dancer" }
            );

            routes.MapRoute(
                name: "Teacher",
                url: "Teacher/{username}",
                defaults: new { controller = "Profiles", action = "Teacher"}
            );
            routes.MapRoute(
                name: "Promoter",
                url: "Promoter/{username}",
                defaults: new { controller = "Profiles", action = "Promoter" }
            );

            routes.MapRoute(
                name: "Owner",
                url: "Owner/{username}",
                defaults: new { controller = "Profiles", action = "Owner" }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}

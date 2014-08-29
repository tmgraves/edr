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
                name: "DancerEdit",
                url: "Dancer/Edit",
                defaults: new { controller = "Dancer", action = "Edit" }
            );
            routes.MapRoute(
                name: "Dancer",
                url: "Dancer/{username}",
                defaults: new { controller = "Dancer", action = "View" }
            );

            routes.MapRoute(
                name: "TeacherEdit",
                url: "Teacher/Edit",
                defaults: new { controller = "Teacher", action = "Edit" }
            );
            routes.MapRoute(
                name: "TeacherApply",
                url: "Teacher/Apply",
                defaults: new { controller = "Teacher", action = "Apply" }
            );
            routes.MapRoute(
                name: "Teacher",
                url: "Teacher/{username}",
                defaults: new { controller = "Teacher", action = "View" }
            );

            routes.MapRoute(
                name: "PromoterEdit",
                url: "Promoter/Edit",
                defaults: new { controller = "Promoter", action = "Edit" }
            );
            routes.MapRoute(
                name: "PromoterApply",
                url: "Promoter/Apply",
                defaults: new { controller = "Promoter", action = "Apply" }
            );
            routes.MapRoute(
                name: "Promoter",
                url: "Promoter/{username}",
                defaults: new { controller = "Promoter", action = "View" }
            );

            routes.MapRoute(
                name: "OwnerEdit",
                url: "Owner/Edit",
                defaults: new { controller = "Owner", action = "Edit" }
            );
            routes.MapRoute(
                name: "OwnerApply",
                url: "Owner/Apply",
                defaults: new { controller = "Owner", action = "Apply" }
            );
            routes.MapRoute(
                name: "Owner",
                url: "Owner/{username}",
                defaults: new { controller = "Owner", action = "View" }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}

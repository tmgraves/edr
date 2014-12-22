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
                name: "VisitorView",
                url: "Visitor/View",
                defaults: new { controller = "Visitor", action = "View" }
            );

            routes.MapRoute(
                name: "EventCreate",
                url: "{role}/{eventType}/Create",
                defaults: new { controller = "Event", action = "Create", role = UrlParameter.Optional, eventType = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DancerEdit",
                url: "Dancer/Edit",
                defaults: new { controller = "Dancer", action = "Edit" }
            );
            routes.MapRoute(
                name: "DancerList",
                url: "Dancer/List",
                defaults: new { controller = "Dancer", action = "List" }
            );
            routes.MapRoute(
                name: "DancerPicture",
                url: "Dancer/ChangePicture",
                defaults: new { controller = "Dancer", action = "ChangePicture" }
            );
            routes.MapRoute(
                name: "DancerProfilePic",
                url: "Dancer/ProfilePicture",
                defaults: new { controller = "Dancer", action = "ProfilePicture" }
            );
            routes.MapRoute(
                name: "DancerUpload",
                url: "Dancer/UploadPicture",
                defaults: new { controller = "Dancer", action = "UploadPicture" }
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
                name: "TeacherList",
                url: "Teacher/List",
                defaults: new { controller = "Teacher", action = "List" }
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
                name: "PromoterList",
                url: "Promoter/List",
                defaults: new { controller = "Promoter", action = "List" }
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
                name: "OwnerList",
                url: "Owner/List",
                defaults: new { controller = "Owner", action = "List" }
            );
            routes.MapRoute(
                name: "Owner",
                url: "Owner/{username}",
                defaults: new { controller = "Owner", action = "View" }
            );
            routes.MapRoute(
                name: "ClassDetail",
                url: "Class/Details/{id}",
                defaults: new { controller = "Event", action = "Details", eventType = "Class", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "WorkshopDetail",
                url: "Workshop/Details/{id}",
                defaults: new { controller = "Event", action = "Details", eventType = "Workshop", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "SocialDetail",
                url: "Social/Details/{id}",
                defaults: new { controller = "Event", action = "Details", eventType = "Social", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "ConcertDetail",
                url: "Concert/Details/{id}",
                defaults: new { controller = "Event", action = "Details", eventType = "Concert", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "ConferenceDetail",
                url: "Conference/Details/{id}",
                defaults: new { controller = "Event", action = "Details", eventType = "Conference", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "OpenHouseDetail",
                url: "OpenHouse/Details/{id}",
                defaults: new { controller = "Event", action = "Details", eventType = "OpenHouse", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "PartyDetail",
                url: "Party/Details/{id}",
                defaults: new { controller = "Event", action = "Details", eventType = "Party", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "RehearsalDetail",
                url: "Rehearsal/Details/{id}",
                defaults: new { controller = "Event", action = "Details", eventType = "Rehearsal", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "ConferenceCenterCreate",
                url: "ConferenceCenter/Create",
                defaults: new { controller = "Place", action = "Create", placeType = "ConferenceCenter" }
            );
            routes.MapRoute(
                name: "HotelCreate",
                url: "Hotel/Create",
                defaults: new { controller = "Place", action = "Create", placeType = "Hotel" }
            );
            routes.MapRoute(
                name: "NightclubCreate",
                url: "Nightclub/Create",
                defaults: new { controller = "Place", action = "Create", placeType = "Nightclub" }
            );
            routes.MapRoute(
                name: "OtherPlaceCreate",
                url: "Other/Create",
                defaults: new { controller = "Place", action = "Create", placeType = "OtherPlace" }
            );
            routes.MapRoute(
                name: "StudioCreate",
                url: "Studio/Create",
                defaults: new { controller = "Place", action = "Create", placeType = "Studio" }
            );
            routes.MapRoute(
                name: "RestaurantCreate",
                url: "Restaurant/Create",
                defaults: new { controller = "Place", action = "Create", placeType = "Restaurant" }
            );
            routes.MapRoute(
                name: "TheaterCreate",
                url: "Theater/Create",
                defaults: new { controller = "Place", action = "Create", placeType = "Theater" }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}

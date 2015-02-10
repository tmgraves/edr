using EDR.Enums;
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
                name: "DancerHome",
                url: "{username}",
                defaults: new { controller = "Dancer", action = "Home" }
            );

            routes.MapRoute(
                name: "SocialMediaGetAuth",
                url: "SocialMedia/AddInstagram",
                defaults: new { controller = "SocialMedia", action = "AddInstagram" }
            );

            routes.MapRoute(
                name: "SocialMediaAuthenticate",
                url: "SocialMedia/Authenticate",
                defaults: new { controller = "SocialMedia", action = "Authenticate" }
            );

            routes.MapRoute(
                name: "StyleIndex",
                url: "DanceStyle/Index",
                defaults: new { controller = "DanceStyle", action = "Index" }
            );
            routes.MapRoute(
                name: "StyleIndexAdd",
                url: "DanceStyle/Index_AddItem",
                defaults: new { controller = "DanceStyle", action = "Index_AddItem" }
            );

            routes.MapRoute(
                name: "VisitorView",
                url: "Visitor/View",
                defaults: new { controller = "Visitor", action = "View" }
            );

            routes.MapRoute(
                name: "PlaceAction",
                url: "Place/{id}/{action}",
                defaults: new { controller = "Place", id = UrlParameter.Optional, action = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "EventAddReview",
                url: "Event/Reviews_Insert",
                defaults: new { controller = "Event", action = "Reviews_Insert" }
            );

            //routes.MapRoute(
            //    name: "EventReview",
            //    url: "Event/{id}/PostReview",
            //    defaults: new { controller = "Event", action = "PostReview" }
            //);

            //routes.MapRoute(
            //    name: "EventReviews2",
            //    url: "Event/{id}/Reviews2",
            //    defaults: new { controller = "Event", action = "Reviews2" }
            //);

            //routes.MapRoute(
            //    name: "EventGetUpdates",
            //    url: "Event/{id}/GetUpdates",
            //    defaults: new { controller = "Event", action = "GetUpdates" }
            //);

            //routes.MapRoute(
            //    name: "EventGetVideos",
            //    url: "Event/{id}/GetVideos",
            //    defaults: new { controller = "Event", action = "GetVideos" }
            //);

            //routes.MapRoute(
            //    name: "EventGetPictures",
            //    url: "Event/{id}/GetPictures",
            //    defaults: new { controller = "Event", action = "GetPictures" }
            //);

            //routes.MapRoute(
            //    name: "EventGetFacebookPictures",
            //    url: "Event/{id}/GetFacebookPictures",
            //    defaults: new { controller = "Event", action = "GetFacebookPictures" }
            //);

            //routes.MapRoute(
            //    name: "EventGetFacebookVideos",
            //    url: "Event/{id}/GetFacebookVideos",
            //    defaults: new { controller = "Event", action = "GetFacebookVideos" }
            //);

            //routes.MapRoute(
            //    name: "EventGetYouTubeVideos",
            //    url: "Event/{id}/GetYouTubeVideos",
            //    defaults: new { controller = "Event", action = "GetYouTubeVideos" }
            //);

            //routes.MapRoute(
            //    name: "EventGetYouTubePlaylists",
            //    url: "Event/{id}/GetYouTubePlaylists",
            //    defaults: new { controller = "Event", action = "GetYouTubePlaylists" }
            //);

            routes.MapRoute(
                name: "EventImportYouTubeVideo",
                url: "Event/ImportYouTubeVideo",
                defaults: new { controller = "Event", action = "ImportYouTubeVideo" }
            );

            routes.MapRoute(
                name: "EventImportFacebookVideo",
                url: "Event/ImportFacebookVideo",
                defaults: new { controller = "Event", action = "ImportFacebookVideo" }
            );

            routes.MapRoute(
                name: "EventDeleteVideo",
                url: "Event/DeleteVideo",
                defaults: new { controller = "Event", action = "DeleteVideo" }
            );

            routes.MapRoute(
                name: "EventDeletePlaylist",
                url: "Event/DeletePlaylist",
                defaults: new { controller = "Event", action = "DeletePlaylist" }
            );

            routes.MapRoute(
                name: "EventUploadPic",
                url: "Event/UploadPicture",
                defaults: new { controller = "Event", action = "UploadPicture" }
            );

            routes.MapRoute(
                name: "EventDeletePic",
                url: "Event/DeletePicture",
                defaults: new { controller = "Event", action = "DeletePicture" }
            );

            routes.MapRoute(
                name: "EventTeacherJoin",
                url: "Event/JoinTeachers",
                defaults: new { controller = "Event", action = "JoinTeachers" }
            );

            routes.MapRoute(
                name: "EventApproveTeacher",
                url: "Event/ApproveTeacher",
                defaults: new { controller = "Event", action = "ApproveTeacher" }
            );

            routes.MapRoute(
                name: "EventImportYouTubeList",
                url: "Event/ImportYouTubeList",
                defaults: new { controller = "Event", action = "ImportYouTubeList" }
            );

            routes.MapRoute(
                name: "EventImportYouTubePlaylistLink",
                url: "Event/ImportYouTubePlaylistLink",
                defaults: new { controller = "Event", action = "ImportYouTubePlaylistLink" }
            );

            routes.MapRoute(
                name: "EventImportPlayListVideoLink",
                url: "Event/ImportPlayListVideoLink",
                defaults: new { controller = "Event", action = "ImportPlayListVideoLink" }
            );

            routes.MapRoute(
                name: "EventEdit",
                url: "Event/{id}/Edit",
                defaults: new { controller = "Event", action = "Edit" }
            );

            routes.MapRoute(
                name: "ClassView",
                url: "Class/{id}",
                defaults: new { controller = "Event", action = "View", eventType = EventType.Class }
            );

            routes.MapRoute(
                name: "SocialView",
                url: "Social/{id}",
                defaults: new { controller = "Event", action = "View", eventType = EventType.Social }
            );

            routes.MapRoute(
                name: "ClassReviews",
                url: "Class/{id}/Reviews",
                defaults: new { controller = "Event", action = "Reviews", eventType = EventType.Class }
            );

            routes.MapRoute(
                name: "SocialReviews",
                url: "Social/{id}/Reviews",
                defaults: new { controller = "Event", action = "Reviews", eventType = EventType.Social }
            );

            routes.MapRoute(
                name: "ClassPictures",
                url: "Class/{id}/Pictures",
                defaults: new { controller = "Event", action = "Pictures", eventType = EventType.Class }
            );

            routes.MapRoute(
                name: "SocialPictures",
                url: "Social/{id}/Pictures",
                defaults: new { controller = "Event", action = "Pictures", eventType = EventType.Social }
            );

            routes.MapRoute(
                name: "ClassVideos",
                url: "Class/{id}/Videos",
                defaults: new { controller = "Event", action = "Videos", eventType = EventType.Class }
            );

            routes.MapRoute(
                name: "SocialVideos",
                url: "Social/{id}/Videos",
                defaults: new { controller = "Event", action = "Videos", eventType = EventType.Social }
            );

            routes.MapRoute(
                name: "ClassPostPicture",
                url: "Class/{id}/PostPicture",
                defaults: new { controller = "Event", action = "PostPicture", eventType = EventType.Class }
            );

            routes.MapRoute(
                name: "SocialPostVideo",
                url: "Social/{id}/PostVideo",
                defaults: new { controller = "Event", action = "PostVideo", eventType = EventType.Social }
            );

            routes.MapRoute(
                name: "ClassPostVideo",
                url: "Class/{id}/PostVideo",
                defaults: new { controller = "Event", action = "PostVideo", eventType = EventType.Class }
            );

            routes.MapRoute(
                name: "SocialPostPicture",
                url: "Social/{id}/PostPicture",
                defaults: new { controller = "Event", action = "PostPicture", eventType = EventType.Social }
            );

            routes.MapRoute(
                name: "EventAction",
                url: "Event/{id}/{action}",
                defaults: new { controller = "Event", action = "View" }
            );

            routes.MapRoute(
                name: "EventCreate",
                url: "{role}/{eventType}/Create",
                defaults: new { controller = "Event", action = "Create", role = UrlParameter.Optional, eventType = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Home",
                url: "Home/{action}",
                defaults: new { controller = "Home", action = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "Account",
                url: "Account/{action}",
                defaults: new { controller = "Account", action = UrlParameter.Optional }
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
                name: "TeacherDetail",
                url: "Teacher/{username}/{action}",
                defaults: new { controller = "Teacher", action = "Home", username = UrlParameter.Optional }
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
                url: "Promoter/{username}/{action}",
                defaults: new { controller = "Promoter", action = "Home" }
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
                url: "Owner/{username}/{action}",
                defaults: new { controller = "Owner", action = "Home" }
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
                name: "DancerDeletePic",
                url: "Dancer/DeletePicture",
                defaults: new { controller = "Dancer", action = "DeletePicture" }
            );
            routes.MapRoute(
                name: "DancerDetail",
                url: "{username}/{action}",
                defaults: new { controller = "Dancer", action = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "DancerMapEvents",
                url: "Dancer/MapEvents",
                defaults: new { controller = "Dancer", action = "MapEvents" }
            );
            routes.MapRoute(
                name: "DancerBackend",
                url: "Dancer/Backend",
                defaults: new { controller = "Dancer", action = "Backend" }
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

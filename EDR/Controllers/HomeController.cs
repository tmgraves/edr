﻿using EDR.Models;
using EDR.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using Owin;
using EDR.Utilities;
using Microsoft.AspNet.Identity;

using YelpSharp;
using System.Data.Entity.Spatial;

namespace EDR.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Index2()
        {
            return View();
        }

        public ActionResult MapTest()
        {
            return View();
        }

        public ActionResult Explore()
        {
            var viewModel = new HomeExploreViewModel();
            viewModel.DanceStyles = DataContext.DanceStyles.Include("Dancers").ToList();
            return View(viewModel);
        }

        public ActionResult Social(int? danceStyle, int? place)
        {
            var DanceStyleLst = DataContext.DanceStyles.ToList();
            ViewBag.danceStyle = new SelectList(DanceStyleLst, "Id", "Name", danceStyle);

            var PlaceLst = DataContext.Places.ToList();
            ViewBag.place = new SelectList(PlaceLst, "Id", "Name", place);

            var viewModel = new SocialViewModel();
            viewModel.Socials = DataContext.Events.OfType<Social>().Include("DanceStyles").Include("EventMembers").Include("EventMembers.Member").Include("Reviews").Where(x => x.IsAvailable == true).Where(y => y.EndDate == null || y.EndDate >= DateTime.Now).OrderBy(z => z.StartDate).ToList();
            //viewModel.Concerts = DataContext.Events.OfType<Concert>().Include("DanceStyles").Include("EventMembers").Include("EventMembers.Member").Where(x => x.IsAvailable == true).Where(y => y.EndDate == null || y.EndDate >= DateTime.Now).OrderBy(z => z.StartDate).ToList();
            //viewModel.Conferences = DataContext.Events.OfType<Conference>().Include("DanceStyles").Include("EventMembers").Include("EventMembers.Member").Where(x => x.IsAvailable == true).Where(y => y.EndDate == null || y.EndDate >= DateTime.Now).OrderBy(z => z.StartDate).ToList();
            //viewModel.Parties = DataContext.Events.OfType<Party>().Include("DanceStyles").Include("EventMembers").Include("EventMembers.Member").Where(x => x.IsAvailable == true).Where(y => y.EndDate == null || y.EndDate >= DateTime.Now).OrderBy(z => z.StartDate).ToList();
            //viewModel.OpenHouses = DataContext.Events.OfType<OpenHouse>().Include("DanceStyles").Include("EventMembers").Include("EventMembers.Member").Where(x => x.IsAvailable == true).Where(y => y.EndDate == null || y.EndDate >= DateTime.Now).OrderBy(z => z.StartDate).ToList();

            if (danceStyle != null)
            {
                viewModel.Socials = viewModel.Socials.Where(x => x.DanceStyles.Any(s => s.Id == danceStyle));
                //viewModel.Concerts = viewModel.Concerts.Where(x => x.DanceStyles.Any(s => s.Id == danceStyle));
                //viewModel.Conferences = viewModel.Conferences.Where(x => x.DanceStyles.Any(s => s.Id == danceStyle));
                //viewModel.Parties = viewModel.Parties.Where(x => x.DanceStyles.Any(s => s.Id == danceStyle));
                //viewModel.OpenHouses = viewModel.OpenHouses.Where(x => x.DanceStyles.Any(s => s.Id == danceStyle));
            }

            if (place != null)
            {
                viewModel.Socials = viewModel.Socials.Where(x => x.Place.Id == place);
                //viewModel.Concerts = viewModel.Concerts.Where(x => x.Place.Id == place);
                //viewModel.Conferences = viewModel.Conferences.Where(x => x.Place.Id == place);
                //viewModel.Parties = viewModel.Parties.Where(x => x.Place.Id == place);
                //viewModel.OpenHouses = viewModel.OpenHouses.Where(x => x.Place.Id == place);
            }
            return View(viewModel);
        }

        public ActionResult Learn(int? danceStyle, string teacher, int? place, int? skillLevel, string location, string[] days, double? CenterLat, double? CenterLng, int? Zoom, double? NELat, double? NELng, double? SWLat, double? SWLng)
        {
            var viewModel = new LearnViewModel();
            viewModel.DanceStyles = DataContext.DanceStyles;
            viewModel.Places = DataContext.Places;
            viewModel.Teachers = DataContext.Teachers.Include("ApplicationUser");
            viewModel.Zoom = Zoom == null ? 10 : (int)Zoom;
            var dayslist = new List<DayOfWeek>();
            if (days != null)
            {
                foreach (var s in days)
                {
                    dayslist.Add((DayOfWeek)Enum.Parse(typeof(DayOfWeek), s));
                }
            }
            viewModel.Days = dayslist;
            viewModel.DaysOfWeek = new List<DayOfWeek>() { DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday };
            //  viewModel.DanceStyles = new SelectList(DanceStyleLst, "Id", "Name", danceStyle);

            //var PlaceLst = DataContext.Places.ToList();
            //ViewBag.place = new SelectList(PlaceLst, "Id", "Name", place);

            //var SkillLevelLst = new List<int> { 1, 2, 3, 4, 5 };
            //ViewBag.skillLevel = new SelectList(SkillLevelLst);

            //var TeacherLst = DataContext.Teachers.ToList();
            //ViewBag.teacher = new SelectList(TeacherLst, "ApplicationUser.Id", "ApplicationUser.FullName", teacher);

            viewModel.Location = location;

            var teachers = DataContext.Teachers.Include("ApplicationUser");
            viewModel.Classes = DataContext.Events.OfType<Class>().Include("Teachers").Include("Teachers.ApplicationUser").Include("DanceStyles").Include("EventMembers.Member").Include("Place").Include("Reviews").Where(x => x.IsAvailable == true).Where(y => y.EndDate == null || y.EndDate >= DateTime.Now).OrderBy(z => z.StartDate);
            //  viewModel.Workshops = DataContext.Events.OfType<Workshop>().Include("Teachers").Include("Teachers.ApplicationUser").Include("DanceStyles").Include("EventMembers.Member").Include("Place").Where(x => x.IsAvailable == true).Where(y => y.EndDate == null || y.EndDate >= DateTime.Now).OrderBy(z => z.StartDate).ToList();

            if (danceStyle != null)
            {
                viewModel.Classes = viewModel.Classes.Where(x => x.DanceStyles.Any(s => s.Id == danceStyle));
            }

            if (teacher != null && teacher != "")
            {
                viewModel.Classes = viewModel.Classes.Where(x => x.Teachers.Any(t => t.ApplicationUser.Id == teacher));
            }

            if (place != null)
            {
                viewModel.Classes = viewModel.Classes.Where(x => x.Place.Id == place);
            }

            if (skillLevel != null)
            {
                viewModel.Classes = viewModel.Classes.Where(x => x.SkillLevel == skillLevel);
            }

            if (days != null)
            {
                viewModel.Classes = viewModel.Classes.Where(x => viewModel.Days.Contains(x.Day));
            }

            var address = new Address();
            if (CenterLat != null && CenterLng != null)
            {
                address = Geolocation.ParseAddress(location);
                //  var myLocation = DbGeography.FromText("POINT(" + address.Longitude.ToString() + " " + address.Latitude.ToString() + ")");
                //  viewModel.Classes = viewModel.Classes.Where(c => DbGeography.FromText("POINT(" + c.Place.Longitude.ToString() + " " + c.Place.Latitude.ToString() + ")").Distance(myLocation) * .00062 < 50);
                viewModel.Classes = viewModel.Classes.Where(c => c.Place.Longitude >= SWLng && c.Place.Longitude <= NELng && c.Place.Latitude >= SWLat && c.Place.Latitude <= NELat);

                //  Set Map Location
                viewModel.SearchAddress = new Address() { Latitude = (double)CenterLat, Longitude = (double)CenterLng };
                //  Set Map Location
            }
            else if (User.Identity.IsAuthenticated)
            {
                var userid = User.Identity.GetUserId();
                var user = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();

                if (user.ZipCode != null)
                {
                    viewModel.SearchAddress = Geolocation.ParseAddress(user.ZipCode);
                }
            }
            else if (viewModel.SearchAddress == null)
            {
                viewModel.SearchAddress = Geolocation.ParseAddress("90065");
            }

            viewModel.Classes = viewModel.Classes.Take(25);

            return View(viewModel);
        }

        public ActionResult Teachers()
        {
            return View(DataContext.Teachers.Include("DanceStyles").ToList());
        }
    }
}
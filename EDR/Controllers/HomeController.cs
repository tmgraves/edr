using EDR.Models;
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
using System.Web.Security;
using System.Collections.ObjectModel;
using Microsoft.AspNet.Identity.EntityFramework;
using EDR.Enums;

namespace EDR.Controllers
{
    //public class Product
    //{
    //    public string Id { get; set; }
    //    public string Name { get; set; }
    //    public string Description { get; set; }
    //    public long Quantity { get; set; }
    //}

    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Test()
        {
            //if (User.Identity.IsAuthenticated)
            //{
            //    var eventList = new List<FacebookEvent>();
            //    var userid = User.Identity.GetUserId();
            //    var user = DataContext.Users.Where(u => u.Id == userid).Include("Events").FirstOrDefault();
            //    var evts = DataContext.Events.Where(e => e.FacebookId != null && e.Creator.Id == user.Id).OrderBy(e => e.CheckedDate).ToList();
            //    var uevts = String.Join(",", evts.Where(e => e.Creator.Id == user.Id).Select(s => s.FacebookId));
            //    eventList = FacebookHelper.GetEvents(uevts, user.FacebookToken, eventList);
            //}

            //FacebookHelper.RefreshEvents();

            //ObservableCollection<Product> inventoryList =
            //        new ObservableCollection<Product>();
            //inventoryList.Add(new Product
            //{
            //    Id = "P101",
            //    Name = "Computer",
            //    Description = "All type of computers",
            //    Quantity = 800
            //});
            //inventoryList.Add(new Product
            //{
            //    Id = "P102",
            //    Name = "Laptop",
            //    Description = "All models of Laptops",
            //    Quantity = 500
            //});
            //inventoryList.Add(new Product
            //{
            //    Id = "P103",
            //    Name = "Camera",
            //    Description = "Hd  cameras",
            //    Quantity = 300
            //});
            //inventoryList.Add(new Product
            //{
            //    Id = "P104",
            //    Name = "Mobile",
            //    Description = "All Smartphones",
            //    Quantity = 450
            //});
            //inventoryList.Add(new Product
            //{
            //    Id = "P105",
            //    Name = "Notepad",
            //    Description = "All branded of notepads",
            //    Quantity = 670
            //});
            //inventoryList.Add(new Product
            //{
            //    Id = "P106",
            //    Name = "Harddisk",
            //    Description = "All type of Harddisk",
            //    Quantity = 1200
            //});
            //inventoryList.Add(new Product
            //{
            //    Id = "P107",
            //    Name = "PenDrive",
            //    Description = "All type of Pendrive",
            //    Quantity = 370
            //});
            //return View(inventoryList);

            //var userManager = new ApplicationUserManager(new UserStore<ApplicationUser>(DataContext));
            //var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(DataContext));
            ////roleManager.Create(new IdentityRole("Owner"));
            ////roleManager.Create(new IdentityRole("Teacher"));
            ////roleManager.Create(new IdentityRole("Promoter"));
            ////roleManager.Create(new IdentityRole("Admin"));

            //// Assign users to role
            //var userid = User.Identity.GetUserId();
            //userManager.AddToRole(userid, "Admin");
            //userManager.AddToRole(userid, "Teacher");
            //userManager.AddToRole(userid, "Owner");
            //userManager.AddToRole(userid, "Promoter");


            //Here  MyDatabaseEntities  is our datacontext 
            List<OrganizationMember> members = DataContext.OrganizationMembers.Where(m => m.OrganizationId == 2).Include("User").ToList();
            return View(members);
        }

        //public ActionResult Index2()
        //{
        //    return View();
        //}

        //public ActionResult MapTest()
        //{
        //    return View();
        //}

        public ActionResult AddJobCache()
        {
            return null;
        }

        public ActionResult Explore()
        {
            var viewModel = new HomeExploreViewModel();
            viewModel.DanceStyles = DataContext.DanceStyles.Include("Dancers").ToList();
            return View(viewModel);
        }

        public ActionResult PrivacyPolicy()
        {
            return View();
        }

        public ActionResult TermsofService()
        {
            return View();
        }

        public ActionResult Social(int? danceStyle, int? place, string location, string[] days, double? CenterLat, double? CenterLng, int? Zoom, double? NELat, double? NELng, double? SWLat, double? SWLng)
        {
            var viewModel = new SocialViewModel();
            viewModel.DanceStyles = DataContext.DanceStyles;
            viewModel.Places = DataContext.Places;
            viewModel.Zoom = Zoom == null ? 10 : (int)Zoom;
            viewModel.Location = location;

            if (location != "" && location != null)
            {
                var address = new Address();
                address = Geolocation.ParseAddress(location);
                CenterLat = address.Latitude;
                CenterLng = address.Longitude;
                NELat = CenterLat + .5;
                SWLat = CenterLat - .5;
                NELng = CenterLng + .5;
                SWLng = CenterLng - .5;
            }

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


            viewModel.Socials = DataContext.Events.OfType<Social>().Include("DanceStyles").Include("EventMembers").Include("EventMembers.Member").Include("Reviews").Where(x => x.IsAvailable == true).Where(y => y.EndDate == null || y.EndDate >= DateTime.Now).OrderBy(z => z.StartDate).ToList();
            //viewModel.Concerts = DataContext.Events.OfType<Concert>().Include("DanceStyles").Include("EventMembers").Include("EventMembers.Member").Where(x => x.IsAvailable == true).Where(y => y.EndDate == null || y.EndDate >= DateTime.Now).OrderBy(z => z.StartDate).ToList();
            //viewModel.Conferences = DataContext.Events.OfType<Conference>().Include("DanceStyles").Include("EventMembers").Include("EventMembers.Member").Where(x => x.IsAvailable == true).Where(y => y.EndDate == null || y.EndDate >= DateTime.Now).OrderBy(z => z.StartDate).ToList();
            //viewModel.Parties = DataContext.Events.OfType<Party>().Include("DanceStyles").Include("EventMembers").Include("EventMembers.Member").Where(x => x.IsAvailable == true).Where(y => y.EndDate == null || y.EndDate >= DateTime.Now).OrderBy(z => z.StartDate).ToList();
            //viewModel.OpenHouses = DataContext.Events.OfType<OpenHouse>().Include("DanceStyles").Include("EventMembers").Include("EventMembers.Member").Where(x => x.IsAvailable == true).Where(y => y.EndDate == null || y.EndDate >= DateTime.Now).OrderBy(z => z.StartDate).ToList();

            if (danceStyle != null)
            {
                viewModel.Socials = viewModel.Socials.Where(x => x.DanceStyles.Any(s => s.Id == danceStyle));
            }

            if (place != null)
            {
                viewModel.Socials = viewModel.Socials.Where(x => x.Place.Id == place);
            }

            if (days != null)
            {
                viewModel.Socials = viewModel.Socials.Where(x => viewModel.Days.Contains(x.Day));
            }

            if (CenterLat != null && CenterLng != null)
            {
                //  var myLocation = DbGeography.FromText("POINT(" + address.Longitude.ToString() + " " + address.Latitude.ToString() + ")");
                //  viewModel.Classes = viewModel.Classes.Where(c => DbGeography.FromText("POINT(" + c.Place.Longitude.ToString() + " " + c.Place.Latitude.ToString() + ")").Distance(myLocation) * .00062 < 50);
                viewModel.Socials = viewModel.Socials.Where(c => c.Place.Longitude >= SWLng && c.Place.Longitude <= NELng && c.Place.Latitude >= SWLat && c.Place.Latitude <= NELat);

                //  Set Map Location
                viewModel.SearchAddress = new Address() { Latitude = (double)CenterLat, Longitude = (double)CenterLng };
                //  Set Map Location
            }

            viewModel.Socials = viewModel.Socials.Take(25);

            return View(viewModel);
        }

        public ActionResult Learn(LearnViewModel model)
        {
            model.Classes = DataContext.Classes
                                .Include("Teachers.ApplicationUser")
                                .Include("DanceStyles")
                                .Include("Place")
                                .Include("EventMembers.Member")
                                .Include("Place")
                                .Include("Reviews")
                                .AsQueryable();
            if (model.DanceStyleId != null)
            {
                model.Classes = model.Classes.Where(c => c.DanceStyles.Select(st => st.Id).Contains((int)model.DanceStyleId));
            }
            if (model.TeacherId != null)
            {
                model.Classes = model.Classes.Where(c => c.Teachers.Select(t => t.ApplicationUser.Id).Contains(model.TeacherId));
            }

            if (model.NELat != null && model.SWLng != null)
            {
                model.Classes = model.Classes.Where(c => c.Place.Longitude >= model.SWLng && c.Place.Longitude <= model.NELng && c.Place.Latitude >= model.SWLat && c.Place.Latitude <= model.NELat);
            }
            if (model.SkillLevel != null)
            {
                model.Classes = model.Classes.Where(x => x.SkillLevel == model.SkillLevel);
            }
            if (model.Days != null)
            {
                model.Classes = model.Classes.Where(x => model.Days.Contains(x.Day));
            }

            model.Classes = model.Classes.ToList().Take(100);
            return View(model);
        }

        //public ActionResult Learn(int? danceStyle, string TeacherId, int? PlaceId, int? skillLevel, string location, string[] days, double? CenterLat, double? CenterLng, int? Zoom, double? NELat, double? NELng, double? SWLat, double? SWLng)
        //{
        //    var viewModel = new LearnViewModel();
        //    viewModel.DanceStyles = DataContext.DanceStyles;
        //    viewModel.Places = DataContext.Places;
        //    viewModel.Teachers = DataContext.Teachers.Include("ApplicationUser");
        //    viewModel.Zoom = Zoom == null ? 10 : (int)Zoom;
        //    viewModel.Location = location;
        //    viewModel.SearchAddress = new Address();

        //    if (location != "" && location != null)
        //    {
        //        var address = new Address();
        //        address = Geolocation.ParseAddress(location);
        //        CenterLat = address.Latitude;
        //        CenterLng = address.Longitude;
        //        NELat = CenterLat + .5;
        //        SWLat = CenterLat - .5;
        //        NELng = CenterLng + .5;
        //        SWLng = CenterLng - .5;
        //    }

        //    var dayslist = new List<DayOfWeek>();
        //    if (days != null)
        //    {
        //        foreach (var s in days)
        //        {
        //            dayslist.Add((DayOfWeek)Enum.Parse(typeof(DayOfWeek), s));
        //        }
        //    }
        //    viewModel.Days = dayslist;
        //    viewModel.DaysOfWeek = new List<DayOfWeek>() { DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday };
        //    //  viewModel.DanceStyles = new SelectList(DanceStyleLst, "Id", "Name", danceStyle);

        //    //var PlaceLst = DataContext.Places.ToList();
        //    //ViewBag.place = new SelectList(PlaceLst, "Id", "Name", place);

        //    //var SkillLevelLst = new List<int> { 1, 2, 3, 4, 5 };
        //    //ViewBag.skillLevel = new SelectList(SkillLevelLst);

        //    //var TeacherLst = DataContext.Teachers.ToList();
        //    //ViewBag.teacher = new SelectList(TeacherLst, "ApplicationUser.Id", "ApplicationUser.FullName", teacher);

        //    var teachers = DataContext.Teachers.Include("ApplicationUser");
        //    viewModel.Classes = DataContext.Events.OfType<Class>().Include("Teachers").Include("Teachers.ApplicationUser").Include("DanceStyles").Include("EventMembers.Member").Include("Place").Include("Reviews").Where(x => x.IsAvailable == true).Where(y => y.EndDate == null || y.EndDate >= DateTime.Now).OrderBy(z => z.StartDate);
        //    //  viewModel.Workshops = DataContext.Events.OfType<Workshop>().Include("Teachers").Include("Teachers.ApplicationUser").Include("DanceStyles").Include("EventMembers.Member").Include("Place").Where(x => x.IsAvailable == true).Where(y => y.EndDate == null || y.EndDate >= DateTime.Now).OrderBy(z => z.StartDate).ToList();

        //    if (danceStyle != null)
        //    {
        //        viewModel.Classes = viewModel.Classes.Where(x => x.DanceStyles.Any(s => s.Id == danceStyle));
        //    }

        //    if (TeacherId != null && TeacherId != "")
        //    {
        //        viewModel.Classes = viewModel.Classes.Where(x => x.Teachers.Any(t => t.ApplicationUser.Id == TeacherId));
        //    }

        //    if (PlaceId != null)
        //    {
        //        viewModel.Classes = viewModel.Classes.Where(x => x.Place.Id == PlaceId);
        //    }

        //    if (skillLevel != null)
        //    {
        //        viewModel.Classes = viewModel.Classes.Where(x => x.SkillLevel == skillLevel);
        //    }

        //    if (days != null)
        //    {
        //        viewModel.Classes = viewModel.Classes.Where(x => viewModel.Days.Contains(x.Day));
        //    }

        //    if (CenterLat != null && CenterLng != null)
        //    {
        //        //  var myLocation = DbGeography.FromText("POINT(" + address.Longitude.ToString() + " " + address.Latitude.ToString() + ")");
        //        //  viewModel.Classes = viewModel.Classes.Where(c => DbGeography.FromText("POINT(" + c.Place.Longitude.ToString() + " " + c.Place.Latitude.ToString() + ")").Distance(myLocation) * .00062 < 50);
        //        viewModel.Classes = viewModel.Classes.Where(c => c.Place.Longitude >= SWLng && c.Place.Longitude <= NELng && c.Place.Latitude >= SWLat && c.Place.Latitude <= NELat);

        //        //  Set Map Location
        //        viewModel.SearchAddress = new Address() { Latitude = (double)CenterLat, Longitude = (double)CenterLng };
        //        //  Set Map Location
        //    }

        //    viewModel.Classes = viewModel.Classes.Take(25);

        //    return View(viewModel);
        //}

        public ActionResult Teachers()
        {
            return View(DataContext.Teachers.Include("DanceStyles").ToList());
        }
        //public ActionResult Test2()
        //{
        //    var model = new Test2();
        //    model.Members = DataContext.OrganizationMembers.Include("User").ToList();
        //    return View(model);
        //}
        //[HttpPost]
        //public ActionResult Test2(Test2 model)
        //{
        //    return View(model);
        //}

        public ActionResult Test3()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Test3(OrderViewModel model)
        {
            model.Order.OrderDetails.Add(new OrderDetail() { TicketId = model.TicketId, Quantity = model.Quantity });
            StoreController.PostTransaction(model);
            return RedirectToAction("Test3", "Home");
        }
    }
}
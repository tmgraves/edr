using AutoPoco;
using AutoPoco.DataSources;
using AutoPoco.Engine;
using EDR.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web;

namespace EDR.Data
{
    public class ApplicationDbInitializer : DropCreateDatabaseIfModelChanges<ApplicationDbContext>
    {
        protected override void Seed(ApplicationDbContext context)
        {
            // Initialize hydrator
            IGenerationSessionFactory factory = AutoPocoContainer.Configure(x =>
            {
                x.Conventions(c =>
                {
                    c.UseDefaultConventions();
                });

                x.AddFromAssemblyContainingType<Class>();
                x.Include<Class>()
                    .Setup(c => c.Name).Use<RandomStringSource>(10, 20)
                    .Setup(c => c.Description).Use<LoremIpsumSource>(2)
                    .Setup(c => c.StartDate).Use<DateOfBirthSource>();
            });
            IGenerationSession session = factory.CreateSession();

            // Initialize identiy managers
            var userManager = new ApplicationUserManager(new UserStore<ApplicationUser>(context));
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

            // Seed users
            var user = new ApplicationUser() { UserName = "user@gmail.com", Email = "user@gmail.com", FirstName = "Johnny", LastName = "Depp", ZipCode = "90210" };
            var tchr = new ApplicationUser() { UserName = "teacher@gmail.com", Email = "teacher@gmail.com", FirstName = "Angelina", LastName = "Jolie", ZipCode = "90210" };
            var prom = new ApplicationUser() { UserName = "promoter@gmail.com", Email = "promoter@gmail.com", FirstName = "Katy", LastName = "Perry", ZipCode = "90210" };

            // Save seeded users
            userManager.Create(user, "Passw0rd!");
            userManager.Create(tchr, "Passw0rd!");
            userManager.Create(prom, "Passw0rd!");

            // Save seeded roles 
            roleManager.Create(new IdentityRole("Teacher"));
            roleManager.Create(new IdentityRole("Promoter"));

            // Assign users to role
            userManager.AddToRole(tchr.Id, "Teacher");
            userManager.AddToRole(prom.Id, "Promoter");

            // Seed Restaurants
            var res = new Restaurant() { Name = "El Floridita", Address = "1253 Vine St", City = "Los Angeles", State = "CA", Zip = "90038" };
            var res2 = new Restaurant() { Name = "Monsoon Cafe", Address = "1212 3rd St", City = "Santa Monica", State = "CA", Zip = "90401" };

            // Seed Hotel
            var trop = new Hotel() { Name = "Tropicana", Address = "3801 Las Vegas Boulevard South", City = "Las Vegas", State = "NV", Zip = "89109" };
            var hilt = new Hotel() { Name = "Hilton", Address = "2100 E Mariposa Ave", City = "El Segundo", State = "CA", Zip = "90245" };

            // Seed Theater
            var gre = new Theater() { Name = "Nokia", Address = "777 Chick Hearn Ct", City = "Los Angeles", State = "CA", Zip = "90015" };
            var hb = new Theater() { Name = "Hollywood Bowl", Address = "2301 N Highland Ave", City = "Los Angeles", State = "CA", Zip = "90068" };

            // Seed Studio
            var thd = new Studio() { Name = "3rd Street Dance", Address = "8558 W 3rd St", City = "Los Angeles", State = "CA", Zip = "90048" };
            var kar = new Studio() { Name = "Karavan", Address = "1626 S Central Ave", City = "Glendale", State = "CA", Zip = "91204" };

            // Seed ConferenceCenter
            var lac = new ConferenceCenter() { Name = "LA Convention Center", Address = "1201 S Figueroa St", City = "Los Angeles", State = "CA", Zip = "90015" };
            var anc = new ConferenceCenter() { Name = "Anaheim Convention Center", Address = "800 W Katella Ave", City = "Anaheim", State = "CA", Zip = "92802" };

            // Seed Nightclubs
            var zan = new Nightclub() { Name = "Zanzibar", Address = "1301 5th St", City = "Santa Monica", State = "CA", Zip = "90401" };
            var may = new Nightclub() { Name = "Mayan", Address = "1038 S Hill St", City = "Los Angeles", State = "CA", Zip = "90015" };

            // Seed data
            var style1 = new DanceStyle { Name = "Salsa (on 1)" };
            var style2 = new DanceStyle { Name = "Salsa (on 2)" };
            var style3 = new DanceStyle { Name = "Bachata" };
            var style4 = new DanceStyle { Name = "Cha cha cha" };
            var style5 = new DanceStyle { Name = "Tango" };
            var style6 = new DanceStyle { Name = "Mambo" };

            var classes = session.List<Class>(110)
                .First(20)
                    .Impose(x => x.DanceStyle, style1)
                    .Impose(x => x.Price, (decimal)25.00)
                    .Impose(x => x.Place, thd)
                .Next(20)
                    .Impose(x => x.DanceStyle, style2)
                    .Impose(x => x.Price, (decimal)20.00)
                    .Impose(x => x.Place, kar)
                .Next(10)
                    .Impose(x => x.DanceStyle, style3)
                    .Impose(x => x.Price, (decimal)10.00)
                    .Impose(x => x.Place, zan)
                .Next(20)
                    .Impose(x => x.DanceStyle, style4)
                    .Impose(x => x.Price, (decimal)5.00)
                    .Impose(x => x.Place, may)
                .Next(20)
                    .Impose(x => x.DanceStyle, style5)
                    .Impose(x => x.Place, gre)
                .Next(20)
                    .Impose(x => x.DanceStyle, style6)
                    .Impose(x => x.Place, hb)
                .All()
                    .Impose(x => x.EndDate, DateTime.Now.AddDays(30))
                    .Impose(x => x.IsAvailable, true)
                .Get();

            context.Classes.AddRange(classes);
            context.Restaurants.Add(res);
            context.Restaurants.Add(res2);
            context.Hotels.Add(trop);
            context.Hotels.Add(hilt);
            context.Theaters.Add(gre);
            context.Theaters.Add(hb);
            context.Studios.Add(thd);
            context.Studios.Add(kar);
            context.ConferenceCenters.Add(lac);
            context.ConferenceCenters.Add(anc);
            context.Nightclubs.Add(zan);
            context.Nightclubs.Add(may);
            context.SaveChanges();

            context.Concerts.Add(new Concert() { Name = "Marc Anthony", Description = "Marc Anthony in concert", StartDate = Convert.ToDateTime("9/22/2014 8:00 PM"), EndDate = Convert.ToDateTime("9/23/2014 1:00 AM"), Price = 50, IsAvailable = true });
            context.Concerts.Add(new Concert() { Name = "Romeo Santos", Description = "Romeo Santos in concert", StartDate = Convert.ToDateTime("10/22/14 9:00 PM"), EndDate = Convert.ToDateTime("10/23/14 12:00 AM"), Price = 85 });
            context.Conferences.Add(new Conference() { Name = "LA Salsa Congress", Description = "LA Salsa Congress", StartDate = Convert.ToDateTime("5/23/14 6:00 PM"), EndDate = Convert.ToDateTime("5/27/14 6:00 AM"), Price = 340 });
            context.Conferences.Add(new Conference() { Name = "LA Bachata Festival", Description = "LA Bachata Festival", StartDate = Convert.ToDateTime("8/15/14 6:00 PM"), EndDate = Convert.ToDateTime("8/19/14 6:00 AM"), Price = 250 });
            context.OpenHouses.Add(new OpenHouse() { Name = "Summer Open House", Description = "Summer Open House", StartDate = Convert.ToDateTime("7/30/14 6:00 PM"), EndDate = Convert.ToDateTime("7/31/14 2:00 AM"), Price = 0 });
            context.OpenHouses.Add(new OpenHouse() { Name = "Dance Showcase", Description = "Dance Showcase", StartDate = Convert.ToDateTime("9/15/14 6:00 PM"), EndDate = Convert.ToDateTime("9/16/14 2:00 AM"), Price = 0 });
            context.Socials.Add(new Social() { Name = "Wednesday Salsa Social", Description = "Monsoon Social", StartDate = Convert.ToDateTime("7/23/14 8:00 PM"), EndDate = Convert.ToDateTime("7/24/14 2:00 AM"), Price = 12 });
            context.Socials.Add(new Social() { Name = "Noypitz Social", Description = "Noypitz Social", StartDate = Convert.ToDateTime("7/27/14 8:00 PM"), EndDate = Convert.ToDateTime("7/28/14 2:00 AM"), Price = 7 });
            context.Workshops.Add(new Workshop() { Name = "Pachanga Bootcamp", Description = "Pachanga Bootcamp", StartDate = Convert.ToDateTime("7/26/14 2:00 PM"), EndDate = Convert.ToDateTime("7/26/14 6:00 PM"), Price = 50 });
            context.Workshops.Add(new Workshop() { Name = "Bachata Bootcamp", Description = "Bachata Bootcamp", StartDate = Convert.ToDateTime("8/26/14 2:00 PM"), EndDate = Convert.ToDateTime("8/26/14 6:00 PM"), Price = 40 });
            context.Parties.Add(new Party() { Name = "Joe's Birthday", Description = "Joe's Birthday", StartDate = Convert.ToDateTime("9/26/14 2:00 PM"), EndDate = Convert.ToDateTime("9/26/14 6:00 PM"), Price = 10 });
            context.SaveChanges();

            context.Teams.Add(new Team() { GroupName = "Christian Team 1 Salsa", GroupDescription = "Level 1 Salsa Performance Team", SkillLevel = 2, TeamManagerName = "Fred Smith", Public = true, FacebookLink = "www.facebook.com/profiles/team1salsa" });
            context.Teams.Add(new Team() { GroupName = "Christian Advanced Bachata", GroupDescription = "Advanced Bachata Performance Team", SkillLevel = 3, TeamManagerName = "Jerry Jones", Public = true, FacebookLink = "www.facebook.com/profiles/bachata3team" });
            context.Schools.Add(new School() { GroupName = "Bachata Caliente", GroupDescription = "Bachata School", SkillLevel = 0, Public = true, FacebookLink = "www.facebook.com/profiles/bachatacaliente" });

            context.SaveChanges();
        }
    }
}
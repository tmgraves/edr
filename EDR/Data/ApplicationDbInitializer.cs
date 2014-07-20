using AutoPoco;
using AutoPoco.DataSources;
using AutoPoco.Engine;
using EDR.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
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
                    .Setup(c => c.Summary).Use<LoremIpsumSource>()
                    .Setup(c => c.Description).Use<LoremIpsumSource>(2)
                    .Setup(c => c.StartDate).Use<DateOfBirthSource>();
            });
            IGenerationSession session = factory.CreateSession();

            // Seed data
            var style1 = new DanceStyle { Name = "Salsa (on 1)" };
            var style2 = new DanceStyle { Name = "Salsa (on 2)" };
            var style3 = new DanceStyle { Name = "Bachata" };
            var style4 = new DanceStyle { Name = "Cha cha cha" };
            var style5 = new DanceStyle { Name = "Tango" };
            var style6 = new DanceStyle { Name = "Mambo" };

            var classes = session.List<Class>(100)
                .First(20)
                    .Impose(x => x.DanceStyle, style1)
                    .Impose(x => x.Price, (decimal)25.00)
                .Next(20)
                    .Impose(x => x.DanceStyle, style2)
                    .Impose(x => x.Price, (decimal)20.00)
                .Next(10)
                    .Impose(x => x.DanceStyle, style2)
                    .Impose(x => x.Price, (decimal)10.00)
                .Next(20)
                    .Impose(x => x.DanceStyle, style2)
                    .Impose(x => x.Price, (decimal)5.00)
                .Next(20)
                    .Impose(x => x.DanceStyle, style2)
                .Next(20)
                    .Impose(x => x.DanceStyle, style2)
                .All()
                    .Impose(x => x.EndDate, DateTime.Now.AddDays(30))
                    .Impose(x => x.IsAvailable, true)
                .Get();

            // Save seeded data
            context.Classes.AddRange(classes);
            context.SaveChanges();

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
        }
    }
}
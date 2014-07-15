using AutoPoco;
using AutoPoco.Engine;
using AutoPoco.DataSources;
using EDR.Models;
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
            var styles = new List<DanceStyle>()
            {
                new DanceStyle { Name = "Salsa (on 1)" },
                new DanceStyle { Name = "Salsa (on 2)" },
                new DanceStyle { Name = "Bachata" },
                new DanceStyle { Name = "Cha cha cha" },
                new DanceStyle { Name = "Tango" },
                new DanceStyle { Name = "Mambo" }
            };

            //IGenerationSessionFactory factory = AutoPocoContainer.Configure(x =>
            //{
            //    x.Conventions(c =>
            //    {
            //        c.UseDefaultConventions();
            //    });

            //    x.AddFromAssemblyContainingType<Class>();
            //    x.Include<Class>()
            //        .Setup(c => c.Name).Use<LoremIpsumSource>()
            //        .Setup(c => c.Summary).Use<LoremIpsumSource>()
            //        .Setup(c => c.Description).Use<LoremIpsumSource>(2);
            //});

            //IGenerationSession session = factory.CreateSession();

            context.DanceStyles.AddRange(styles);
            context.SaveChanges();
        }
    }
}
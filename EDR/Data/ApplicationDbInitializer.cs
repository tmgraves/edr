using EDR.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using EDR.Utilities;

namespace EDR.Data
{
    public class ApplicationDbInitializer : DropCreateDatabaseIfModelChanges<ApplicationDbContext>
    {
        protected override void Seed(ApplicationDbContext context)
        {
            // Seed dance styles
            var styles = new List<DanceStyle>()
            {
                new DanceStyle { Name = "Salsa", YouTubeVideoID="sAmBsIZAbO8", Description="Salsa is a popular form of social dance that originated in New York with strong influences from Latin America, particularly Cuba and Puerto Rico." },
                new DanceStyle { Name = "Pachanga", YouTubeVideoID="8uvi_-UKGmA", Description="As a dance, pachanga has been described as 'a happy-go-lucky dance' of Cuban origin with a Charleston flavor due to the double bending and straightening of the knees. It is danced to the downbeat of four-four time to the usual mambo offbeat music characterized by the charanga instrumentation of flutes, violins, and drums." },
                new DanceStyle { Name = "Bachata", YouTubeVideoID="Ig_oVESyLFI", Description="The Dominican Bachata dance style is the original style of Bachata, originating from the Dominican Republic where the music also was born." },
                new DanceStyle { Name = "Cha cha cha", YouTubeVideoID="wcO-GEhZEFs", Description="As described above, the basis of the modern dance was laid down in the 1950s by Pierre & Lavelle[10] and developed in the 1960s by Walter Laird and other top competitors of the time. The basic steps taught to learners today are based on these accounts." },
                new DanceStyle { Name = "Tango", YouTubeVideoID="J6Ja5soU-5M", Description="Argentine tango dancing consists of a variety of styles that developed in different regions and eras, and in response to the crowding of the venue and even the fashions in clothing. Even though the present forms developed in Argentina and Uruguay, they were also exposed to influences re-imported from Europe and North America." },
                new DanceStyle { Name = "Mambo", YouTubeVideoID="ecjSMANTuaU", Description="Mambo is a Latin dance of Cuba. Mambo music was invented during the 1930s by Arsenio Rodríguez,[1] developed in Havana by Cachao and made popular by Dámaso Pérez Prado and Benny Moré." },
                new DanceStyle { Name = "Hustle", YouTubeVideoID="NllknGUIOWE", Description="The Hustle is a catchall name for some disco dances which were extremely popular in the 1970s. Today it mostly refers to the unique partner dance done in ballrooms and nightclubs to disco music.[1] It has some features in common with swing dance." },
                new DanceStyle { Name = "West Coast Swing", YouTubeVideoID="8_pQ-tcKnbY", Description="West Coast Swing (WCS aka Push or Whip) is a partner dance with roots in Lindy Hop. It is characterized by a distinctive elastic look that results from its basic extension-compression technique of partner connection, and is danced primarily in a slotted area on the dance floor. " }
            };

            // Seed events
            var latinstyles = new List<DanceStyle>() { styles[0], styles[2], styles[3], styles[1] };
            var tangostyles = new List<DanceStyle>() { styles[4] };
            var mambostyles = new List<DanceStyle>() { styles[5] };

            // Initialize identiy managers
            var userManager = new ApplicationUserManager(new UserStore<ApplicationUser>(context));
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

            // Seed users
            var user = new ApplicationUser() { UserName = "user@gmail.com", Email = "user@gmail.com", FirstName = "Johnny", LastName = "Depp", ZipCode = "90210" };
            var liz = new ApplicationUser() { UserName = "teacher@gmail.com", Email = "teacher@gmail.com", FirstName = "Liz", LastName = "Lirases", ZipCode = "90210" };
            var tchr = new Teacher()    { ApplicationUser=liz, Resume = "Liz Lira, the 'Rose of Salsa,' was born in La Paz, Bolivia, and immigrated to the United States at the early age of eight. Making Southern California her new home, she embraced the art of dance and excelled in both ballet and jazz.", DanceStyles = latinstyles, FacebookLink = "https://www.facebook.com/liz.lirala", Website = "http://www.lizlira.com/" };
            var prom = new ApplicationUser() { UserName = "promoter@gmail.com", Email = "promoter@gmail.com", FirstName = "Katy", LastName = "Perry", ZipCode = "90210" };
            var eddie = new ApplicationUser() { UserName = "teacher2@gmail.com", Email = "teacher2@gmail.com", FirstName = "Eddie", LastName = "Torres", ZipCode = "90056" };
            var tchr2 = new Teacher() { ApplicationUser=eddie, Resume = "Eddie Torres (born on July 3, 1950), also known as 'The Mambo King', is a salsa dance instructor.[1] Torres' technique developed from various sources including Afro-Cuban son, mambo, and North American jazz dance. [2] He is one of the more popular dancers of New York style salsa. He is famous for his way of dancing and teaching salsa, with the female starting to move forward (always On 2 timing).[3] Torres' style can be contrasted with the more showy Los Angeles style.", DanceStyles = latinstyles, FacebookLink = "https://www.facebook.com/pages/Eddie-Torres/160918620698544", Website = "http://www.eddietorresny.com/Eddie_Torres_NY/Eddie_Torres_NY.com.html" };

            // Save seeded users
            userManager.Create(user, "Passw0rd!");
            userManager.Create(liz, "Passw0rd!");
            userManager.Create(prom, "Passw0rd!");
            userManager.Create(eddie, "Passw0rd!");

            // Save seeded roles 
            roleManager.Create(new IdentityRole("Teacher"));
            roleManager.Create(new IdentityRole("Promoter"));

            // Assign users to role
            userManager.AddToRole(liz.Id, "Teacher");
            userManager.AddToRole(eddie.Id, "Promoter");

            // Seed places
            var places = new List<Place>()
            {
                new Restaurant() { Name = "El Floridita", Address = "1253 Vine St", City = "Los Angeles", State = "CA", Zip = "90038", Country = "USA", FacebookLink="https://www.facebook.com/elfloridita", Website="http://www.elfloridita.com/" },
                new Restaurant() { Name = "Monsoon Cafe", Address = "1212 3rd St", City = "Santa Monica", State = "CA", Zip = "90401", Country = "USA", FacebookLink="https://www.facebook.com/monsoonsalsanight?fref=ts&ref=br_tf", Website="https://www.facebook.com/MonsoonCafeSM"  },
                new Hotel() { Name = "Tropicana", Address = "3801 Las Vegas Boulevard South", City = "Las Vegas", State = "NV", Zip = "89109", Country = "USA", FacebookLink="https://www.facebook.com/troplv", Website="http://www.troplv.com/" },
                new Hotel() { Name = "Hilton", Address = "2100 E Mariposa Ave", City = "El Segundo", State = "CA", Zip = "90245", Country = "USA", FacebookLink="https://www.facebook.com/HiltonLAX", Website="http://www.hilton.com/AirportLAX" },
                new Theater() { Name = "Nokia", Address = "777 Chick Hearn Ct", City = "Los Angeles", State = "CA", Zip = "90015", Country = "USA", FacebookLink="https://www.facebook.com/nokiala", Website="http://www.nokiatheatrelalive.com/" },
                new Theater() { Name = "Hollywood Bowl", Address = "2301 N Highland Ave", City = "Los Angeles", State = "CA", Zip = "90068", Country = "USA", FacebookLink="https://www.facebook.com/HollywoodBowl", Website="http://www.hollywoodbowl.com/" },
                new Studio() { Name = "3rd Street Dance", Address = "8558 W 3rd St", City = "Los Angeles", State = "CA", Zip = "90048", Country = "USA", FacebookLink="https://www.facebook.com/3rdstreetdance", Website="http://www.thirdstreetdance.com/" },
                new Studio() { Name = "Karavan", Address = "1626 S Central Ave", City = "Glendale", State = "CA", Zip = "91204", Country = "USA", FacebookLink="https://www.facebook.com/pages/Karavan-Dance-Studio/160508247306728", Website="http://karavandancestudio.com" },
                new ConferenceCenter() { Name = "LA Convention Center", Address = "1201 S Figueroa St", City = "Los Angeles", State = "CA", Zip = "90015", Country = "USA", FacebookLink="https://www.facebook.com/ConventionLA", Website="http://www.lacclink.com" },
                new ConferenceCenter() { Name = "Anaheim Convention Center", Address = "800 W Katella Ave", City = "Anaheim", State = "CA", Zip = "92802", Country = "USA", FacebookLink="https://www.facebook.com/AnaheimConventionCenter", Website="http://www.anaheimconventioncenter.com/" },
                new Nightclub() { Name = "Zanzibar", Address = "1301 5th St", City = "Santa Monica", State = "CA", Zip = "90401", Country = "USA", FacebookLink="https://www.facebook.com/ZanzibarLive", Website="http://www.zanzibarlive.com/" },
                new Nightclub() { Name = "Mayan", Address = "1038 S Hill St", City = "Los Angeles", State = "CA", Zip = "90015", Country = "USA", FacebookLink="https://www.facebook.com/the.mayan.la", Website="http://www.clubmayan.com/" },
                new Studio() { Name = "Dance Doctor", Address = "1440 4th St", City = "Santa Monica", State = "CA", Zip = "90401", Country = "USA", FacebookLink="https://www.facebook.com/JohnCasseseTheDanceDoctor", Website="http://www.dancedoctor.com/" },
                new Studio() { Name = "Granada", Address = "17 S 1st St", City = "Alhambra", State = "CA", Zip = "91801", Country = "USA", FacebookLink="https://www.facebook.com/TheGranadaLAAlhambra", Website="http://www.thegranadala.com/" },
                new Nightclub() { Name = "Ixtapa", Address = "119 E Colorado Blvd", City = "Pasadena", State = "CA", Zip = "91105", Country = "USA", FacebookLink="https://www.facebook.com/pages/IXTAPA-LOUNGE/188629852685", Website="http://www.cantinaixtapa.com/" }
            };

            var events = new List<Event>()
            {
                new Concert() { Name = "Marc Anthony", Description = "Marc Anthony in concert", Place = places[4], StartDate = Convert.ToDateTime("9/22/2014 8:00 PM"), EndDate = Convert.ToDateTime("9/23/2014 1:00 AM"), Price = 50, IsAvailable = true, DanceStyles=latinstyles, Time=new TimeSpan(20,0,0), Duration=ApplicationUtility.CalculateTime(0, 4, 0), Recurring=false },
                new Concert() { Name = "Romeo Santos", Description = "Romeo Santos in concert", Place = places[5], StartDate = Convert.ToDateTime("10/22/14 9:00 PM"), EndDate = Convert.ToDateTime("10/23/14 12:00 AM"), Price = 85, IsAvailable = true, DanceStyles=new List<DanceStyle> { styles[2]}, Time=new TimeSpan(21,0,0), Duration=ApplicationUtility.CalculateTime(0, 4, 0), Recurring=false },
                new Conference() { Name = "LA Salsa Congress", Description = "LA Salsa Congress", Place = places[2], StartDate = Convert.ToDateTime("5/23/14 6:00 PM"), EndDate = Convert.ToDateTime("5/27/14 6:00 AM"), Price = 340, IsAvailable = true, DanceStyles=latinstyles, Time=new TimeSpan(18,0,0), Duration=ApplicationUtility.CalculateTime(3, 0, 0), Recurring=false },
                new Conference() { Name = "LA Bachata Festival", Description = "LA Bachata Festival", Place = places[3], StartDate = Convert.ToDateTime("8/15/14 6:00 PM"), EndDate = Convert.ToDateTime("8/19/14 6:00 AM"), Price = 250, IsAvailable = true, DanceStyles=latinstyles, Time=new TimeSpan(20,0,0), Duration=ApplicationUtility.CalculateTime(3, 0, 0), Recurring=false },
                new OpenHouse() { Name = "Summer Open House", Description = "Summer Open House", Place = places[6], StartDate = Convert.ToDateTime("7/30/14 6:00 PM"), EndDate = Convert.ToDateTime("7/31/14 2:00 AM"), Price = 0, IsAvailable = true, DanceStyles=styles, Time=new TimeSpan(18,0,0), Duration=ApplicationUtility.CalculateTime(0, 4, 0), Recurring=false },
                new OpenHouse() { Name = "Dance Showcase", Description = "Dance Showcase", Place = places[7], StartDate = Convert.ToDateTime("9/15/14 6:00 PM"), EndDate = Convert.ToDateTime("9/16/14 2:00 AM"), Price = 0, IsAvailable = true, DanceStyles=styles, Time=new TimeSpan(19,0,0), Duration=ApplicationUtility.CalculateTime(0, 4, 0), Recurring=false },
                new Social() { Name = "Wednesday Salsa Social", Description = "Monsoon Social", Place = places[10], StartDate = Convert.ToDateTime("7/23/14 8:00 PM"), EndDate = Convert.ToDateTime("7/24/14 2:00 AM"), Price = 12, IsAvailable = true, DanceStyles=latinstyles, Time=new TimeSpan(21,0,0), Duration=ApplicationUtility.CalculateTime(0, 5, 0), Recurring=true, Frequency=Frequency.Weekly, Day=DayOfWeek.Wednesday, Interval=1 },
                new Social() { Name = "Noypitz Social", Description = "Noypitz Social", Place = places[11], StartDate = Convert.ToDateTime("7/27/14 8:00 PM"), EndDate = Convert.ToDateTime("7/28/14 2:00 AM"), Price = 7, IsAvailable = true, DanceStyles=latinstyles, Time=new TimeSpan(21,0,0), Duration=ApplicationUtility.CalculateTime(0, 5, 0), Recurring=true, Frequency=Frequency.Weekly, Day=DayOfWeek.Sunday, Interval=1  },
                new Workshop() { Name = "Pachanga Bootcamp", Description = "Pachanga Bootcamp", Place = places[8], StartDate = Convert.ToDateTime("7/26/14 2:00 PM"), EndDate = Convert.ToDateTime("7/26/14 6:00 PM"), Price = 50, IsAvailable = true, DanceStyles=new List<DanceStyle> { styles[1] }, Time=new TimeSpan(12,0,0), Duration=ApplicationUtility.CalculateTime(0, 4, 0), Recurring=false },
                new Workshop() { Name = "Bachata Bootcamp", Description = "Bachata Bootcamp", Place = places[9], StartDate = Convert.ToDateTime("8/26/14 2:00 PM"), EndDate = Convert.ToDateTime("8/26/14 6:00 PM"), Price = 40, IsAvailable = true, DanceStyles=new List<DanceStyle> { styles[2] }, Time=new TimeSpan(21,0,0), Duration=ApplicationUtility.CalculateTime(0, 4, 0), Recurring=false },
                new Party() { Name = "Joe's Birthday", Description = "Joe's Birthday", Place = places[11], StartDate = Convert.ToDateTime("9/26/14 2:00 PM"), EndDate = Convert.ToDateTime("9/26/14 6:00 PM"), Price = 10, IsAvailable = true, DanceStyles=latinstyles, Time=new TimeSpan(20,0,0), Duration=ApplicationUtility.CalculateTime(0, 4, 0), Recurring=false },
                
                new Class() { Name = "Beginners salsa", Description = "Learn salsa with a step by step, fun and social salsa lessons.", DanceStyles=new List<DanceStyle> { styles[0] } , Place = places[6], StartDate = DateTime.Today.AddDays(4), EndDate = DateTime.Today.AddDays(4), Price = 10, IsAvailable = true, Time=new TimeSpan(20,0,0), Duration=ApplicationUtility.CalculateTime(0, 4, 0), Recurring=false },
                new Class() { Name = "Enjoy salsa", Description = "Enjoy salsa with a step by step, fun and social salsa lessons that will have you smiling.", DanceStyles=new List<DanceStyle> { styles[1] } , Place = places[7], StartDate = DateTime.Today.AddDays(12), EndDate = DateTime.Today.AddDays(12), Price = 15, IsAvailable = true, Time=new TimeSpan(20,0,0), Duration=ApplicationUtility.CalculateTime(0, 4, 0), Recurring=false },
                new Class() { Name = "Salsa workout", Description = "Get a workout with this fun and social salsa lessons that has everyone sweating.", DanceStyles=new List<DanceStyle> { styles[1] } , Place = places[7], StartDate = DateTime.Today.AddDays(13), EndDate = DateTime.Today.AddDays(13), Price = 20, IsAvailable = true, Time=new TimeSpan(20,0,0), Duration=ApplicationUtility.CalculateTime(0, 4, 0), Recurring=false },
                new Class() { Name = "Salsa bootcamp", Description = "Salsa bootcamp is a step by step, fun and social salsa lessons that get everyone dancing.", DanceStyles=new List<DanceStyle> { styles[1] } , Place = places[7], StartDate = DateTime.Today.AddDays(17), EndDate = DateTime.Today.AddDays(17), Price = 25, IsAvailable = true, Time=new TimeSpan(20,0,0), Duration=ApplicationUtility.CalculateTime(0, 4, 0), Recurring=false },
                new Class() { Name = "Learn Bachata", Description = "Learn the bachata with experienced instructors and great music.", DanceStyles=new List<DanceStyle> { styles[2] } , Place = places[10], StartDate = DateTime.Today.AddDays(19), EndDate = DateTime.Today.AddDays(19), Price = 5, IsAvailable = true, Time=new TimeSpan(20,0,0), Duration=ApplicationUtility.CalculateTime(0, 4, 0), Recurring=false },
                new Class() { Name = "Cha cha cha with us", Description = "Get out here and cha cha cha with an exciting group of dancers.", DanceStyles=new List<DanceStyle> { styles[3] } , Place = places[11], StartDate = DateTime.Today.AddDays(25), EndDate = DateTime.Today.AddDays(25), Price = 5, IsAvailable = true, Time=new TimeSpan(20,0,0), Duration=ApplicationUtility.CalculateTime(0, 4, 0), Recurring=false },
                new Class() { Name = "Learn to tango", Description = "No better time them now to learn to tango.", DanceStyles=new List<DanceStyle> { styles[4] } , Place = places[10], StartDate = DateTime.Today.AddDays(31), EndDate = DateTime.Today.AddDays(31), Price = 10, IsAvailable = true, Time=new TimeSpan(20,0,0), Duration=ApplicationUtility.CalculateTime(0, 4, 0), Recurring=false },
                new Class() { Name = "Master the mambo", Description = "Great mambo class for intermediate to advanced mambo'rs.", DanceStyles=new List<DanceStyle> { styles[5] } , Place = places[11], StartDate = DateTime.Today.AddDays(45), EndDate = DateTime.Today.AddDays(45), Price = 10, IsAvailable = true, Time=new TimeSpan(20,0,0), Duration=ApplicationUtility.CalculateTime(0, 4, 0), Recurring=false }
            };

            var salsa3rdStClassSeries = new ClassSeries() { Name = "Advanced Salsa Class", Description = "Learn advanced Salsa techniques.  Learn better footwork, spins, styling.  This is a very challenging advanced class, so prerequisite is to have a firm understanding of beginnner and intermediate Salsa techniques and footwork.  For more information, ask the instructor.", DanceStyles = new List<DanceStyle> { styles[0] }, Place = places[6], StartDate = Convert.ToDateTime("7/28/14 8:00pm"), EndDate = DateTime.Today.AddYears(1), Price = 15, IsAvailable = true, Prerequisite = "Beginner and Intermediate Salsa", SkillLevel = 4, Teachers = new List<Teacher> { tchr }, Frequency = Frequency.Weekly, Day = DayOfWeek.Monday, Time = new TimeSpan(20, 00, 00), EndTime = new TimeSpan(21, 00, 00) };
            var salsaIxSalClassSeries = new ClassSeries() { Name = "Learn to salsa", Description = "Learn the basics of Salsa dancing.  Meet fun people in the process.  Stick around for social dancing to try out your new moves!", DanceStyles = new List<DanceStyle> { styles[0] }, Place = places[14], StartDate = Convert.ToDateTime("7/29/14 8:00pm"), EndDate = DateTime.Today.AddYears(1), Price = 10, IsAvailable = true, Prerequisite = "None", SkillLevel = 1, Teachers = new List<Teacher> { tchr2 }, Frequency = Frequency.Weekly, Day = DayOfWeek.Tuesday, Time = new TimeSpan(20, 00, 00), EndTime = new TimeSpan(21, 00, 00) };
            var salsaGraBacClassSeries = new ClassSeries() { Name = "Bachata Class", Description = "Learn to dance Bachata.  Learn footwork, partnering, and styling techniques.  And stick around for social afterward.", DanceStyles = new List<DanceStyle> { styles[2] }, Place = places[13], StartDate = Convert.ToDateTime("7/29/14 9:00pm"), EndDate = DateTime.Today.AddYears(1), Price = 15, IsAvailable = true, Prerequisite = "None", SkillLevel = 2, Teachers = new List<Teacher> { tchr, tchr2 }, Frequency = Frequency.Weekly, Day = DayOfWeek.Tuesday, Time = new TimeSpan(21, 00, 00), EndTime = new TimeSpan(22, 00, 00) };
            var salsaDDBacClassSeries = new ClassSeries() { Name = "Advanced Bachata", Description = "Learn advanced bachata techniques, styling, and patterns.  Must have solid understanding of basic and intermediate bachata skills.", DanceStyles = new List<DanceStyle> { styles[2] }, Place = places[12], StartDate = Convert.ToDateTime("7/30/14 7:00pm"), EndDate = DateTime.Today.AddYears(1), Price = 15, IsAvailable = true, Prerequisite = "Beginner and Intermediate Bachata", SkillLevel = 3, Teachers = new List<Teacher> { tchr2 }, Frequency = Frequency.Weekly, Day = DayOfWeek.Wednesday, Time = new TimeSpan(19, 00, 00), EndTime = new TimeSpan(20, 00, 00) };

            var salsaclasses = new List<Class>();

            salsaclasses.Add(new Class() { Name = "Advanced Salsa Class", Description = "Learn advanced Salsa techniques.  Learn better footwork, spins, styling.  This is a very challenging advanced class, so prerequisite is to have a firm understanding of beginnner and intermediate Salsa techniques and footwork.  For more information, ask the instructor.", DanceStyles = new List<DanceStyle> { styles[0] }, Place = places[6], StartDate = Convert.ToDateTime("7/28/14 8:00pm"), EndDate = null, Price = 15, IsAvailable = true, Prerequisite = "Beginner and Intermediate Salsa", SkillLevel = 4, Teachers = new List<Teacher> { tchr }, Time = new TimeSpan(20, 00, 00), Duration = ApplicationUtility.CalculateTime(0, 1, 0), Recurring = true, Frequency = Frequency.Weekly, Day = DayOfWeek.Monday, Interval = 1 });
            salsaclasses.Add(new Class() { Name = "Learn to salsa", Description = "Learn the basics of Salsa dancing.  Meet fun people in the process.  Stick around for social dancing to try out your new moves!", DanceStyles = new List<DanceStyle> { styles[0] }, Place = places[14], StartDate = Convert.ToDateTime("7/29/14 8:00pm"), EndDate = null, Price = 10, IsAvailable = true, Prerequisite = "None", SkillLevel = 1, Teachers = new List<Teacher> { tchr2 }, Time = new TimeSpan(20, 00, 00), Duration = ApplicationUtility.CalculateTime(0, 1, 0), Recurring = true, Frequency = Frequency.Weekly, Day = DayOfWeek.Tuesday, Interval = 1 });
            salsaclasses.Add(new Class() { Name = "Bachata Class", Description = "Learn to dance Bachata.  Learn footwork, partnering, and styling techniques.  And stick around for social afterward.", DanceStyles = new List<DanceStyle> { styles[2] }, Place = places[13], StartDate = Convert.ToDateTime("7/29/14 9:00pm"), EndDate = null, Price = 15, IsAvailable = true, Prerequisite = "None", SkillLevel = 2, Teachers = new List<Teacher> { tchr, tchr2 }, Time = new TimeSpan(21, 00, 00), Duration = ApplicationUtility.CalculateTime(0, 1, 0), Recurring = true, Frequency = Frequency.Weekly, Day = DayOfWeek.Tuesday, Interval = 1 });
            salsaclasses.Add(new Class() { Name = "Advanced Bachata", Description = "Learn advanced bachata techniques, styling, and patterns.  Must have solid understanding of basic and intermediate bachata skills.", DanceStyles = new List<DanceStyle> { styles[2] }, Place = places[12], StartDate = Convert.ToDateTime("7/30/14 7:00pm"), EndDate = null, Price = 15, IsAvailable = true, Prerequisite = "Beginner and Intermediate Bachata", SkillLevel = 3, Teachers = new List<Teacher> { tchr2 }, Time = new TimeSpan(19, 00, 00), Duration = ApplicationUtility.CalculateTime(0, 1, 0), Recurring = true, Frequency = Frequency.Weekly, Day = DayOfWeek.Wednesday, Interval = 1 });

            var socials = new List<Social>();
            socials.Add(new Social() { Name = "Wednesday Salsa Social", Description = "Monsoon Social", Place = places[1], StartDate = Convert.ToDateTime("7/23/14 8:00pm"), EndDate = null, Price = 0, IsAvailable = true, DanceStyles = latinstyles, Time = new TimeSpan(21, 00, 00), Duration = ApplicationUtility.CalculateTime(0, 5, 0), Recurring = true, Frequency = Frequency.Weekly, Day = DayOfWeek.Wednesday, Interval = 1 });
            socials.Add(new Social() { Name = "Monday Zanzibar Salsa Social", Description = "Zanzibar Social", Place = places[10], StartDate = Convert.ToDateTime("7/1/14 8:00pm"), EndDate = null, Price = 0, IsAvailable = true, DanceStyles = latinstyles, Time = new TimeSpan(21, 00, 00), Duration = ApplicationUtility.CalculateTime(0, 5, 0), Recurring = true, Frequency = Frequency.Weekly, Day = DayOfWeek.Monday, Interval = 1 });
            context.Events.AddRange(socials);

            // Seed groups
            var groups = new List<Group>()
            {
                new Team() { GroupName = "Christian Team 1 Salsa", GroupDescription = "Level 1 Salsa Performance Team", SkillLevel = 2, TeamManagerName = "Fred Smith", Public = true, FacebookLink = "www.facebook.com/profiles/team1salsa" },
                new Team() { GroupName = "Christian Advanced Bachata", GroupDescription = "Advanced Bachata Performance Team", SkillLevel = 3, TeamManagerName = "Jerry Jones", Public = true, FacebookLink = "www.facebook.com/profiles/bachata3team" },
                new School() { GroupName = "Bachata Caliente", GroupDescription = "Bachata School", SkillLevel = 0, Public = true, FacebookLink = "www.facebook.com/profiles/bachatacaliente" }
            };

            //  Seed Reviews
            var reviews = new List<Review>()
            {
                new Review() { Author=user, ReviewText="This class is great.  I got a lot out of it.", Rating=4, ReviewDate=DateTime.Now, Event = salsaclasses[0] },
                new Review() { Author=prom, ReviewText="Friendly teacher, challenging class, good students.  BYOP.", Rating=3, ReviewDate=DateTime.Now.AddDays(-8), Event = salsaclasses[0] },
                new Review() { Author=user, ReviewText="This class was average.  I didn't learn much.", Rating=2, ReviewDate=DateTime.Now.AddDays(-2), Event = salsaclasses[1] },
                new Review() { Author=prom, ReviewText="Definitely BYOP.  The students aren't that advanced yet.", Rating=3, ReviewDate=DateTime.Now.AddDays(-5), Event = salsaclasses[1] },
                new Review() { Author=user, ReviewText="I'm a beginner dancer and this class was really hard", Rating=4, ReviewDate=DateTime.Now.AddDays(-3), Event = salsaclasses[2] },
                new Review() { Author=prom, ReviewText="Class was too hard for me.", Rating=2, ReviewDate=DateTime.Now.AddDays(-9), Event = salsaclasses[2] },
                new Review() { Author=user, ReviewText="This class was really hard.  #BYOP.", Rating=3, ReviewDate=DateTime.Now.AddDays(-11), Event = salsaclasses[3] },
                new Review() { Author=prom, ReviewText="Definitely BYOP.  Usually a small class.", Rating=4, ReviewDate=DateTime.Now.AddDays(-13), Event = salsaclasses[3] }
            };
            context.Reviews.AddRange(reviews);

            context.Places.AddRange(places);
            context.Events.AddRange(salsaclasses);
            context.Events.AddRange(events);
            context.Groups.AddRange(groups);
            context.SaveChanges();
        }
    }
}
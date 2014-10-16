using System;
using System.Linq;
using System.Web.Mvc;
using DayPilot.Web.Mvc;
using DayPilot.Web.Mvc.Enums;
using DayPilot.Web.Mvc.Events.Month;
using EDR.Data;
using EDR.Models;

namespace DayPilotCalendarMvc.Controllers
{
    public class DayPilotController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Backend()
        {
            return new Dpm().CallBack(this);
        }

        class Dpm : DayPilotMonth
        {
            ApplicationDbContext db = new ApplicationDbContext();

            protected override void OnInit(InitArgs e)
            {
                var db = new ApplicationDbContext();
                Events = from ev in db.Events select ev;

                DataIdField = "id";
                DataTextField = "Name";
                DataStartField = "StartDate";
                DataEndField = "EndDate"; 
                
                Update(CallBackUpdateType.Full);
            }

            protected override void OnEventResize(EventResizeArgs e)
            {
                var toBeResized = (from ev in db.Events where ev.Id == Convert.ToInt32(e.Id) select ev).First();
                toBeResized.StartDate = e.NewStart;
                toBeResized.EndDate = e.NewEnd;
                db.SaveChanges();
                Update();
            }

            protected override void OnEventMove(EventMoveArgs e)
            {
                var toBeResized = (from ev in db.Events where ev.Id == Convert.ToInt32(e.Id) select ev).First();
                toBeResized.StartDate = e.NewStart;
                toBeResized.EndDate = e.NewEnd;
                db.SaveChanges();
                Update();
            }

            protected override void OnTimeRangeSelected(TimeRangeSelectedArgs e)
            {
                var toBeCreated = new Event { StartDate = e.Start, EndDate = e.End, Name = (string)e.Data["name"] };
                db.Events.Add(toBeCreated);
                db.SaveChanges();
                Update();
            }

            protected override void OnFinish()
            {
                if (UpdateType == CallBackUpdateType.None)
                {
                    return;
                }

                Events = from ev in db.Events select ev;

                DataIdField = "Id";
                DataTextField = "Name";
                DataStartField = "StartDate";
                DataEndField = "EndDate";
            }

        }

    }
}

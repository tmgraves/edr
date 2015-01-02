using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DHTMLX.Common;
using DHTMLX.Scheduler;
using DHTMLX.Scheduler.Controls;
using DHTMLX.Scheduler.Data;
using EDR.Models;

namespace EDR.Controllers
{
    public class SchedulerController : BaseController
    {
        public struct SchedulerEvent
        {
            public int id;
            public string text;
            public DateTime start_date;
            public DateTime end_date;
            public double lat;
            public double lng;
            public string event_location;
        }

        // GET: Scheduler
        public ActionResult GoogleMap()
        {
            var scheduler = new DHXScheduler(this) { Skin = DHXScheduler.Skins.Terrace };
            scheduler.Config.xml_date = "%m %d, %Y %H:%i";
            scheduler.Views.Clear();
            scheduler.Views.Add(new WeekView());
            scheduler.Views.Add(new MapView());
            scheduler.InitialView = (new MapView()).Name;
            scheduler.LoadData = true;
            scheduler.DataAction = "MapEvents";
            return View(scheduler);
        }

        public ContentResult MapEvents()
        {
            var today = DateTime.Today;

            var events = DataContext.Events.ToList();
            var lstEvents = new List<object>();

            foreach(Event e in events)
            {
                lstEvents.Add(new { id = e.Id, text = e.Name, start_date = e.NextDate, end_date = e.EndDateTime, lat = e.Place.Latitude, lng = e.Place.Longitude, event_location = e.Place.Address + ", " + e.Place.City + ", " + e.Place.State + " " + e.Place.Zip });
            }

            var data = new SchedulerAjaxData(lstEvents);

            var data2 = new SchedulerAjaxData(new List<object>() {
                new {id=2, text="Kurtzenhouse", start_date=today.AddDays(1).AddHours(13), end_date=today.AddDays(1).AddHours(16), lat=48.7396839, lng=7.813368099999934, event_location="D37, 67240 Kurtzenhouse, France"},
                new {id=3, text="Forêt Domaniale", start_date=today.AddDays(2).AddHours(10), end_date=today.AddDays(2).AddHours(12), lat=48.767333, lng=5.793258000000037, event_location="Forêt Domaniale de la Reine, Véry, 54200 Royaumeix, France"},         
                new {id=4, text="Windstein", start_date=today.AddDays(3).AddHours(7), end_date=today.AddDays(3).AddHours(8), lat=49.0003477, lng=7.687306499999977, event_location="1 Rue du Nagelsthal, 67110 Windstein, France"}
                });

            return data;
        }
    }
}
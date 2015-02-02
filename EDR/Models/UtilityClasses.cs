using EDR.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EDR.Models
{
    public class UtilityClasses
    {
    }

    public class Media
    {
        public int Id { get; set; }
        public MediaType MediaType { get; set; }
        private string _Title;
        public string Title
        {
            get
            {
                return _Title == null ? "No Title" : _Title;
            }
            set
            {
                _Title = value;
            }
        }
        public DateTime MediaDate { get; set; }
        public string MediaUrl { get; set; }
        public string PhotoUrl { get; set; }
        public ApplicationUser Author { get; set; }
        public string Text { get; set; }
        public MediaSource MediaSource { get; set; }
    }

    public class EventMedia : Media
    {
        public Event Event { get; set; }
    }

    public class PlaceMedia : Media
    {
        public Place Place { get; set; }
    }
}
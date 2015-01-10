using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EDR.Enums;

namespace EDR.Models.ViewModels
{
    public class EventReviewViewModel
    {
        public int EventId { get; set; }
        public Review Review { get; set; }
    }

    public class EventDetailViewModel
    {
        public Event Event { get; set; }
        public EventType EventType { get; set; }
        public IEnumerable<Teacher> Teachers { get; set; }
    }

    public class EventCreateViewModel
    {
        public Event Event { get; set; }
        [DataType(DataType.Time)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:H:mm tt}")]
        public DateTime Time { get; set; }
        public string EventType { get; set; }
        public string Role { get; set; }
        [Display(Name = "Your Skill Level")]
        public SkillLevel SkillLevel { get; set; }
        [Display(Name = "Prerequisite(s)")]
        public string Prerequisite { get; set; }
        public List<SelectListItem> PlaceList { get; set; }
        public int PlaceId { get; set; }
        [Display(Name = "Name of the Location")]
        public string Name { get; set; }
        public string Address { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public State State { get; set; }
        public string Zip { get; set; }
        public IEnumerable<DanceStyleListItem> AvailableStyles { get; set; }
        public IEnumerable<DanceStyleListItem> SelectedStyles { get; set; }
        public PostedStyles PostedStyles { get; set; }
    }

    public class EventNewViewModel
    {
        public IEnumerable<DanceStyleListItem> AvailableStyles { get; set; }
        public IEnumerable<FacebookEvent> FacebookEvents { get; set; }
    }

    public class ClassNewViewModel : EventNewViewModel
    {

    }

    public class EventViewModel
    {
        public Event Event { get; set; }
        public Class Class { get; set; }
        public Social Social { get; set; }
    }
}
﻿using System;
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

    public class EventEditViewModel
    {
        public Event Event { get; set; }
        public List<SelectListItem> Places { get; set; }
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

    public class EventBaseViewModel
    {
        public Event Event { get; set; }
    }

    public class EventViewModel : EventBaseViewModel
    {
        public Class Class { get; set; }
        public Social Social { get; set; }
        public Review Review { get; set; }
        public IEnumerable<ClassTeacherInvitation> ClassTeacherInvitations { get; set; }
    }

    public class EventReviewsViewModel
    {
        public int EventId { get; set; }
        public IEnumerable<Review> EventReviews { get; set; }
        public Review NewReview { get; set; }
    }

    public class EventChangePictureViewModel : EventBaseViewModel
    {
        public IEnumerable<FacebookPhoto> FacebookPictures { get; set; }
    }

    public class EventPostPictureViewModel : EventBaseViewModel
    {
        public IEnumerable<FacebookPhoto> FacebookPictures { get; set; }
    }

    public class EventPostVideoViewModel : EventBaseViewModel
    {
        public IEnumerable<YouTubeVideo> YoutubeVideos { get; set; }
    }

    public class EventListViewModel
    {
        public Address Location { get; set; }
        public EventType EventType { get; set; }
        public IEnumerable<Event> Events { get; set; }
    }
}
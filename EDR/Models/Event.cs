using EDR.Enums;
using EDR.Utilities;
using Microsoft.Linq.Translations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EDR.Models
{
    public class Event : Entity
    {
        //  Default Constructor
        public Event()
        {
            Interval = 1;
            Frequency = Enums.Frequency.Weekly;
            UpdatedDate = DateTime.Now;
            CheckedDate = DateTime.Now;
        }

        [Required]
        public string Name { get; set; }
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }
        [Display(Name = "Facebook Page")]
        [RegularExpression("http[s]?://(www.facebook.com)/?[a-zA-Z0-9/\\-\\.]*", ErrorMessage = "Please enter a valid facebook page.")]
        public string FacebookLink { get; set; }
        public string FacebookId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime StartDate { get; set; }

        [Required]
        [Display(Name = "Is this an All Day event?")]
        public bool AllDay { get; set; }

        [DataType(DataType.Time)]
        [Display(Name = "Start Time")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:HH:mm}")]
        public DateTime? StartTime { get; set; }

        [DataType(DataType.Time)]
        [Display(Name = "End Time")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:HH:mm}")]
        public DateTime? EndTime { get; set; }

        [Display(Name = "Duration")]
        public int Duration { get; set; }
        
        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? EndDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Updated Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? UpdatedDate { get; set; }
        [DataType(DataType.Date)]
        [Display(Name = "Checked Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? CheckedDate { get; set; }

        [DataType(DataType.Currency)]
        public Nullable<decimal> Price { get; set; }

        public bool IsAvailable { get; set; }

        [Required]
        [Display(Name = "Does This Event Repeat?")]
        public bool Recurring { get; set; }
        [DefaultValue(1)]
        public Frequency Frequency { get; set; }
        [DefaultValue(1)]
        [Range(1, 100)]
        public int Interval { get; set; }
        public DayOfWeek Day 
        {
            get
            {
                return StartDate.DayOfWeek;
            }
        }
        public string MonthDays { get; set; }

        public string PhotoUrl { get; set; }
        public string VideoUrl { get; set; }

        [Index("IX_Events_ChildEvents")]
        public int? ParentEventId { get; set; }
        [ForeignKey("ParentEventId")]
        public virtual Event ParentEvent { get; set; }

        [Display(Name = "Next Date")]
        public DateTime NextDate
        {
            get
            {
                if (Recurring)
                {
                    return ApplicationUtility.GetNextDate(StartDate, Frequency, (int)Interval, Day, null, MonthDays);
                }
                else
                {
                    return StartDate;
                }
            }
        }

        [Display(Name = "End Date/Time")]
        public DateTime EndDateTime
        {
            get
            {
                return NextDate.AddMinutes(Duration);
            }
        }

        public virtual Place Place { get; set; }
        public ApplicationUser Creator { get; set; }
        public ICollection<EventMember> EventMembers { get; set; }
        public ICollection<LinkedFacebookObject> LinkedFacebookObjects { get; set; }

        public ICollection<DanceStyle> DanceStyles { get; set; }

        public virtual Series Series { get; set; }

        public ICollection<Review> Reviews { get; set; }

        public ICollection<ApplicationUser> Users { get; set; }

        public ICollection<EventPicture> Pictures { get; set; }
        public ICollection<EventVideo> Videos { get; set; }
        public ICollection<EventPlaylist> Playlists { get; set; }
        public ICollection<EventAlbum> Albums { get; set; }
        public ICollection<EventAttendee> Attendees { get; set; }
        public ICollection<Feed> Feeds { get; set; }
        public ICollection<Event> ChildEvents { get; set; }
    }

    public class LinkedFacebookObject : Entity
    {
        public string FacebookId { get; set; }
        public MediaSource MediaSource { get; set; }
        public string Url { get; set; }
        public string Name { get; set; }
        [Required]
        public virtual Event Event { get; set; }
        [Required]
        public FacebookObjectType ObjectType { get; set; }
        //public ICollection<ObjectFeed> Feeds { get; set; }
    }

    public enum FacebookObjectType
    {
        Group,
        Event,
        Page
    }

    public class EventAttendee : Entity
    {
        public virtual Event Event { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
    }

    public class Feed : Entity
    {
        [Required]
        public virtual Event Event { get; set; }
        public DateTime UpdateTime { get; set; }
        public string Message { get; set; }
        public string Link { get; set; }
    }

    //public class ObjectFeed : Entity
    //{
    //    [Required]
    //    public virtual LinkedFacebookObject Object { get; set; }
    //    public DateTime UpdateTime { get; set; }
    //    public string Message { get; set; }
    //    public string Link { get; set; }
    //}
}
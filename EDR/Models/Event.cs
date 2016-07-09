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
using System.Web.Mvc;
using Foolproof;

namespace EDR.Models
{
    //public class Test
    //{
    //    [Required]
    //    public string Name { get; set; }

    //    [Required]
    //    public DateTime Start { get; set; }

    //    [Required]
    //    [GreaterThan("Start")]
    //    public DateTime End { get; set; }
    //}
    public class Event : Entity
    {
        //  Default Constructor
        public Event()
        {
            Interval = 1;
            Frequency = Enums.Frequency.Weekly;
            UpdatedDate = DateTime.Now;
            CheckedDate = DateTime.Now;
            //EventInstances = new List<EventInstance>();
            //Tickets = new List<Ticket>();
            //Place = new Place();
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
        [DisplayFormat(DataFormatString = "{0:MM-dd-yyyy}")]
        public DateTime StartDate { get; set; }

        [Required]
        [Display(Name = "Is this an All Day event?")]
        public bool AllDay { get; set; }

        [DataType(DataType.Time)]
        [Display(Name = "Start Time")]
        [DisplayFormat(DataFormatString = "{0:hh:mm tt}", ApplyFormatInEditMode = false)]
        public DateTime? StartTime { get; set; }

        [DataType(DataType.Time)]
        [Display(Name = "End Time")]
        [DisplayFormat(DataFormatString = "{0:hh:mm tt}", ApplyFormatInEditMode = false)]
        [GreaterThan("StartTime")]
        public DateTime? EndTime { get; set; }

        [Display(Name = "Duration")]
        public int Duration { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        [DisplayFormat(DataFormatString = "{0:MM-dd-yyyy}")]
        [GreaterThanOrEqualTo("StartDate")]
        public DateTime? EndDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Updated Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? UpdatedDate { get; set; }
        [DataType(DataType.Date)]
        [Display(Name = "Checked Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? CheckedDate { get; set; }

        [DisplayFormat(DataFormatString = "{0:C}")]
        [DataType(DataType.Currency)]
        public Nullable<decimal> Price { get; set; }
        public bool Free { get; set; }

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

        //[Index("IX_Events_ChildEvents")]
        //public int? ParentEventId { get; set; }
        //[ForeignKey("ParentEventId")]
        //public virtual Event ParentEvent { get; set; }

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

        [Required]
        public int PlaceId { get; set; }
        [ForeignKey("PlaceId")]
        public virtual Place Place { get; set; }
        public ApplicationUser Creator { get; set; }
        public ICollection<EventMember> EventMembers { get; set; }
        public ICollection<LinkedFacebookObject> LinkedFacebookObjects { get; set; }
        public ICollection<LinkedMedia> LinkedMedia { get; set; }

        public virtual ICollection<DanceStyle> DanceStyles { get; set; }

        public virtual Series Series { get; set; }

        public ICollection<Review> Reviews { get; set; }

        public ICollection<ApplicationUser> Users { get; set; }

        public ICollection<EventPicture> Pictures { get; set; }
        public ICollection<EventVideo> Videos { get; set; }
        public ICollection<EventPlaylist> Playlists { get; set; }
        public ICollection<EventAlbum> Albums { get; set; }
        public ICollection<EventAttendee> Attendees { get; set; }
        public ICollection<Feed> Feeds { get; set; }
        //public ICollection<Event> ChildEvents { get; set; }
        //  public ICollection<EventTicket> EventTickets { get; set; }
        public ICollection<EventInstance> EventInstances { get; set; }
        public virtual ICollection<Ticket> Tickets { get; set; }
        public ICollection<Performance> Performances { get; set; }
    }

    public class EventInstance : Entity
    {
        public int? EventId { get; set; }
        [ForeignKey("EventId")]
        public virtual Event Event { get; set; }
        [DisplayFormat(DataFormatString = "{0:MM-dd-yyyy}")]
        public DateTime DateTime { get; set; }
        [DisplayFormat(DataFormatString = "{0:MM-dd-yyyy}")]
        [GreaterThanOrEqualTo("DateTime")]
        public DateTime EndDate { get; set; }
        [DataType(DataType.Time)]
        [Display(Name = "Start Time")]
        [DisplayFormat(DataFormatString = "{0:hh:mm tt}", ApplyFormatInEditMode = false)]
        public DateTime? StartTime { get; set; }
        [DataType(DataType.Time)]
        [Display(Name = "End Time")]
        [GreaterThanOrEqualTo("StartTime")]
        [DisplayFormat(DataFormatString = "{0:hh:mm tt}", ApplyFormatInEditMode = false)]
        public DateTime? EndTime { get; set; }
        public ICollection<EventRegistration> EventRegistrations { get; set; }
        public int? PlaceId { get; set; }
        [ForeignKey("PlaceId")]
        public virtual Place Place { get; set; }
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

    public class LinkedMedia : Entity
    {
        public MediaSource MediaSource { get; set; }
        public string FacebookId { get; set; }
        public string Url { get; set; }
        public string Name { get; set; }
        public Event Event { get; set; }
        [Required]
        public string ObjectType { get; set; }
        //public ICollection<ObjectFeed> Feeds { get; set; }
        private bool _default = false;
        public bool Default
        {
            get { return _default; }
            set { _default = value; }
        }
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
        public Event Event { get; set; }
        public School School { get; set; }
        public Place Place { get; set; }
        public Teacher Teacher { get; set; }
        public Promoter Promoter { get; set; }
        public DateTime UpdateTime { get; set; }
        public string Message { get; set; }
        public string Link { get; set; }
        public MediaType Type { get; set; }
        public string PhotoUrl { get; set; }
    }

    public class EventRegistration : Entity
    {
        [Required]
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
        [Required]
        public int EventInstanceId { get; set; }
        [ForeignKey("EventInstanceId")]
        public EventInstance Instance { get; set; }
        private DateTime _date = DateTime.Now;
        public DateTime DateRegistered
        {
            get { return _date; }
            set { _date = value; }
        }
        public int? UserTicketId { get; set; }
        [ForeignKey("UserTicketId")]
        public UserTicket UserTicket { get; set; }
        public DateTime? Checkedin { get; set; }
    }

    //public class EventTicket : Entity
    //{
    //    [Required]
    //    public int EventId { get; set; }
    //    [ForeignKey("EventId")]
    //    public Event Event { get; set; }
    //    [Required]
    //    public int TicketId { get; set; }
    //    [ForeignKey("TicketId")]
    //    public Ticket Ticket { get; set; }
    //}

    public class Ticket : Entity
    {
        [Required]
        [DisplayFormat(DataFormatString = "{0:G0}")]
        public decimal Quantity { get; set; }
        [Required]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal Price { get; set; }
        public int? SchoolId { get; set; }
        [ForeignKey("SchoolId")]
        public virtual School School { get; set; }
        public int? EventId { get; set; }
        [ForeignKey("EventId")]
        public virtual Event Event { get; set; }
        //public ICollection<EventTicket> EventTickets { get; set; }
        [Display(Name = "Purchase Limit")]
        public int? Limit { get; set; }
        [Display(Name = "Valid From")]
        public DateTime? Start { get; set; }
        [Display(Name = "Valid To")]
        public DateTime? End { get; set; }
        public ICollection<UserTicket> UserTickets { get; set; }
        public ICollection<OrderDetail> OrderDetails { get; set; }
    }

    //public class EventTicketPlaceholder
    //{
    //    public bool Connect { get; set; }
    //    public EventTicket EventTicket { get; set; }
    //}

    //public class ObjectFeed : Entity
    //{
    //    [Required]
    //    public virtual LinkedFacebookObject Object { get; set; }
    //    public DateTime UpdateTime { get; set; }
    //    public string Message { get; set; }
    //    public string Link { get; set; }
    //}
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EDR.Enums;
using EDR.Utilities.Validators;
using System.ComponentModel;
using Foolproof;

namespace EDR.Models.ViewModels
{
    public class EventReviewViewModel
    {
        public int EventId { get; set; }
        public Review Review { get; set; }
    }

    public class EventAttendeesViewModel
    {
        public Event Event { get; set; }

        [Required]
        [Display(Name = "Your Name")]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Your Email Address")]
        public string Email { get; set; }

        [DataType(DataType.EmailAddress)]
        [Display(Name = "Confirm Your Email")]
        [System.ComponentModel.DataAnnotations.Compare("Email", ErrorMessage = "Email Addresses do not match.")]
        public string ConfirmEmail{ get; set; }

        public string Phone { get; set; }
    }

    public class EventDetailViewModel
    {
        public Event Event { get; set; }
        public EventType EventType { get; set; }
        public IEnumerable<Teacher> Teachers { get; set; }
    }

    public class EventCreateViewModel : EventBaseViewModel
    {
        public int? SchoolId { get; set; }
        public RoleName Role { get; set; }
        public ClassType ClassType { get; set; }
        public SocialType SocialType { get; set; }
        public Place NewPlace { get; set; }
        public List<PlaceItem> Places { get; set; }
        public MultiCheckBox MonthDays { get; set; }
        public string MonthDay { get; set; }
        public string HiddenMonthDay { get; set; }
        public MultiCheckBox StylesCheckboxList { get; set; }
        public IEnumerable<FacebookEvent> FacebookEvents { get; set; }
        public string FacebookId { get; set; }
        public string FacebookEventName { get; set; }
        public string SelectedFacebookEventId { get; set; }
        [Display(Name = "Enter Facebook Url")]
        public string FacebookLink { get; set; }
        [Display(Name = "Skill Level")]
        [DefaultValue(1)]
        [Range(1, 100)]
        public int SkillLevel { get; set; }
        public bool AddtoMyPlaces { get; set; }
        public string CreateAction { get; set; }
        public bool UseSchoolTickets { get; set; }
        //  [RequiredIf("UseSchoolTickets", Operator.EqualTo, false)]
        //  public Ticket EventTicket { get; set; }
        //  public List<Ticket> Tickets { get; set; }
        //  public string[] TicketId { get; set; }
        [RequiredIf("UseSchoolTickets", Operator.EqualTo, "false", ErrorMessage = "Enter a Ticket Quantity")]
        [DisplayFormat(DataFormatString = "{0:G0}")]
        [Display(Name = "Quantity")]
        [DefaultValue(1)]
        [Range(1, 1000)]
        public int? TicketQuantity { get; set; }
        [RequiredIf("UseSchoolTickets", Operator.EqualTo, "false", ErrorMessage = "Enter a Ticket Price")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        [Display(Name = "Price")]
        public decimal? TicketPrice { get; set; }

        //[Range(1, 12)]
        //public int StartHour { get; set; }
        //[Range(0, 59)]
        //public int StartMinute { get; set; }
        //public string StartAMPM { get; set; }

        //[Range(1, 12)]
        //public int EndHour { get; set; }
        //[Range(0, 59)]
        //public int EndMinute { get; set; }
        //public string EndAMPM { get; set; }

        public EventCreateViewModel()
        {
            Event = new Event();
            Places = new List<PlaceItem>();
            MonthDays = new MultiCheckBox();
            StylesCheckboxList = new MultiCheckBox();
            UseSchoolTickets = true;
            //  Tickets = new List<Ticket>();
        }
        public EventCreateViewModel(EventType eventType, int? schoolId, RoleName role)
        {
            SchoolId = schoolId;
            EventType = eventType;
            Places = new List<PlaceItem>();
            MonthDays = new MultiCheckBox();
            StylesCheckboxList = new MultiCheckBox();
            UseSchoolTickets = true;
            //  Tickets = new List<Ticket>();

            NewPlace = new Place() { Id = 0, Latitude = 0.0, Longitude = 0.0, Public = false, PlaceType = PlaceType.OtherPlace };

            //  New Event
            if (eventType == EDR.Enums.EventType.Class)
            {
                Event = new Class() { StartDate = DateTime.Today, EndDate = DateTime.Today, StartTime = DateTime.Today.Add(new TimeSpan(20, 00, 0)), EndTime = DateTime.Today.Add(new TimeSpan(21, 00, 0)), Place = new Place(), SchoolId = (int)schoolId };
            }
            else
            {
                Event = new Social() { StartDate = DateTime.Today, EndDate = DateTime.Today, StartTime = DateTime.Today.Add(new TimeSpan(20, 00, 0)), EndTime = DateTime.Today.Add(new TimeSpan(24, 00, 0)), Place = new Place() };
            }
        }
    }

    public class MultiCheckBox
    {
        public IEnumerable<SelectListItem> AvailableItems { get; set; }
        public IEnumerable<SelectListItem> SelectedItems { get; set; }
        public string[] PostedItems { get; set; }

        public MultiCheckBox()
        {
            AvailableItems = new List<SelectListItem>();
            SelectedItems = new List<SelectListItem>();
        }
    }

    public class ImportFacebookEventViewModel
    {
        public IEnumerable<FacebookEvent> FacebookEvents { get; set; }
        [Display(Name = "Enter Facebook Url")]
        public string FacebookLink { get; set; }
        public EventType? Type { get; set; }
    }

    public class EventEditViewModel : EventBaseViewModel
    {
        public ApplicationUser User { get; set; }
        public UpdateType UpdateType { get; set; }
        public int SchoolId { get; set; }
        public int? PlaceId { get; set; }
        public List<PlaceItem> Places { get; set; }
        public RoleName Role { get; set; }
        public ClassType ClassType { get; set; }
        public SocialType SocialType { get; set; }
        public Place NewPlace { get; set; }
        public IEnumerable<DanceStyleListItem> AvailableStyles { get; set; }
        public IEnumerable<DanceStyleListItem> SelectedStyles { get; set; }
        public PostedStyles PostedStyles { get; set; }
        public List<SelectListItem> MonthDays { get; set; }
        public List<SelectListItem> SelectedMonthDays { get; set; }
        public string[] PostedMonthDays { get; set; }
        public string MonthDay { get; set; }
        public string HiddenMonthDay { get; set; }
        [Display(Name = "Save this Place")]
        public bool AddtoMyPlaces { get; set; }
        [Display(Name = "Skill Level")]
        [DefaultValue(1)]
        [Range(1, 100)]
        public int SkillLevel { get; set; }
    }

    //public class EventCreateViewModel : EventBaseViewModel
    //{
    //    public int SchoolId { get; set; }
    //    public List<PlaceItem> Places { get; set; }
    //    public RoleName Role { get; set; }
    //    public ClassType ClassType { get; set; }
    //    public SocialType SocialType { get; set; }
    //    public Place NewPlace { get; set; }
    //    public IEnumerable<DanceStyleListItem> AvailableStyles { get; set; }
    //    public IEnumerable<DanceStyleListItem> SelectedStyles { get; set; }
    //    public PostedStyles PostedStyles { get; set; }
    //    public List<SelectListItem> MonthDays { get; set; }
    //    public List<SelectListItem> SelectedMonthDays { get; set; }
    //    public string[] PostedMonthDays { get; set; }
    //    public string MonthDay { get; set; }
    //    public string HiddenMonthDay { get; set; }
    //    [Display(Name = "Save this Place")]
    //    public bool AddtoMyPlaces { get; set; }
    //    [Display(Name = "Skill Level")]
    //    [DefaultValue(1)]
    //    [Range(1, 100)]
    //    public int SkillLevel { get; set; }
    //}

    public class EventNewViewModel
    {
        public IEnumerable<DanceStyleListItem> AvailableStyles { get; set; }
        public IEnumerable<FacebookEvent> FacebookEvents { get; set; }
    }

    public class ClassNewViewModel : EventNewViewModel
    {

    }
    public class EventInstancesViewModel : EventBaseViewModel
    {

    }

    public class EventUpdates : EventBaseViewModel
    {
        public List<Media> Media { get; set; }
        public DateTime Created { get; set; }
    }

    public class EventBaseViewModel
    {
        public Event Event { get; set; }
        public string ReturnUrl { get; set; }
        public EventType? EventType { get; set; }
    }

    public class EventViewModel : EventBaseViewModel
    {
        public Class Class { get; set; }
        public Social Social { get; set; }
        public IEnumerable<ClassTeacherInvitation> ClassTeacherInvitations { get; set; }
        public IEnumerable<EventMedia> MediaUpdates { get; set; }
        public EventReviewsViewModel Reviews { get; set; }
        public IEnumerable<YouTubeVideo> YoutubeVideos { get; set; }
        public IEnumerable<FacebookVideo> FacebookVideos { get; set; }
        public IEnumerable<YouTubePlaylist> YouTubePlaylists { get; set; }
        public IEnumerable<FacebookEvent> FacebookEvents { get; set; }
        public IEnumerable<LinkedFacebookObject> LinkedFacebookObjects { get; set; }
        public RoleName Role { get; set; }
        public int AvailableTickets { get; set; }
        public IEnumerable<Ticket> Tickets { get; set; }

        public EventViewModel()
        {
            ClassTeacherInvitations = new List<ClassTeacherInvitation>();
            MediaUpdates = new List<EventMedia>();
            Reviews = new EventReviewsViewModel();
            YoutubeVideos = new List<YouTubeVideo>();
            FacebookVideos = new List<FacebookVideo>();
            YouTubePlaylists = new List<YouTubePlaylist>();
            FacebookEvents = new List<FacebookEvent>();
            LinkedFacebookObjects = new List<LinkedFacebookObject>();
            //  Tickets = new List<Ticket>();
            AvailableTickets = 0;
        }
    }

    public class EventManageViewModel : EventBaseViewModel
    {
        public int? SchoolId { get; set; }
        public Place NewPlace { get; set; }
        public int? NewStyleId { get; set; }
        public string NewYoutubePlayList { get; set; }

        public EventManageViewModel()
        {
        }
        public EventManageViewModel(Event evnt)
        {
            Event = evnt;
        }
    }

    public class EventDanceStylesPartialViewModel
    {
        public IEnumerable<DanceStyle> DanceStyles { get; set; }
        public EventType EventType { get; set; }
        public int EventId { get; set; }
    }

    public class EventInstanceManageViewModel
    {
        public EventInstance Instance { get; set; }
        public Place NewPlace { get; set; }
    }

    public class EventFacebookPictureContainer : EventBaseViewModel
    {
        public IEnumerable<FacebookPhoto> FacebookPictures { get; set; }
    }

    public class EventFacebookAlbumContainer : EventBaseViewModel
    {
        public IEnumerable<FacebookAlbum> FacebookAlbums { get; set; }
    }

    public class EventFacebookVideosContainer : EventBaseViewModel
    {
        public IEnumerable<FacebookVideo> FacebookVideos { get; set; }
    }
    public class EventYouTubeVideosContainer : EventBaseViewModel
    {
        public IEnumerable<YouTubeVideo> YouTubeVideos { get; set; }
    }
    public class EventYouTubePlaylistsContainer : EventBaseViewModel
    {
        public IEnumerable<YouTubePlaylist> YouTubePlaylists { get; set; }
    }

    public class EventInstagramPicturesContainer : EventBaseViewModel
    {
        public IEnumerable<InstagramPicture> InstagramPictures { get; set; }
    }

    public class EventVideos : EventBaseViewModel
    {
        public IEnumerable<EventVideo> Videos { get; set; }
    }

    public class EventPictures : EventBaseViewModel
    {
        public IEnumerable<EventPicture> Pictures { get; set; }
        public IEnumerable<EventAlbum> Albums { get; set; }
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

    public class EventChangeCoverViewModel : EventBaseViewModel
    {
        public IEnumerable<Media> Media { get; set; }
    }

    public class EventPostPictureViewModel : EventBaseViewModel
    {
        public IEnumerable<FacebookPhoto> FacebookPictures { get; set; }
        public EventFacebookPictureContainer FacebookPictureContainer { get; set; }
    }

    public class EventPostVideoViewModel : EventBaseViewModel
    {
        public IEnumerable<YouTubeVideo> YoutubeVideos { get; set; }
        public IEnumerable<FacebookVideo> FacebookVideos { get; set; }
        public IEnumerable<YouTubePlaylist> YouTubePlaylists { get; set; }
    }

    public class EventListViewModel : EventBaseViewModel
    {
        public Address Location { get; set; }
        public IEnumerable<Event> Events { get; set; }
    }

    public class ConfirmFacebookEvent
    {
        public Event Event { get; set; }
        public EventType? Type { get; set; }
        public ApplicationUser User { get; set; }
        public RoleName Role { get; set; }
        public ClassType? ClassType { get; set; }
        public SocialType? SocialType { get; set; }
        public IEnumerable<DanceStyleListItem> AvailableStyles { get; set; }
        public IEnumerable<DanceStyleListItem> SelectedStyles { get; set; }
        [RequiredStringArrayValue(ErrorMessage = "Select at least (1) Dance Style")]
        public PostedStyles PostedStyles { get; set; }
        public List<SelectListItem> MonthDays { get; set; }
        public List<SelectListItem> SelectedMonthDays { get; set; }
        public string[] PostedMonthDays { get; set; }
        public string MonthDay { get; set; }
        public string HiddenMonthDay { get; set; }
        [DefaultValue(1)]
        [Range(1, 100)]
        public int SkillLevel { get; set; }
    }

    public class EventLinkedFacebookEventContainer
    {
        public IEnumerable<FacebookEvent> FacebookEvents { get; set; }
        [Display(Name = "Enter Facebook Url")]
        public string FacebookLink { get; set; }
        public EventType? Type { get; set; }
        public Event Event { get; set; }
    }

    public class EventLinkedFacebookGroupContainer
    {
        public IEnumerable<FacebookGroup> FacebookGroups { get; set; }
        [Display(Name = "Enter Facebook Url")]
        public string FacebookLink { get; set; }
        public EventType? Type { get; set; }
        public Event Event { get; set; }
    }

    public class EventLinkedFacebookPageContainer
    {
        [Display(Name = "Enter Facebook Url")]
        public string FacebookLink { get; set; }
        public EventType? Type { get; set; }
        public Event Event { get; set; }
    }

    public class RelatedEvents : EventBaseViewModel
    {
        public IEnumerable<Event> Events { get; set; }
    }

    public class PlaceItem : Place
    {
        public bool Selected { get; set; }
    }

    public class EventsViewModel
    {
        public EventType EventType { get; set; }
        public Address Location { get; set; }
        public int Zoom { get; set; }
        public IEnumerable<Event> Events { get; set; }
    }

    public class AddTeacherViewModel
    {
        public Class Class { get; set; }
        public List<Teacher> Teachers { get; set; }
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        public IEnumerable<DanceStyleListItem> AvailableStyles { get; set; }
        public IEnumerable<DanceStyleListItem> SelectedStyles { get; set; }
        public PostedStyles PostedStyles { get; set; }
        [Display(Name = "Location")]
        public string Location { get; set; }
    }

    public class EventFeeViewModel : EventBaseViewModel
    {
        public int SchoolId { get; set; }
    }
}
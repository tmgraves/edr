using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EDR.Enums;
using EDR.Utilities.Validators;

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

    public class EventCreateViewModel : EventBaseViewModel
    {
        [DataType(DataType.Time)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:H:mm tt}")]
        public DateTime Time { get; set; }
        public RoleName Role { get; set; }
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
        public IEnumerable<FacebookEvent> FacebookEvents { get; set; }
        public ApplicationUser User { get; set; }
    }

    public class ImportFacebookEventViewModel : EventBaseViewModel
    {
        public IEnumerable<FacebookEvent> FacebookEvents { get; set; }
        public int? PlaceId { get; set; }
        public ApplicationUser User { get; set; }
    }

    public class EventEditViewModel : EventBaseViewModel
    {
        public ApplicationUser User { get; set; }
        public UpdateType UpdateType { get; set; }
        public int PlaceId { get; set; }
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
    }

    public class EventNewViewModel
    {
        public IEnumerable<DanceStyleListItem> AvailableStyles { get; set; }
        public IEnumerable<FacebookEvent> FacebookEvents { get; set; }
    }

    public class ClassNewViewModel : EventNewViewModel
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
    }

    public class EventLinkedFacebookEventContainer : EventBaseViewModel
    {
        public IEnumerable<FacebookEvent> FacebookEvents { get; set; }
    }

    public class EventLinkedFacebookGroupContainer : EventBaseViewModel
    {
        public IEnumerable<FacebookGroup> FacebookGroups { get; set; }
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
}
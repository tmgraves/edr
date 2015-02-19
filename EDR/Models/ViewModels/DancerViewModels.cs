using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EDR.Models.ViewModels
{
    public class DancerViewModels
    {
    }

    public class DancerViewViewModel
    {
        // TODO: FILL IN PROPERTIES NEEDED FOR VIEW

        public ApplicationUser Dancer { get; set; }
        public IEnumerable<FacebookFriend> FriendList { get; set; }
        public List<YouTubeVideo> YouTubeVideos { get; set; }
        public Address Address { get; set; }

        public EventListViewModel Classes { get; set; }
        public EventListViewModel Socials { get; set; }

        public IEnumerable<Teacher> Teachers { get; set; }
        public IEnumerable<Event> SuggestedEvents { get; set; }
        public IEnumerable<Class> SuggestedClasses { get; set; }
        public IEnumerable<Social> SuggestedSocials { get; set; }

        public IEnumerable<EventMedia> MediaUpdates { get; set; }
        public IEnumerable<SpotifyPlaylist> SpotifyPlaylists { get; set; }
    }

    public class DancerEditViewModel
    {
        // TODO: FILL IN PROPERTIES NEEDED FOR VIEW

        public ApplicationUser Dancer { get; set; }
        public IEnumerable<DanceStyleListItem> AvailableStyles { get; set; }
        public IEnumerable<DanceStyleListItem> SelectedStyles { get; set; }
        public PostedStyles PostedStyles { get; set; }
    }

    public class ChangePictureViewModel
    {
        public ApplicationUser Dancer { get; set; }
        public IEnumerable<FacebookPhoto> FacebookPictures { get; set; }
        public string ReturlUrl { get; set; }
    }
}
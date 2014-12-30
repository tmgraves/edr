using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EDR.Models.ViewModels
{
    public class DancerViewViewModel
    {
        // TODO: FILL IN PROPERTIES NEEDED FOR VIEW

        public ApplicationUser Dancer { get; set; }
        public IEnumerable<FacebookFriend> FriendList { get; set; }
        public List<YouTubeVideo> YouTubeVideos { get; set; }
        public Address Address { get; set; }
        public IEnumerable<Class> Classes { get; set; }
        public IEnumerable<Social> Socials { get; set; }
        public IEnumerable<Concert> Concerts { get; set; }
        public IEnumerable<Conference> Conferences { get; set; }
        public IEnumerable<OpenHouse> OpenHouses { get; set; }
        public IEnumerable<Party> Parties { get; set; }
        public IEnumerable<Teacher> Teachers { get; set; }
        public IEnumerable<Event> SuggestedEvents { get; set; }
    }
}
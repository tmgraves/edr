using DHTMLX.Scheduler;
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

        public EventListViewModel Classes { get; set; }
        public EventListViewModel Socials { get; set; }

        public IEnumerable<Teacher> Teachers { get; set; }
        public IEnumerable<Event> SuggestedEvents { get; set; }
        public DHXScheduler Scheduler { get; set; }
    }
}
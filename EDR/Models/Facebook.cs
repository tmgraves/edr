using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EDR.Models
{
    public class FacebookFriend
    {
        [Required]
        [Display(Name = "Friend's name")]
        public string Name { get; set; }
        public string Id { get; set; }
        public string Link { get; set; }
        public string ImageURL { get; set; }
        public string Email { get; set; }
        public ApplicationUser User { get; set; }
    }

    public class FacebookPhoto
    {
        public string Album;
        public string Id;
        public string Name;
        public string Link;
        public string Source;
        public string LargeSource;
        public DateTime PhotoDate;
    }

    public class FacebookEvent
    {
        public string Id;
        public FacebookPhoto CoverPhoto;
        public string Description;
        public DateTime? EndTime;
        public bool IsDateOnly;
        public string Location;
        public string Name;
        public string Owner;
        public string Privacy;
        public DateTime StartTime;
        public string TicketUri;
        public string Timezone;
        public DateTime Updated;
        public string EventLink;
    }
}
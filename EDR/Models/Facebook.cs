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
        public FacebookAddress Address;
    }

    public class FacebookAddress : Address
    {
        public string FacebookId;
    }

    public class FacebookVideo
    {
        public string Id;
        public DateTime Created_Time;
        //  The description of the video.
        public string Description;
        //The HTML element that may be embedded in a Web page to play the video.
        public string Embed_Html;
        //The profile that created the video.
        public string From;
        //The icon that Facebook displays when videos are published to the feed.
        public string Icon;
        //The video title or caption.
        public string Name;
        //The URL for the thumbnail picture of the video.
        public string Picture;
        //A URL to the raw, playable video file.
        public string Source;
        //The last time the video or its caption was updated.
        public DateTime Updated_Time;
    }
}
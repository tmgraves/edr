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

    public class FacebookAlbum
    {
        //  The album ID.
        public string Id;
        //  Whether the viewer can upload photos to this album
        public bool Can_Upload;
        //  Number of photos in this album.
        public double? Count;
        //  The ID of the album's cover photo.
        public string Cover_Photo;
        public string Thumbnail;
        //  The time the album was initially created.
        public DateTime Created_Time;
        //  The description of the album.
        public string Description;
        //  The profile that created the album.
        //  public User From;
        //  A link to this album on Facebook.
        public string Link;
        //  The textual location of the album.
        public string Location;
        //  The title of the album.
        public string Name;
        //  The place associated with this album.
        //  public Page place;
        //  The privacy settings for the album.
        public string Privacy;
        //  The type of the album.
        //  type enum{app, cover, profile, mobile, wall, normal, album}
        //  The last time the album was updated.
        public DateTime Updated_Time;
        public List<FacebookPhoto> Photos;
    }

    public class FacebookPhoto
    {
        [Key]
        public string Id;
        public string Album;
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
        [Display(Name = "Event Name")]
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
        public string Location;
        public string FacebookId;
        public string WebsiteUrl;
        public string FacebookUrl;
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

    public class FacebookGroup
    {
        //  Group ID
        public string Id;
        //  Information about the group's cover photo.
        public Picture Cover;
        //  A brief description of the group.
        public string Description;
        //  The email address to upload content to the group. Only current members of the group can use this.
        public string Email;
        //  The URL for the group's icon.
        public string Icon;
        //  The group's website.
        public string Link;
        //  The name of the group.
        public string Name;
        //  The profile that created this group.
        //  public User|Page owner;
        //  The privacy setting of the group.
        public string Privacy;
        //  The last time the group was updated (this includes changes in the group's properties and changes in posts and comments if the session user can see them).
        public DateTime Updated_Time;
        public List<FacebookPost> Posts;
    }

    public class FacebookFeed
    {
        public string Message;
    }

    public class FacebookPost
    {
        //  The post ID
        public string Id;
        //  A list of available actions on the post (including commenting, liking, and any optional app-specified actions).
        //  public object[] actions;
        //  Information about the app this post was published by.
        //  public App application;
        //  The call to action type used in any Page posts for mobile app engagement ads.
        //  object call_to_action
        //  The caption of a link in the post (appears beneath the name).
        public string Caption;
        //  The time the post was initially published.
        public DateTime Created_Time;
        //  A description of a link in the post (appears beneath the caption).
        public string Description;
        //  Object that controls news feed targeting for this post. Anyone in these groups will be more likely to see this post, others will be less likely, but may still see it anyway. Any of the targeting fields shown here can be used, none are required (applies to Pages only).
        //  object feed_targeting
        //  Information about the profile that posted the message.
        //  Profile from
        //  A link to an icon representing the type of this post.
        public string Icon;
        //  If this post is marked as hidden (applies to Pages only).
        public bool Is_Hidden;
        //  The link attached to this post.
        public string Link;
        //  The status message in the post.
        public string Message;
        //  Profiles tagged in message.
        //  object message_tags
        //  The name of the link.
        public string Name;
        //  The ID of any uploaded photo or video attached to the post.
        public string Object_Id;
        //  The picture scraped from any link included with the post.
        public string Picture;
        //  Any location information attached to the post.
        //  Page place
        //  The privacy settings of the post.
        //  object privacy
        //  A list of properties for any attached video, for example, the length of the video.
        //  object[] properties
        //  The shares count of this post. For public posts, it is only shown after the post has been shared more than 10 times.
        //  object shares
        //  A URL to any Flash movie or video file attached to the post.
        public string Source;
        //  Description of the type of a status update.
        //  enum{mobile_status_update, created_note, added_photos, added_video, shared_story, created_group, created_event, wall_post, app_created_story, published_story, tagged_in_photo, approved_friend} status_type
        //  Text from stories not intentionally generated by users, such as those generated when two people become friends, or when someone else posts on the person's wall.
        public string Story;
        //  Deprecated field, same as message_tags.
        //  object story_tags
        //  Profiles mentioned or targeted in this post.
        //  to Profile[]
        //  A string indicating the object type of this post.
        //  enum{link, status, photo, video, offer} type
        public string Type;
        //  The time of the last change to this post, or the comments on it.
        public DateTime Updated_Time;
        //  Profiles tagged as being 'with' the publisher of the post.
        //  Profile[] with_tags
    }
}
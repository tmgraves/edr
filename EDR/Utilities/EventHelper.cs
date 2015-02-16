using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EDR.Models;
using EDR.Data;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using System.Security.Claims;
using System.IO;
using System.Net;
using System.Web.Mvc;
using EDR.Models.ViewModels;
using EDR.Enums;
using Microsoft.AspNet.Identity;
using EDR.Utilities;
using System.Data.Entity;

namespace EDR.Utilities
{
    public class EventHelper
    {
        public static List<Media> BuildAllUpdates(IEnumerable<Event> events, MediaTarget target)
        {
            var lstMedia = new List<Media>();
            foreach(var evt in events)
            {
                BuildUpdates(evt, target, ref lstMedia);
            }
            return lstMedia;
        }

        public static void BuildUpdates(Event evt, MediaTarget target, ref List<Media> lstMedia)
        {
            foreach (var p in evt.Pictures)
            {
                lstMedia.Add(new EventMedia() { Event = evt, Id = p.Id, SourceName = p.Title, SourceLink = p.SourceLink, Author = p.PostedBy, MediaDate = p.PhotoDate, MediaType = Enums.MediaType.Picture, PhotoUrl = p.Filename, Title = p.Title, MediaSource = p.MediaSource, Link = p.Filename, Target = target });
            }
            foreach (var v in evt.Videos)
            {
                lstMedia.Add(new EventMedia() { Event = evt, Id = v.Id, SourceName = v.Title, SourceLink = v.VideoUrl, Author = v.Author, MediaDate = v.PublishDate, MediaType = Enums.MediaType.Video, PhotoUrl = v.PhotoUrl, MediaUrl = v.VideoUrl, Title = v.Title, MediaSource = v.MediaSource, Target = target });
            }
            foreach (var lst in evt.Playlists)
            {
                var videos = YouTubeHelper.GetPlaylistVideos(lst.YouTubeId);

                foreach (var movie in videos)
                {
                    lstMedia.Add(new EventMedia() { Event = evt, SourceName = movie.Title, SourceLink = movie.VideoLink.ToString(), Author = lst.Author, MediaDate = movie.PubDate, MediaType = Enums.MediaType.Video, PhotoUrl = movie.Thumbnail.ToString(), MediaUrl = movie.VideoLink.ToString(), Title = movie.Title, MediaSource = lst.MediaSource, Target = target });
                }
            }

            foreach (var fbob in evt.LinkedFacebookObjects.Where(f => f.MediaSource == MediaSource.Facebook))
            {
                if (evt.Creator != null && evt.Creator.FacebookToken != null)
                {
                    var posts = FacebookHelper.GetFeed(fbob.Id, evt.Creator.FacebookToken);
                    foreach (var post in posts)
                    {
                        if (post.Type == "video")
                        {
                            lstMedia.Add(new EventMedia() { Event = evt, SourceName = fbob.Name, SourceLink = fbob.Url, Author = evt.Creator, MediaDate = post.Created_Time, MediaType = Enums.MediaType.Video, PhotoUrl = post.Picture, MediaUrl = post.Source, Text = post.Description, MediaSource = MediaSource.Facebook, Link = post.Source, Target = target });
                        }
                        else if (post.Type == "photo")
                        {
                            lstMedia.Add(new EventMedia() { Event = evt, SourceName = fbob.Name, SourceLink = fbob.Url, Author = evt.Creator, MediaDate = post.Created_Time, MediaType = Enums.MediaType.Picture, PhotoUrl = post.Picture, Text = post.Description, MediaSource = MediaSource.Facebook, Link = post.Link, Target = target });
                        }
                        else if (post.Type == "status")
                        {
                            lstMedia.Add(new EventMedia() { Event = evt, SourceName = fbob.Name, SourceLink = fbob.Url, Author = evt.Creator, MediaDate = post.Created_Time, MediaType = Enums.MediaType.Comment, Title = post.Name, Text = post.Message, MediaSource = MediaSource.Facebook, Link = post.Source, Target = target });
                        }
                    }
                }
            }
        }
    }
}
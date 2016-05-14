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
            return BuildAllUpdates(events, target, false);
        }

        public static List<Media> BuildAllUpdates(IEnumerable<Event> events, MediaTarget target, bool mediaOnly = false)
        {
            var lstMedia = new List<Media>();
            foreach(var evt in events)
            {
                BuildUpdates(evt, target, ref lstMedia, mediaOnly);
            }
            return lstMedia;
        }

        public static void BuildUpdates(Event evt, MediaTarget target, ref List<Media> lstMedia)
        {
            BuildUpdates(evt, target, ref lstMedia, false);
        }

        public static void BuildVideos(Event evt, MediaTarget target, ref List<Media> lstMedia)
        {
            BuildUpdates(evt, target, ref lstMedia, false, true);
        }

        public static void BuildUpdates(Event evt, MediaTarget target, ref List<Media> lstMedia, bool mediaOnly = false, bool videosOnly = false, bool picturesOnly = false)
        {
            if (!videosOnly)
            {
                if (evt.Pictures != null)
                {
                    foreach (var p in evt.Pictures)
                    {
                        lstMedia.Add(new EventMedia() { Event = evt, Id = p.Id, SourceName = p.Title, SourceLink = p.SourceLink, Author = p.PostedBy, MediaDate = p.PhotoDate, MediaType = Enums.MediaType.Picture, PhotoUrl = p.Filename, Title = p.Title, MediaSource = p.MediaSource, Link = p.Filename, Target = target });
                    }
                }

                if (evt.Albums != null)
                {
                    foreach (var a in evt.Albums)
                    {
                        if (a.PostedBy.FacebookToken != null && a.PostedBy.FacebookToken != null)
                        {
                            var photos = FacebookHelper.GetAlbumPhotos(a.PostedBy.FacebookToken, a.FacebookId);

                            foreach (var photo in photos)
                            {
                                lstMedia.Add(new EventMedia() { Event = evt, SourceName = a.Name, SourceLink = a.SourceLink, Author = a.PostedBy, MediaDate = photo.PhotoDate, MediaType = Enums.MediaType.Picture, PhotoUrl = photo.LargeSource, Title = photo.Name, MediaSource = a.MediaSource, Link = photo.Link, Target = target });
                            }
                        }
                    }
                }
            }
            if (!picturesOnly)
            {
                if (evt.Videos != null)
                {
                    foreach (var v in evt.Videos)
                    {
                        lstMedia.Add(new EventMedia() { Event = evt, Id = v.Id, SourceName = v.Title, SourceLink = v.VideoUrl.ToString(), Author = v.Author, MediaDate = v.PublishDate, MediaType = Enums.MediaType.Video, PhotoUrl = v.PhotoUrl.ToString(), MediaUrl = v.VideoUrl.ToString(), Title = v.Title, MediaSource = v.MediaSource, Target = target });
                    }

                }

                if (evt.Playlists != null)
                {
                    var videos = new List<YouTubeVideo>();
                    foreach (var lst in evt.Playlists)
                    {
                        var ytids = videos.Select(v => v.Id).ToArray();
                        videos.AddRange(YouTubeHelper.GetPlaylistVideos(lst.YouTubeId).Where(v => !ytids.Contains(v.Id)).ToList());
                        lstMedia.AddRange(videos.Select(v => new EventMedia() { Event = evt, SourceName = v.Title, SourceLink = v.VideoLink.ToString(), MediaDate = v.PubDate, MediaType = Enums.MediaType.Video, PhotoUrl = v.Thumbnail.ToString(), MediaUrl = v.VideoLink.ToString(), Title = v.Title, MediaSource = MediaSource.YouTube, Target = target, Playlist = lst, Author = lst.Author } ).ToList());
                    }

                    //foreach (var movie in videos)
                    //{
                    //    lstMedia.Add(new EventMedia() { Event = evt, SourceName = movie.Title, SourceLink = movie.VideoLink.ToString(), Author = lst.Author, MediaDate = movie.PubDate, MediaType = Enums.MediaType.Video, PhotoUrl = movie.Thumbnail.ToString(), MediaUrl = movie.VideoLink.ToString(), Title = movie.Title, MediaSource = lst.MediaSource, Target = target, Playlist = lst });
                    //}
                }
            }

            if (evt.LinkedFacebookObjects != null)
            {
                foreach (var fbob in evt.LinkedFacebookObjects.Where(f => f.MediaSource == MediaSource.Facebook))
                {
                    if (evt.Creator != null && evt.Creator.FacebookToken != null)
                    {
                        var posts = FacebookHelper.GetFeed(fbob.FacebookId, evt.Creator.FacebookToken);
                        
                        if (posts != null)
                        {
                            foreach (var post in posts.Where(p => !p.Is_Hidden))
                            {
                                if (post.Type == "video")
                                {
                                    if (!picturesOnly)
                                    {
                                        lstMedia.Add(new EventMedia() { Event = evt, SourceName = fbob.Name, SourceLink = fbob.Url, Author = evt.Creator, MediaDate = post.Created_Time, MediaType = Enums.MediaType.Video, PhotoUrl = post.Picture, MediaUrl = post.Source, Text = post.Description, Title = post.Description, MediaSource = MediaSource.Facebook, Link = post.Source, Target = target });
                                    }
                                }
                                else if (post.Type == "photo")
                                {
                                    if (!videosOnly)
                                    {
                                        lstMedia.Add(new EventMedia() { Event = evt, SourceName = fbob.Name, SourceLink = fbob.Url, Author = evt.Creator, MediaDate = post.Created_Time, MediaType = Enums.MediaType.Picture, PhotoUrl = post.Picture, Text = post.Description, MediaSource = MediaSource.Facebook, Link = post.Link, Target = target });
                                    }
                                }
                                else if (post.Type == "status" && !mediaOnly && post.Message != null)
                                {
                                    if (!videosOnly && !picturesOnly)
                                    {
                                        lstMedia.Add(new EventMedia() { Event = evt, SourceName = fbob.Name, SourceLink = fbob.Url, Author = evt.Creator, MediaDate = post.Created_Time, MediaType = Enums.MediaType.Comment, Title = post.Name, Text = post.Message, MediaSource = MediaSource.Facebook, Link = post.Source, Target = target });
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
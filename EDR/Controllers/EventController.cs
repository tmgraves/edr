﻿using EDR.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using EDR.Models;
using System.Data.Entity.Validation;
using EDR.Enums;
using System.Data.Entity;
using EDR.Utilities;
using System.IO;

namespace EDR.Controllers
{
    public class EventController : BaseController
    {
        #region reviews
        public ActionResult Reviews(int id, EventType eventType)
        {
            var model = LoadEvent(id, eventType);
            var reviews = new EventReviewsViewModel();
            reviews.EventReviews = DataContext.Reviews.Where(x => x.Event.Id == id);
            reviews.EventId = id;
            reviews.NewReview = new Review();
            reviews.NewReview.Like = true;
            var userid = User.Identity.GetUserId();
            reviews.NewReview.Author = DataContext.Users.Where(x => x.Id == userid).FirstOrDefault();

            model.Reviews = reviews;

            return View(model);
        }

        public ActionResult Reviews2(int id, EventType eventType)
        {
            var reviews = new EventReviewsViewModel();
            reviews.EventReviews = DataContext.Reviews.Where(x => x.Event.Id == id);
            reviews.EventId = id;
            reviews.NewReview = new Review();
            reviews.NewReview.Like = true;
            var userid = User.Identity.GetUserId();
            reviews.NewReview.Author = DataContext.Users.Where(x => x.Id == userid).FirstOrDefault();

            return View(reviews);
        }

        public ActionResult Reviews_Insert(EventReviewsViewModel model)
        {
            var er = ModelState.IsValid;
            var userid = User.Identity.GetUserId();
            model.NewReview.Author = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();
            model.NewReview.Event = DataContext.Events.Where(e => e.Id == model.EventId).FirstOrDefault();
            var today = DateTime.Today;
            model.NewReview.ReviewDate = today;
            DataContext.Reviews.Add(model.NewReview);
            DataContext.SaveChanges();

            var reviews = DataContext.Reviews.Where(x => x.Event.Id == model.NewReview.Event.Id);
            return PartialView("~/Views/Shared/Events/_Reviews.cshtml", reviews);
        }
        #endregion

        //public ActionResult Details(int id, EventType eventType)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }

        //    var model = new EventDetailViewModel();
        //    model.Event = DataContext.Events.Include("DanceStyles").Include("Reviews").Where(x => x.Id == id).FirstOrDefault();
        //    model.EventType = eventType;

        //    if (model.Event == null)
        //    {
        //        return HttpNotFound();
        //    }

        //    if (model.Event is Class)
        //    {
        //        model.Teachers = DataContext.Teachers.Where(t => t.Classes.Any(c => c.Id == id)).ToList();
        //    }
        //    else if (model.Event is Workshop)
        //    {
        //        model.Teachers = DataContext.Teachers.Where(t => t.Workshops.Any(w => w.Id == id)).ToList();
        //    }

        //    return View(model);
        //}

        public ActionResult View(int id, EventType eventType)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var model = LoadEvent(id, eventType);
            model.EventType = eventType;
            model.LinkedFacebookObjects = DataContext.Events.Where(e => e.Id == id).Include("LinkedFacebookObjects").FirstOrDefault().LinkedFacebookObjects;

            if (model.Event == null)
            {
                return HttpNotFound();
            }

            return View(model);
        }

        public ActionResult Pictures(int id, EventType eventType)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var model = LoadEvent(id, eventType);

            if (model.Event == null)
            {
                return HttpNotFound();
            }

            return View(model);
        }

        public ActionResult Videos(int id, EventType eventType)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var model = LoadEvent(id, eventType);
            model.EventType = eventType;

            if (model.Event == null)
            {
                return HttpNotFound();
            }

            return View(model);
        }
        public ActionResult GetVideos(int id, EventType eventType)
        {
            var model = new EventVideos();
            model.EventType = eventType;
            model.ReturnUrl = Url.Action("Videos", new { id = id, eventType = eventType });

            var evt = DataContext.Events.Where(e => e.Id == id)
                    .Include("Creator")
                    .Include("Videos")
                    .Include("Videos.Author")
                    .Include("Playlists")
                    .Include("Playlists.Author")
                    .Include("LinkedFacebookObjects")
                    .FirstOrDefault();

            var lstMedia = new List<Media>();
            EventHelper.BuildVideos(evt, MediaTarget.Event, ref lstMedia);
            var lstVideos = new List<EventVideo>();
            foreach (var m in lstMedia)
            {
                    lstVideos.Add(new EventVideo() { Event = model.Event, Id = m.Id, Author = m.Author, PublishDate = m.MediaDate, PhotoUrl = m.PhotoUrl, VideoUrl = m.MediaUrl, Title = m.Title, MediaSource = m.MediaSource, PlayList = m.Playlist });
            }
            model.Videos = lstVideos;
            return PartialView("~/Views/Shared/Events/_VideosPartial.cshtml", model);
        }

        public ActionResult GetPictures(int id, EventType eventType)
        {
            var model = new EventPictures();
            model.EventType = eventType;
            model.ReturnUrl = Url.Action("Pictures", new { id = id, eventType = eventType });
            var lstPictures = new List<EventPicture>();

            var evt = DataContext.Events.Where(e => e.Id == id)
                    .Include("Creator")
                    .Include("Pictures")
                    .Include("Pictures.PostedBy")
                    .FirstOrDefault();

            lstPictures = evt.Pictures.ToList();

            if (evt.FacebookId != null)
            {
                if (evt.Creator != null && evt.Creator.FacebookToken != null)
                {
                    var posts = new List<FacebookPost>();
                    if (Session["FacebookPosts"] != null)
                    {
                        posts = (List<FacebookPost>)Session["FacebookPosts"];
                    }
                    else
                    {
                        posts = FacebookHelper.GetFeed(evt.FacebookId, evt.Creator.FacebookToken);
                        Session["FacebookPosts"] = posts;
                    }

                    foreach (var post in posts.Where(p => p.Type == "photo"))
                    {
                        lstPictures.Add(new EventPicture() { Event = evt, PostedBy = evt.Creator, PhotoDate = post.Created_Time, Filename = post.Link, ThumbnailFilename = post.Picture, Title = post.Description, MediaSource = MediaSource.Facebook });
                    }
                }

            }

            model.Pictures = lstPictures;

            return PartialView("~/Views/Shared/Events/_PicturesPartial.cshtml", model);
        }

        public ActionResult GetFacebookPictures(int id, EventType eventType)
        {
            var model = new EventFacebookPictureContainer();
            model.EventType = eventType;
            var evt = DataContext.Events.Where(e => e.Id == id).Include("Creator").Include("Pictures").FirstOrDefault();
            model.Event = evt;

            var userid = User.Identity.GetUserId();
            var user = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();
            var token = user.FacebookToken;
            if (token != null)
            {
                if (Session["FacebookPictures"] == null)
                {
                    Session["FacebookPictures"] = FacebookHelper.GetPhotos(token);

                }
                var facebookIds = evt.Pictures.Where(p => p.FacebookId != null).Select(p => p.FacebookId).ToArray();

                model.FacebookPictures = ((List<FacebookPhoto>)Session["FacebookPictures"]).Where(p => !facebookIds.Any(f => f.Contains(p.Id)));
            }

            return PartialView("~/Views/Shared/Events/_AddFacebookPicturesPartial.cshtml", model);
        }

        public ActionResult GetFacebookVideos(int id, EventType eventType)
        {
            var model = new EventFacebookVideosContainer();
            model.EventType = eventType;
            var evt = DataContext.Events.Where(e => e.Id == id).Include("Creator").Include("Videos").FirstOrDefault();
            model.Event = evt;

            var userid = User.Identity.GetUserId();
            var user = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();
            var token = user.FacebookToken;
            if (token != null)
            {
                if (Session["FacebookVideos"] == null)
                {
                    Session["FacebookVideos"] = FacebookHelper.GetVideos(token);

                }
                var facebookIds = evt.Videos.Where(p => p.FacebookId != null).Select(p => p.FacebookId).ToArray();

                model.FacebookVideos = ((List<FacebookVideo>)Session["FacebookVideos"]).Where(p => !facebookIds.Any(f => f.Contains(p.Id)));
            }

            return PartialView("~/Views/Shared/Events/_AddFacebookVideosPartial.cshtml", model);
        }

        public ActionResult GetYouTubeVideos(int id, EventType eventType)
        {
            var model = new EventYouTubeVideosContainer();
            model.EventType = eventType;
            var evt = DataContext.Events.Where(e => e.Id == id).Include("Creator").Include("Videos").FirstOrDefault();
            model.Event = evt;

            var userid = User.Identity.GetUserId();
            var user = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();
            var ytUsername = user.YouTubeUsername;
            if (ytUsername != null)
            {
                if (Session["YouTubeVideos"] == null)
                {
                    Session["YouTubeVideos"] = YouTubeHelper.GetVideos(ytUsername);
                }
                var youtubeIds = evt.Videos.Where(p => p.YoutubeId != null).Select(p => p.YoutubeId).ToArray();

                model.YouTubeVideos = ((List<YouTubeVideo>)Session["YouTubeVideos"]).Where(p => !youtubeIds.Any(f => f.Contains(p.Id)));
            }

            return PartialView("~/Views/Shared/Events/_AddYouTubeVideosPartial.cshtml", model);
        }

        public ActionResult GetYouTubePlaylists(int id, EventType eventType)
        {
            var model = new EventYouTubePlaylistsContainer();
            model.EventType = eventType;
            var evt = DataContext.Events.Where(e => e.Id == id).Include("Creator").Include("Playlists").FirstOrDefault();
            model.Event = evt;

            var userid = User.Identity.GetUserId();
            var user = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();
            var ytUsername = user.YouTubeUsername;
            if (ytUsername != null)
            {
                if (Session["YouTubePlaylists"] == null)
                {
                    Session["YouTubePlaylists"] = YouTubeHelper.GetPlaylists(ytUsername);
                }
                var playlistIds = evt.Playlists.Where(p => p.YouTubeId != null).Select(p => p.YouTubeId).ToArray();

                model.YouTubePlaylists = ((List<YouTubePlaylist>)Session["YouTubePlaylists"]).Where(p => !playlistIds.Any(f => f.Contains(p.Id)));
            }

            return PartialView("~/Views/Shared/Events/_AddYouTubePlaylistsPartial.cshtml", model);
        }

        public ActionResult GetInstagramPictures(int id, EventType eventType)
        {
            var model = new EventInstagramPicturesContainer();
            model.EventType = eventType;
            var evt = DataContext.Events.Where(e => e.Id == id).Include("Creator").Include("Pictures").FirstOrDefault();
            model.Event = evt;

            var userid = User.Identity.GetUserId();
            var user = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();
            var token = user.InstagramToken;
            if (token != null)
            {
                if (Session["InstagramPictures"] == null)
                {
                    Session["InstagramPictures"] = InstagramHelper.GetPictures(token, (int)user.InstagramId);

                }
                var instagramIds = evt.Pictures.Where(p => p.InstagramId != null).Select(p => p.InstagramId).ToArray();

                model.InstagramPictures = ((List<InstagramPicture>)Session["InstagramPictures"]).Where(p => !instagramIds.Any(f => f.Contains(p.InstagramId)));
            }

            return PartialView("~/Views/Shared/Events/_AddInstagramPicturesPartial.cshtml", model);
        }

        public ActionResult GetUpdates(int id)
        {
            //  Media Updates
            var lstMedia = new List<Media>();
            var updates = new EventUpdates();
            var rebuild = false;

            if (Session["Event" + id.ToString() + "Updates"] != null)
            {
                updates = (EventUpdates)Session["Event" + id.ToString() + "Updates"];
                if (updates.Created < DateTime.Now.AddMinutes(-5))
                {
                    rebuild = true;
                }
            }
            else
            {
                rebuild = true;
            }

            if (rebuild)
            {
                var evt = DataContext.Events.Where(e => e.Id == id)
                        .Include("Creator")
                        .Include("Pictures")
                        .Include("Pictures.PostedBy")
                        .Include("Videos")
                        .Include("Videos.Author")
                        .Include("Playlists")
                        .Include("Playlists.Author")
                        .Include("LinkedFacebookObjects")
                        .FirstOrDefault();

                EventHelper.BuildUpdates(evt, MediaTarget.Event, ref lstMedia);
                Session["Event" + id.ToString() + "Updates"] = new EventUpdates() { Media = lstMedia, Created = DateTime.Now };
            }
            updates = (EventUpdates)Session["Event" + id.ToString() + "Updates"];
            lstMedia = updates.Media;

            return PartialView("~/Views/Shared/_MediaUpdatesPartial.cshtml", lstMedia);

            //foreach (var p in evt.Pictures)
            //{
            //    lstMedia.Add(new EventMedia() { Id = p.Id, SourceName = p.Title, SourceLink = p.SourceLink, Author = p.PostedBy, MediaDate = p.PhotoDate, MediaType = Enums.MediaType.Picture, PhotoUrl = p.Filename, Title = p.Title, MediaSource = p.MediaSource, Link = p.Filename });
            //}
            //foreach (var v in evt.Videos)
            //{
            //    lstMedia.Add(new EventMedia() { Id = v.Id, SourceName = v.Title, SourceLink = v.VideoUrl, Author = v.Author, MediaDate = v.PublishDate, MediaType = Enums.MediaType.Video, PhotoUrl = v.PhotoUrl, MediaUrl = v.VideoUrl, Title = v.Title, MediaSource = v.MediaSource });
            //}
            //foreach (var lst in evt.Playlists)
            //{
            //    var videos = YouTubeHelper.GetPlaylistVideos(lst.YouTubeId);

            //    foreach (var movie in videos)
            //    {
            //        lstMedia.Add(new EventMedia() { SourceName = movie.Title, SourceLink = movie.VideoLink.ToString(), Author = lst.Author, MediaDate = movie.PubDate, MediaType = Enums.MediaType.Video, PhotoUrl = movie.Thumbnail.ToString(), MediaUrl = movie.VideoLink.ToString(), Title = movie.Title, MediaSource = lst.MediaSource });
            //    }
            //}

            //foreach (var fbob in evt.LinkedFacebookObjects.Where(f => f.MediaSource == MediaSource.Facebook))
            //{
            //    if (evt.Creator != null && evt.Creator.FacebookToken != null)
            //    {
            //        var posts = FacebookHelper.GetFeed(fbob.Id, evt.Creator.FacebookToken);
            //        foreach (var post in posts)
            //        {
            //            if (post.Type == "video")
            //            {
            //                lstMedia.Add(new EventMedia() { SourceName = fbob.Name, SourceLink = fbob.Url, Author = evt.Creator, MediaDate = post.Created_Time, MediaType = Enums.MediaType.Video, PhotoUrl = post.Picture, MediaUrl = post.Source, Text = post.Description, MediaSource = MediaSource.Facebook, Link = post.Source });
            //            }
            //            else if (post.Type == "photo")
            //            {
            //                lstMedia.Add(new EventMedia() { SourceName = fbob.Name, SourceLink = fbob.Url, Author = evt.Creator, MediaDate = post.Created_Time, MediaType = Enums.MediaType.Picture, PhotoUrl = post.Picture, Text = post.Description, MediaSource = MediaSource.Facebook, Link = post.Link });
            //            }
            //            else if (post.Type == "status")
            //            {
            //                lstMedia.Add(new EventMedia() { SourceName = fbob.Name, SourceLink = fbob.Url, Author = evt.Creator, MediaDate = post.Created_Time, MediaType = Enums.MediaType.Comment, Title = post.Name, Text = post.Message, MediaSource = MediaSource.Facebook, Link = post.Source });
            //            }
            //        }
            //    }
            //}
            //  Media Updates

        }

        private EventViewModel LoadEvent(int id, EventType eventType)
        {
            var model = new EventViewModel();
            model.EventType = eventType;
            model.Event = DataContext.Events.Where(x => x.Id == id)
                    .Include("Place")
                    .Include("DanceStyles")
                    .Include("Reviews")
                    .Include("EventMembers")
                    .Include("EventMembers.Member")
                    .Include("Creator")
                    .FirstOrDefault();

            if (eventType == EventType.Class)
            {
                model.Class = DataContext.Events.OfType<Class>().Where(x => x.Id == id)
                    .Include("Teachers")
                    .Include("Teachers.ApplicationUser")
                    .FirstOrDefault();
                model.ClassTeacherInvitations = DataContext.ClassTeacherInvitations.Where(i => i.ClassId == id)
                                                        .Include("Teacher")
                                                        .Include("Teacher.ApplicationUser")
                                                        .Include("Teacher.ApplicationUser.UserPictures");
            }
            else if (eventType == EventType.Social)
            {
                model.Social = DataContext.Events.OfType<Social>().Where(x => x.Id == id).Include("Promoters").Include("Promoters.ApplicationUser").FirstOrDefault();
            }

            return model;
        }

        [Authorize]
        public ActionResult ApproveTeacher(int teacherId, int classId, string returnUrl)
        {
            var invite = DataContext.ClassTeacherInvitations.Where(i => i.TeacherId == teacherId && i.ClassId == classId).FirstOrDefault();
            invite.Approved = true;
            DataContext.Entry(invite).State = EntityState.Modified;
            var cl = DataContext.Events.OfType<Class>().Where(c => c.Id == classId).Include("Teachers").FirstOrDefault();
            cl.Teachers.Add(DataContext.Teachers.Where(t => t.Id == teacherId).FirstOrDefault());
            DataContext.Entry(cl).State = EntityState.Modified;
            DataContext.SaveChanges();
            ViewBag.Message = "Teacher was approved";
            return Redirect(returnUrl);
        }
        
        #region pictures
        [Authorize]
        public ActionResult ChangeCover(int id, EventType eventType)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var model = new EventChangeCoverViewModel();
            model.EventType = eventType;
            model.Event = DataContext.Events.Where(x => x.Id == id)
                        .Include("Creator")
                        .Include("Pictures")
                        .Include("Pictures.PostedBy")
                        .Include("Videos")
                        .Include("Videos.Author")
                        .Include("Playlists")
                        .Include("Playlists.Author")
                        .Include("LinkedFacebookObjects")
                        .FirstOrDefault();
            var lstMedia = new List<Media>();
            EventHelper.BuildUpdates(model.Event, MediaTarget.Event, ref lstMedia, true);
            model.Media = lstMedia;

            return View(model);
        }
        [Authorize]
        public ActionResult UploadPicture()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public ActionResult UploadPicture(HttpPostedFileBase file, EventChangePictureViewModel model, string returnUrl)
        {
            UploadFile newFile = ApplicationUtility.LoadPicture(file);

            if (newFile.UploadStatus == "Success")
            {
                var ev = DataContext.Events.Where(x => x.Id == model.Event.Id).Include("Pictures").FirstOrDefault();
                var today = DateTime.Now;
                var userid = User.Identity.GetUserId();
                var user = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();
                ev.Pictures.Add(new EventPicture() { Title = newFile.FileName, Filename = newFile.FilePath, ThumbnailFilename = newFile.ThumbnailFilePath, PhotoDate = today, PostedBy = user });
                DataContext.Entry(ev).State = EntityState.Modified;
                DataContext.SaveChanges();
            }
            else
            {
                ViewBag.Message = newFile.UploadStatus;
            }
            return Redirect(returnUrl);
        }

        [Authorize]
        public ActionResult DeletePicture(int pictureId, string returnUrl)
        {
            var picture = DataContext.Pictures.Find(pictureId);
            DataContext.Pictures.Remove(picture);
            DataContext.Entry(picture).State = EntityState.Deleted;
            DataContext.SaveChanges();
            ViewBag.Message = ApplicationUtility.DeletePicture(picture);
            return Redirect(returnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult ChangeCover(ChangePictureViewModel model)
        {
            return View(model);
        }

        [Authorize]
        public ActionResult Cover(int eventId, string photoUrl, string videoUrl, string returnUrl )
        {
            try
            {
                var ev = DataContext.Events.Where(x => x.Id == eventId).Include("Pictures").Include("Pictures.PostedBy").FirstOrDefault();
                ev.PhotoUrl = photoUrl;
                ev.VideoUrl = videoUrl;
                
                DataContext.Entry(ev).State = EntityState.Modified;
                DataContext.SaveChanges();
                return Redirect(returnUrl);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ChangePicture", "Event", new { id = eventId });
            }
        }

        [Authorize]
        public ActionResult AddFacebookPicture(int eventId, string id, string returnUrl)
        {
            try
            {
                string userId = User.Identity.GetUserId();
                var ev = DataContext.Events.Where(e => e.Id == eventId).Include("Pictures").FirstOrDefault();
                var picture = ((List<FacebookPhoto>)Session["FacebookPictures"]).Where(p => p.Id == id).FirstOrDefault();
                var userid = User.Identity.GetUserId();
                var postedby = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();
                ev.Pictures.Add(new EventPicture() { Title = picture.Name, ThumbnailFilename = picture.Source, Filename = picture.LargeSource, PhotoDate = picture.PhotoDate, PostedBy = postedby, MediaSource = MediaSource.Facebook, FacebookId = picture.Id, SourceLink = picture.Link });
                DataContext.Entry(ev).State = EntityState.Modified;
                DataContext.SaveChanges();
                return Redirect(returnUrl);
            }
            catch (Exception ex)
            {
                return Redirect(returnUrl);
            }
        }

        [Authorize]
        public ActionResult AddInstagramPicture(int eventId, string id, string returnUrl)
        {
            try
            {
                string userId = User.Identity.GetUserId();
                var ev = DataContext.Events.Where(e => e.Id == eventId).Include("Pictures").FirstOrDefault();
                var picture = ((List<InstagramPicture>)Session["InstagramPictures"]).Where(p => p.InstagramId == id).FirstOrDefault();
                var userid = User.Identity.GetUserId();
                var postedby = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();
                ev.Pictures.Add(new EventPicture() { Title = picture.Caption, ThumbnailFilename = picture.Thumbnail, Filename = picture.Photo, PhotoDate = picture.PhotoDate, PostedBy = postedby, MediaSource = MediaSource.Instagram, InstagramId = picture.InstagramId, SourceLink = picture.Link });
                DataContext.Entry(ev).State = EntityState.Modified;
                DataContext.SaveChanges();
                return Redirect(returnUrl);
            }
            catch (Exception ex)
            {
                return Redirect(returnUrl);
            }
        }

        [Authorize]
        public ActionResult PostPicture(int id, EventType eventType)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var model = new EventPostPictureViewModel();
            model.Event = DataContext.Events.Where(x => x.Id == id).Include("Place").Include("Pictures").FirstOrDefault();
            model.EventType = eventType;
            //var userid = User.Identity.GetUserId();
            //var token = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault().FacebookToken;
            //if (token != null)
            //{
            //    model.FacebookPictures = FacebookHelper.GetPhotos(token);
            //    Session["FacebookPictures"] = model.FacebookPictures;
            //}

            return View(model);
        }

#endregion

        #region videos
        [Authorize]
        public ActionResult PostVideo(int id, EventType eventType)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var model = new EventPostVideoViewModel();
            model.EventType = eventType;
            model.Event = DataContext.Events.Where(x => x.Id == id)
                                .Include("Place")
                                .Include("Videos")
                                .Include("Videos.Author")
                                .Include("Playlists")
                                .Include("Playlists.Author")
                                .FirstOrDefault();
            var userid = User.Identity.GetUserId();
            var user = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();
            var youtubeUsername = user.YouTubeUsername;
            if (youtubeUsername != null)
            {
                model.YoutubeVideos = YouTubeHelper.GetVideos(youtubeUsername);
                Session["YouTubeVideos"] = model.YoutubeVideos;
                model.YouTubePlaylists = YouTubeHelper.GetPlaylists(youtubeUsername);
                Session["YouTubePlaylists"] = model.YouTubePlaylists;
            }
            var facebookToken = user.FacebookToken;
            if (facebookToken != null)
            {
                model.FacebookVideos = FacebookHelper.GetVideos(facebookToken);
                Session["FacebookVideos"] = model.FacebookVideos;
            }

            return View(model);
        }
        [Authorize]
        public ActionResult ImportYouTubeVideo(string videoId, int eventId, string returnUrl)
        {
            var ytVideo = ((List<YouTubeVideo>)Session["YouTubeVideos"]).Where(x => x.Id == videoId).FirstOrDefault();
            var userid = User.Identity.GetUserId();

            var auth = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();

            var eVideo = new EventVideo() { Title = ytVideo.Title, PublishDate = ytVideo.PubDate, YoutubeId = videoId, Author = auth, VideoUrl = "https://www.youtube.com/watch?v=" + videoId + "&feature=player_embedded", PhotoUrl = "https://img.youtube.com/vi/" + videoId + "/mqdefault.jpg", MediaSource = MediaSource.YouTube };

            var ev = DataContext.Events.Where(e => e.Id == eventId).Include("Videos").FirstOrDefault();
            ev.Videos.Add(eVideo);
            DataContext.Entry(ev).State = EntityState.Modified;
            DataContext.SaveChanges();
            ViewBag.Message = "Video was imported";
            return Redirect(returnUrl);
        }
        [Authorize]
        public ActionResult ImportFacebookVideo(string videoId, int eventId, string returnUrl)
        {
            var video = ((List<FacebookVideo>)Session["FacebookVideos"]).Where(x => x.Id == videoId).FirstOrDefault();
            var userid = User.Identity.GetUserId();

            var auth = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();

            var eVideo = new EventVideo() { Title = video.Name != null ? video.Name : "No Title", PublishDate = video.Created_Time, FacebookId = videoId, Author = auth, VideoUrl = "https://www.facebook.com/video.php?v=" + videoId, PhotoUrl = video.Picture, MediaSource = MediaSource.Facebook };

            var ev = DataContext.Events.Where(e => e.Id == eventId).Include("Videos").FirstOrDefault();
            ev.Videos.Add(eVideo);
            DataContext.Entry(ev).State = EntityState.Modified;
            DataContext.SaveChanges();
            ViewBag.Message = "Video was imported";
            return Redirect(returnUrl);
        }
        [Authorize]
        public ActionResult ImportYouTubeList(string Id, int eventId, string returnUrl)
        {
            var ytPlaylist = ((List<YouTubePlaylist>)Session["YouTubePlaylists"]).Where(x => x.Id == Id).FirstOrDefault();
            var userid = User.Identity.GetUserId();
            var auth = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();
            var ev = DataContext.Events.Where(e => e.Id == eventId).Include("Playlists").FirstOrDefault();
            var ePlaylist = new EventPlaylist() { Title = ytPlaylist.Name, PublishDate = ytPlaylist.PubDate, YouTubeId = ytPlaylist.Id, Author = auth, YouTubeUrl = ytPlaylist.Url, CoverPhoto = ytPlaylist.ThumbnailUrl, MediaSource = MediaSource.YouTube };

            ev.Playlists.Add(ePlaylist);
            DataContext.Entry(ev).State = EntityState.Modified;
            DataContext.SaveChanges();
            ViewBag.Message = "Playlist was imported";
            return Redirect(returnUrl);
        }
        [Authorize]
        public ActionResult ImportYouTubePlaylistLink(string playlistUrl, int eventId, string returnUrl)
        {
            var ytPlaylist = YouTubeHelper.GetPlaylist(playlistUrl);
            var userid = User.Identity.GetUserId();
            var auth = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();
            var ev = DataContext.Events.Where(e => e.Id == eventId).Include("Playlists").FirstOrDefault();
            var ePlaylist = new EventPlaylist() { Title = ytPlaylist.Name, PublishDate = ytPlaylist.PubDate, YouTubeId = ytPlaylist.Id, Author = auth, YouTubeUrl = ytPlaylist.Url, CoverPhoto = ytPlaylist.ThumbnailUrl, MediaSource = MediaSource.YouTube };

            ev.Playlists.Add(ePlaylist);
            DataContext.Entry(ev).State = EntityState.Modified;
            DataContext.SaveChanges();
            ViewBag.Message = "Playlist was imported";
            return Redirect(returnUrl);
        }
        [Authorize]
        public ActionResult ImportPlayListVideoLink(string videoUrl, int eventId, string returnUrl)
        {
            var ytVideo = YouTubeHelper.GetVideo(videoUrl);
            var userid = User.Identity.GetUserId();
            var auth = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();
            var ev = DataContext.Events.Where(e => e.Id == eventId).Include("Videos").FirstOrDefault();
            var eVideo = new EventVideo() { Title = ytVideo.Title, PublishDate = ytVideo.PubDate, YoutubeId = ytVideo.Id, Author = auth, VideoUrl = "https://www.youtube.com/watch?v=" + ytVideo.Id + "&feature=player_embedded", PhotoUrl = "https://img.youtube.com/vi/" + ytVideo.Id + "/mqdefault.jpg", MediaSource = MediaSource.YouTube };

            ev.Videos.Add(eVideo);
            DataContext.Entry(ev).State = EntityState.Modified;
            DataContext.SaveChanges();
            ViewBag.Message = "Video was imported";
            return Redirect(returnUrl);
        }

        [Authorize]
        public ActionResult DeleteVideo(int videoId, string returnUrl)
        {
            DataContext.Videos.Remove(DataContext.Videos.Where(v => v.Id == videoId).FirstOrDefault());
            DataContext.SaveChanges();
            ViewBag.Message = "Video was deleted";
            return Redirect(returnUrl);
        }
        [Authorize]
        public ActionResult DeletePlaylist(int listId, string returnUrl)
        {
            DataContext.Playlists.Remove(DataContext.Playlists.Where(l => l.Id == listId).FirstOrDefault());
            DataContext.SaveChanges();
            ViewBag.Message = "Playlist was removed";
            return Redirect(returnUrl);
        }
        #endregion

        [Authorize]
        public ActionResult JoinTeachers(int eventId, string returnUrl)
        {
            var userid = User.Identity.GetUserId();
            var teacher = DataContext.Teachers.Where(t => t.ApplicationUser.Id == userid).FirstOrDefault();
            var cls = DataContext.Events.OfType<Class>().Where(c => c.Id == eventId).FirstOrDefault();
            DataContext.ClassTeacherInvitations.Add(new ClassTeacherInvitation() { Teacher = teacher, Class = cls });
            DataContext.SaveChanges();
            ViewBag.Message = "You requested to join this class as a teacher";
            return Redirect(returnUrl);
        }

        [Authorize]
        public ActionResult Signup(int id, string returnUrl)
        {
            var userId = User.Identity.GetUserId();
            var user = DataContext.Users.Where(x => x.Id == userId).FirstOrDefault();
            var evt = DataContext.Events.Where(e => e.Id == id).FirstOrDefault();
            if (DataContext.EventMembers.Where(x => x.Member.Id == user.Id && x.Event.Id == id).ToList().Count == 0)
            {
                var newMember = new EventMember() { Event = evt, Member = user };
                DataContext.EventMembers.Add(newMember);
            }

            if (evt is Class)
            {
                var teachers = DataContext.Teachers.Where(t => t.Classes.Any(c => c.Id == id)).ToList();
                foreach (Teacher t in teachers)
                {
                    if (DataContext.Students.Where(x => x.DancerId == user.Id && x.TeacherId == t.Id).ToList().Count == 0)
                    {
                        DataContext.Students.Add(new Student() { Teacher = t, Dancer = user });
                    }
                }
            }
            DataContext.SaveChanges();
            return Redirect(returnUrl);
        }

        [Authorize]
        public ActionResult LeaveEvent(int id, string returnUrl)
        {
            var userId = User.Identity.GetUserId();
            var user = DataContext.Users.Where(x => x.Id == userId).FirstOrDefault();
            var evt = DataContext.Events.Where(e => e.Id == id).FirstOrDefault();
            DataContext.EventMembers.Remove(DataContext.EventMembers.Where(m => m.UserId == userId && m.EventId == id).FirstOrDefault());
            DataContext.SaveChanges();
            return Redirect(returnUrl);
        }

        [Authorize]
        public ActionResult Edit(int id, EventType eventType)
        {
            var model = new EventEditViewModel();
            var ev = DataContext.Events.Where(e => e.Id == id).Include("DanceStyles").FirstOrDefault();
            model.Event = ev;
            model.EventType = eventType;
            if (eventType == EventType.Class)
            {
                model.ClassType = DataContext.Events.OfType<Class>().Where(c => c.Id == id).FirstOrDefault().ClassType;
            }
            else
            {
                model.SocialType = DataContext.Events.OfType<Social>().Where(s => s.Id == id).FirstOrDefault().SocialType;
            }

            var userid = User.Identity.GetUserId();

            //  For Dance Styles Checkbox List
            var selectedStyles = new List<DanceStyleListItem>();
            foreach (DanceStyle ss in model.Event.DanceStyles)
            {
                selectedStyles.Add(new DanceStyleListItem { Id = ss.Id, Name = ss.Name });
            }
            model.SelectedStyles = selectedStyles;

            var styles = new List<DanceStyleListItem>();
            foreach (DanceStyle s in DataContext.DanceStyles)
            {
                styles.Add(new DanceStyleListItem { Id = s.Id, Name = s.Name });
            }
            model.AvailableStyles = styles.OrderBy(x => x.Name);
            //  For Dance Styles Checkbox List

            //  For Month Days Checkbox List
            var selectedMonthDays = new List<SelectListItem>();
            string[] daysarray;
            model.SelectedMonthDays = new List<SelectListItem>();
            if (model.Event.MonthDays != null)
            {
                daysarray = model.Event.MonthDays.Split(new char[] { '-' });
                foreach(var day in daysarray)
                {
                    model.SelectedMonthDays.Add(new SelectListItem() { Value = day, Text = day });
                }
            }
            model.MonthDays = new List<SelectListItem>() { new SelectListItem() { Value = "1", Text = "1st" }, new SelectListItem() { Value = "2", Text = "2nd" }, new SelectListItem() { Value = "3", Text = "3rd" }, new SelectListItem() { Value = "4", Text = "4th" } };
            //  For Month Days Checkbox List

            //  Set Month day text
            var daysofmonth = new string[] {"blank", "1st", "2nd", "3rd", "4th", "5th", "6th", "7th", "8th", "9th", "10th", "11th", "12th", "13th", "14th", "15th", "16th", "17th", "18th", "19th", "20th", "21st", "22nd", "23rd", "24th", "25th", "26th", "27th", "28th", "29th", "30th", "31st"};
            model.MonthDay = daysofmonth[model.Event.StartDate.Day];
            //  Set Month day text

            model.Places = new List<PlaceItem>();
            var places = new List<Place>();
            if (eventType == EventType.Class)
            {
                var teacher = DataContext.Teachers.Where(t => t.Classes.Any(c => c.Id == id) && t.ApplicationUser.Id == userid).Include("Places").FirstOrDefault();
                if (teacher == null)
                {
                    var owner = DataContext.Owners.Where(o => o.Classes.Any(c => c.Id == id) && o.ApplicationUser.Id == userid).Include("Places").FirstOrDefault();
                    if (owner != null)
                    {
                        places = owner.Places.ToList();
                    }
                }
                else
                {
                    places = teacher.Places.ToList();
                }
            }
            else
            {
                var promoter = DataContext.Promoters.Where(p => p.Socials.Any(s => s.Id == id) && p.ApplicationUser.Id == userid).Include("Places").FirstOrDefault();
                if (promoter == null)
                {
                    var owner = DataContext.Owners.Where(o => o.Socials.Any(c => c.Id == id) && o.ApplicationUser.Id == userid).Include("Places").FirstOrDefault();
                    places = owner.Places.ToList();
                }
                else
                {
                    places = promoter.Places.ToList();
                }
            }

            foreach(var pl in places)
            {
                model.Places.Add(new PlaceItem() { Address = pl.Address, Address2 = pl.Address2, City = pl.City, Country = pl.Country, FacebookId = pl.FacebookId, FacebookLink = pl.FacebookLink, Filename = pl.Filename, Id = pl.Id, Latitude = pl.Latitude, Longitude = pl.Longitude, Name = pl.Name, PlaceType = pl.PlaceType, State = pl.State, ThumbnailFilename = pl.ThumbnailFilename, Website = pl.Website, Zip = pl.Zip, Selected = ev.Place.Id == pl.Id ? true : false });
            }

            return View(model);
        }

        [Authorize]
        [HttpPost]
        public ActionResult Edit(EventEditViewModel model)
        {
            try
            {
                var userid = User.Identity.GetUserId();
                var user = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();

                //  Update Event Details
                var evt = DataContext.Events.Where(c => c.Id == model.Event.Id).Include("DanceStyles").FirstOrDefault();
                evt.Name = model.Event.Name;
                evt.StartDate = model.Event.StartDate;
                evt.AllDay = model.Event.AllDay;
                evt.StartTime = model.Event.StartTime;
                evt.EndTime = model.Event.EndTime;
                evt.EndDate = model.Event.EndDate;
                evt.Description = model.Event.Description;
                evt.Recurring = model.Event.Recurring;
                evt.Frequency = model.Event.Frequency;
                evt.Interval = model.Event.Interval;
                if (model.PostedMonthDays != null)
                {
                    evt.MonthDays = String.Join("-", model.PostedMonthDays) + "-" + model.HiddenMonthDay;
                }
                else
                {
                    evt.MonthDays = model.HiddenMonthDay;
                }
                //  Dance Styles
                evt.DanceStyles.Clear();
                var styles = DataContext.DanceStyles.Where(s => model.PostedStyles.DanceStyleIds.Contains(s.Id.ToString()));
                evt.DanceStyles = styles.ToList();
                evt.Place = model.Event.Place;
                //  Dance Styles

                if (model.EventType == EventType.Class)
                {
                    var cls = DataContext.Events.OfType<Class>().Where(c => c.Id == model.Event.Id).FirstOrDefault();
                    cls.ClassType = model.ClassType;
                }
                else
                {
                    var soc = DataContext.Events.OfType<Social>().Where(c => c.Id == model.Event.Id).FirstOrDefault();
                    soc.SocialType = model.SocialType;
                }

                DataContext.Entry(evt).State = EntityState.Modified;
                DataContext.SaveChanges();

                return RedirectToAction("View", "Event", new { id = model.Event.Id, eventType = model.EventType });
            }
            catch (DbEntityValidationException e)
            {
                var msg = "";
                foreach (var eve in e.EntityValidationErrors)
                {
                    msg = eve.Entry.Entity.GetType().Name + " " + eve.Entry.State;
                    foreach (var ve in eve.ValidationErrors)
                    {
                        msg = ve.PropertyName + " " + ve.ErrorMessage;
                    }
                }
                return View();
            }

            return View();
        }

        [Authorize]
        public ActionResult Delete(int id)
        {
            var evt = DataContext.Events.Where(e => e.Id == id).Include("Videos").Include("Pictures").Include("Playlists").FirstOrDefault();

            var returnUrl = "";
            //  Set Return
            if (evt is Class)
            {
                var userid = User.Identity.GetUserId();
                var user = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();
                if (DataContext.Events.OfType<Class>().Where(c => c.Id == id).FirstOrDefault().Teachers.Where(t => t.ApplicationUser.Id == userid).Count() == 1)
                {
                    returnUrl = Url.Action("Home", "Teacher", new { username = User.Identity.Name });
                }
            }
            if (evt is Social)
            {
                var userid = User.Identity.GetUserId();
                var user = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();
                if (DataContext.Events.OfType<Social>().Where(c => c.Id == id).FirstOrDefault().Promoters.Where(p => p.ApplicationUser.Id == userid).Count() == 1)
                {
                    returnUrl = Url.Action("Home", "Promoter", new { username = User.Identity.Name });
                }
            }
            if (returnUrl == "")
            {
                returnUrl = Url.Action("Home", "Dancer", new { username = User.Identity.Name });
            }
            //  Set Return
            
            evt.Videos.Clear();
            evt.Pictures.Clear();
            evt.Playlists.Clear();
            DataContext.Events.Remove(evt);
            DataContext.SaveChanges();

            return Redirect(returnUrl);
        }

        [Authorize]
        public ActionResult Create(RoleName role, EventType eventType, int? placeId)
        {
            var model = new EventCreateViewModel();
            var id = User.Identity.GetUserId();
            model.Role = role;
            model.EventType = eventType;

            LoadModel(model);

            if (placeId != null)
            {
                model.PlaceId = (int)placeId;
            }

            var userid = User.Identity.GetUserId();
            var user = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();

            if (user.FacebookToken != null)
            {
                if (Session["FacebookEvents"] == null)
                {
                    Session["FacebookEvents"] = FacebookHelper.GetEvents(user.FacebookToken);
                }
                if (Session["FacebookEvents"] != null)
                {
                    model.FacebookEvents = (List<FacebookEvent>)Session["FacebookEvents"];
                }
            }

            return View(model);
        }

        //[Authorize]
        //public ActionResult AddFacebookEvent(string id, RoleName role, EventType eventType)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            var userid = User.Identity.GetUserId();
        //            var user = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();
        //            int eventId;

        //            var fbevent = ((List<FacebookEvent>)Session["FacebookEvents"]).Where(x => x.Id == id).FirstOrDefault();

        //            Address ad = new Address();
        //            Place pl = new Place();
        //            if (fbevent.Address.FacebookId != null)
        //            {
        //                ad = Utilities.Geolocation.ParseAddress(fbevent.Address.Street + " " + fbevent.Address.City + ", " + fbevent.Address.State + " " + fbevent.Address.ZipCode);
        //                pl = new Place() { FacebookId = fbevent.Address.FacebookId, Name = fbevent.Location, PlaceType = Enums.PlaceType.OtherPlace, Zip = fbevent.Address.ZipCode, Address = fbevent.Address.Street, City = fbevent.Address.City, State = (Enums.State)System.Enum.Parse(typeof(Enums.State), fbevent.Address.State), Latitude = fbevent.Address.Latitude, Longitude = fbevent.Address.Longitude };
        //            }
        //            else
        //            {
        //                ad = Utilities.Geolocation.ParseAddress(user.ZipCode != null ? user.ZipCode : "90065");
        //                pl = new Place() { Name = "TBD", PlaceType = Enums.PlaceType.OtherPlace, Zip = user.ZipCode != null ? user.ZipCode : "90065", Address = ad.StreetNumber + " " + ad.StreetName, City = ad.City, State = (Enums.State)System.Enum.Parse(typeof(Enums.State), ad.State), Latitude = ad.Latitude, Longitude = ad.Longitude };
        //            }

        //            if (eventType == EventType.Class)
        //            {
        //                var cls = new Class() { Name = fbevent.Name, ClassType = Enums.ClassType.Class, Description = fbevent.Description, EndDate = fbevent.EndTime, FacebookId = id, StartDate = fbevent.StartTime, PhotoUrl = fbevent.CoverPhoto.LargeSource, Place = pl };
        //                if (role == RoleName.Teacher)
        //                {
        //                    var teacher = DataContext.Teachers.Where(x => x.ApplicationUser.Id == userid).Include("ApplicationUser").FirstOrDefault();
        //                    teacher.Classes.Add(cls);
        //                    DataContext.Entry(teacher).State = EntityState.Modified;
        //                    DataContext.SaveChanges();
        //                }
        //                else if (role == RoleName.Owner)
        //                {
        //                    var owner = DataContext.Owners.Where(x => x.ApplicationUser.Id == userid).Include("ApplicationUser").FirstOrDefault();
        //                    owner.Classes.Add(cls);
        //                    DataContext.Entry(owner).State = EntityState.Modified;
        //                    DataContext.SaveChanges();
        //                }
        //                eventId = cls.Id;
        //            }
        //            else
        //            {
        //                var social = new Social() { Name = fbevent.Name, SocialType = Enums.SocialType.Social, Description = fbevent.Description, EndDate = fbevent.EndTime, FacebookId = id, StartDate = fbevent.StartTime, PhotoUrl = fbevent.CoverPhoto.LargeSource, Place = pl };
        //                if (role == RoleName.Promoter)
        //                {
        //                    var promoter = DataContext.Promoters.Where(x => x.ApplicationUser.Id == userid).Include("ApplicationUser").FirstOrDefault();
        //                    promoter.Socials.Add(social);
        //                    DataContext.Entry(promoter).State = EntityState.Modified;
        //                    DataContext.SaveChanges();
        //                }
        //                else if (role == RoleName.Owner)
        //                {
        //                    var owner = DataContext.Owners.Where(x => x.ApplicationUser.Id == userid).Include("ApplicationUser").FirstOrDefault();
        //                    owner.Socials.Add(social);
        //                    DataContext.Entry(owner).State = EntityState.Modified;
        //                    DataContext.SaveChanges();
        //                }
        //                eventId = social.Id;
        //            }
        //            return RedirectToAction("View", "Event", new { id = eventId, eventType = eventType });
        //        }
        //        catch (DbEntityValidationException e)
        //        {
        //            var msg = "";
        //            foreach (var eve in e.EntityValidationErrors)
        //            {
        //                msg = eve.Entry.Entity.GetType().Name + " " + eve.Entry.State;
        //                foreach (var ve in eve.ValidationErrors)
        //                {
        //                    msg = ve.PropertyName + " " + ve.ErrorMessage;
        //                }
        //            }
        //            return View();
        //        }
        //    }
        //    return View();
        //}

        [Authorize]
        [HttpPost]
        public ActionResult ConfirmFacebookEvent(ConfirmFacebookEvent model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var userid = User.Identity.GetUserId();
                    var user = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();
                    int eventId;

                    Place pl = new Place();
                    if (model.PlaceId == 0)
                    {
                        if (model.NewPlace.Latitude == 0.0 || model.NewPlace.Longitude == 0.0)
                        {
                            var ad = Geolocation.ParseAddress(model.NewPlace.Address + " " + model.NewPlace.City + ", " + model.NewPlace.State + " " + model.NewPlace.Zip);
                            model.NewPlace.Longitude = ad.Longitude;
                            model.NewPlace.Latitude = ad.Latitude;
                        }
                        pl = model.NewPlace;
                        pl.PlaceType = Enums.PlaceType.OtherPlace;
                    }
                    else if (model.PlaceId > 0)
                    {
                        pl = DataContext.Places.Where(p => p.Id == model.PlaceId).FirstOrDefault();
                    }
                    else
                    {
                        Address ad = new Address();
                        ad = Utilities.Geolocation.ParseAddress(user.ZipCode != null ? user.ZipCode : "90065");
                        pl = new Place() { Name = "TBD", PlaceType = Enums.PlaceType.OtherPlace, Zip = user.ZipCode != null ? user.ZipCode : "90065", Address = ad.StreetNumber + " " + ad.StreetName, City = ad.City, State = (Enums.State)System.Enum.Parse(typeof(Enums.State), ad.State), Latitude = ad.Latitude, Longitude = ad.Longitude };
                    }
                    model.NewEvent.Place = pl;

                    var obj = new LinkedFacebookObject() { MediaSource = MediaSource.Facebook, Name = model.NewEvent.Name, Id = model.NewEvent.FacebookId, Url = model.NewEvent.FacebookLink, ObjectType = FacebookObjectType.Event };
                    if (model.EventType == EventType.Class)
                    {
                        var cls = new Class() { Name = model.NewEvent.Name, Description = model.NewEvent.Description, FacebookId = model.NewEvent.FacebookId, PhotoUrl = model.NewEvent.PhotoUrl, StartDate = model.NewEvent.StartDate, EndDate = model.NewEvent.EndDate, ClassType = model.ClassType, Place = pl, FacebookLink = model.NewEvent.FacebookLink, Creator = user, DanceStyles = DataContext.DanceStyles.Where(s => model.PostedStyles.DanceStyleIds.Any(ps => ps == s.Id.ToString())).ToList(), Interval = 1, LinkedFacebookObjects = new List<LinkedFacebookObject>() { obj } };
                        if (model.Role == RoleName.Teacher)
                        {
                            var teacher = DataContext.Teachers.Where(x => x.ApplicationUser.Id == userid).Include("ApplicationUser").Include("Places").FirstOrDefault();
                            teacher.Classes.Add(cls);
                            teacher.Places.Add(pl);
                            DataContext.Entry(teacher).State = EntityState.Modified;
                            DataContext.SaveChanges();
                        }
                        else if (model.Role == RoleName.Owner)
                        {
                            var owner = DataContext.Owners.Where(x => x.ApplicationUser.Id == userid).Include("ApplicationUser").FirstOrDefault();
                            owner.Classes.Add(cls);
                            DataContext.Entry(owner).State = EntityState.Modified;
                            DataContext.SaveChanges();
                        }
                        eventId = cls.Id;
                    }
                    else
                    {
                        var social = new Social() { Name = model.NewEvent.Name, Description = model.NewEvent.Description, FacebookId = model.NewEvent.FacebookId, PhotoUrl = model.NewEvent.PhotoUrl, StartDate = model.NewEvent.StartDate, EndDate = model.NewEvent.EndDate, SocialType = model.SocialType, Place = pl, FacebookLink = model.NewEvent.FacebookLink, Creator = user, DanceStyles = DataContext.DanceStyles.Where(s => model.PostedStyles.DanceStyleIds.Any(ps => ps == s.Id.ToString())).ToList(), Interval = 1, LinkedFacebookObjects = new List<LinkedFacebookObject>() { obj } };
                        if (model.Role == RoleName.Promoter)
                        {
                            var promoter = DataContext.Promoters.Where(x => x.ApplicationUser.Id == userid).Include("ApplicationUser").FirstOrDefault();
                            promoter.Socials.Add(social);
                            DataContext.Entry(promoter).State = EntityState.Modified;
                            DataContext.SaveChanges();
                        }
                        else if (model.Role == RoleName.Owner)
                        {
                            var owner = DataContext.Owners.Where(x => x.ApplicationUser.Id == userid).Include("ApplicationUser").FirstOrDefault();
                            owner.Socials.Add(social);
                            DataContext.Entry(owner).State = EntityState.Modified;
                            DataContext.SaveChanges();
                        }
                        eventId = social.Id;
                    }
                    return RedirectToAction("View", "Event", new { id = eventId, eventType = model.EventType });
                }
                catch (DbEntityValidationException e)
                {
                    var msg = "";
                    foreach (var eve in e.EntityValidationErrors)
                    {
                        msg = eve.Entry.Entity.GetType().Name + " " + eve.Entry.State;
                        foreach (var ve in eve.ValidationErrors)
                        {
                            msg = ve.PropertyName + " " + ve.ErrorMessage;
                        }
                    }
                    return View();
                }
            }
            return View();
        }

        [Authorize]
        public ActionResult ConfirmFacebookEvent(string id, RoleName role, EventType eventType)
        {
            var model = new ConfirmFacebookEvent();
            var userid = User.Identity.GetUserId();

            //  Load Dance Styles
            var selectedStyles = new List<DanceStyleListItem>();
            model.SelectedStyles = selectedStyles;

            var styles = new List<DanceStyleListItem>();
            foreach (DanceStyle s in DataContext.DanceStyles)
            {
                styles.Add(new DanceStyleListItem { Id = s.Id, Name = s.Name });
            }
            model.AvailableStyles = styles.OrderBy(x => x.Name);
            //  Load Dance Styles

            model.EventType = eventType;
            var fbevent = ((List<FacebookEvent>)Session["FacebookEvents"]).Where(x => x.Id == id).FirstOrDefault();
            model.Role = role;
            model.PlaceId = -1;
            model.NewPlace = new PlaceItem() { Id = 0, Name = fbevent.Location, Address = fbevent.Address.Street, City = fbevent.Address.City, State = fbevent.Address.State != null ? (State)Enum.Parse(typeof(State), fbevent.Address.State) : State.CA, Zip = fbevent.Address.ZipCode, Latitude = fbevent.Address.Latitude, Longitude = fbevent.Address.Longitude};
            model.Places = new List<PlaceItem>();
            var blankPlace = new PlaceItem() { Id = 0 };
            model.Places.Add(blankPlace);
            model.NewEvent = new Event() { Name = fbevent.Name, Description = fbevent.Description, StartDate = Convert.ToDateTime(fbevent.StartTime.ToShortDateString()), EndDate = fbevent.EndTime, PhotoUrl = fbevent.CoverPhoto.LargeSource, FacebookId = fbevent.Id, FacebookLink = fbevent.EventLink, Interval = 1 };
            if (eventType == EventType.Class)
            {

                if (role == RoleName.Teacher)
                {
                    foreach(var pl in DataContext.Places.Where(p => p.Teachers.Any(t => t.ApplicationUser.Id == userid)))
                    {
                        model.Places.Add(new PlaceItem() { Address = pl.Address, Address2 = pl.Address2, City = pl.City, Country = pl.Country, FacebookId = pl.FacebookId, FacebookLink = pl.FacebookLink, Filename = pl.Filename, Id = pl.Id, Latitude = pl.Latitude, Longitude = pl.Longitude, Name = pl.Name, PlaceType = pl.PlaceType, State = pl.State, ThumbnailFilename = pl.ThumbnailFilename, Website = pl.Website, Zip = pl.Zip });
                    }
                }
                else
                {
                    foreach (var pl in DataContext.Places.Where(p => p.Owners.Any(o => o.ApplicationUser.Id == userid)))
                    {
                        model.Places.Add(new PlaceItem() { Address = pl.Address, Address2 = pl.Address2, City = pl.City, Country = pl.Country, FacebookId = pl.FacebookId, FacebookLink = pl.FacebookLink, Filename = pl.Filename, Id = pl.Id, Latitude = pl.Latitude, Longitude = pl.Longitude, Name = pl.Name, PlaceType = pl.PlaceType, State = pl.State, ThumbnailFilename = pl.ThumbnailFilename, Website = pl.Website, Zip = pl.Zip });
                    }
                }
            }
            else
            {
                if (role == RoleName.Promoter)
                {
                    foreach (var pl in DataContext.Places.Where(p => p.Promoters.Any(pr => pr.ApplicationUser.Id == userid)))
                    {
                        model.Places.Add(new PlaceItem() { Address = pl.Address, Address2 = pl.Address2, City = pl.City, Country = pl.Country, FacebookId = pl.FacebookId, FacebookLink = pl.FacebookLink, Filename = pl.Filename, Id = pl.Id, Latitude = pl.Latitude, Longitude = pl.Longitude, Name = pl.Name, PlaceType = pl.PlaceType, State = pl.State, ThumbnailFilename = pl.ThumbnailFilename, Website = pl.Website, Zip = pl.Zip });
                    }
                }
                else
                {
                    foreach (var pl in DataContext.Places.Where(p => p.Owners.Any(o => o.ApplicationUser.Id == userid)))
                    {
                        model.Places.Add(new PlaceItem() { Address = pl.Address, Address2 = pl.Address2, City = pl.City, Country = pl.Country, FacebookId = pl.FacebookId, FacebookLink = pl.FacebookLink, Filename = pl.Filename, Id = pl.Id, Latitude = pl.Latitude, Longitude = pl.Longitude, Name = pl.Name, PlaceType = pl.PlaceType, State = pl.State, ThumbnailFilename = pl.ThumbnailFilename, Website = pl.Website, Zip = pl.Zip });
                    }
                }
            }

            if (fbevent.Address.Street != null && fbevent.Address.City != null && fbevent.Address.State != null && fbevent.Address.ZipCode != null)
            {
                var fbeventAddress = Geolocation.ParseAddress(fbevent.Address.Street + " " + fbevent.Address.City + ", " + fbevent.Address.State + " " + fbevent.Address.ZipCode);
                var pl = model.Places.Where(p => p.Address == fbeventAddress.Street && p.City == fbeventAddress.City && p.State.ToString() == fbeventAddress.State).FirstOrDefault();
                if (pl != null)
                {
                    pl.Selected = true;
                }
                else
                {
                    blankPlace.Selected = true;
                }
            }
            else
            {
                blankPlace.Selected = true;
            }

            return View(model);
        }

        private void LoadModel(EventCreateViewModel model)
        {
            var id = User.Identity.GetUserId();

            var selectedStyles = new List<DanceStyleListItem>();
            model.SelectedStyles = selectedStyles;

            var styles = new List<DanceStyleListItem>();
            foreach (DanceStyle s in DataContext.DanceStyles)
            {
                styles.Add(new DanceStyleListItem { Id = s.Id, Name = s.Name });
            }
            model.AvailableStyles = styles.OrderBy(x => x.Name);

            if (model.Role == RoleName.Teacher)
            {
                model.PlaceList = DataContext.Places.Where(x => x.Teachers.Any(t => t.ApplicationUser.Id == id)).Select(p => new SelectListItem() { Text = p.Name, Value = p.Id.ToString() }).ToList();
            }
            else if (model.Role == RoleName.Owner)
            {
                model.PlaceList = DataContext.Places.Where(x => x.Owners.Any(t => t.ApplicationUser.Id == id)).Select(p => new SelectListItem() { Text = p.Name, Value = p.Id.ToString() }).ToList();
            }
            else if (model.Role == RoleName.Promoter)
            {
                model.PlaceList = DataContext.Places.Where(x => x.Promoters.Any(t => t.ApplicationUser.Id == id)).Select(p => new SelectListItem() { Text = p.Name, Value = p.Id.ToString() }).ToList();
            }
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(EventCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var event1 = model.Event;
                event1.StartDate = event1.StartDate.AddHours(model.Time.Hour).AddMinutes(model.Time.Minute);
                event1.Place = DataContext.Places.Find(model.PlaceId);
                var id = User.Identity.GetUserId();

                event1.DanceStyles = new List<DanceStyle>();
                var styles = DataContext.DanceStyles.Where(x => model.PostedStyles.DanceStyleIds.Contains(x.Id.ToString())).ToList();

                event1.DanceStyles.Clear();

                foreach (DanceStyle s in styles)
                {
                    event1.DanceStyles.Add(s);
                }

                if (model.EventType == EventType.Class)
                {
                    var class1 = ConvertToClass(event1);
                    if (model.Role == RoleName.Teacher)
                    {
                        var teacher = DataContext.Teachers.Include("ApplicationUser").Where(x => x.ApplicationUser.Id == id).FirstOrDefault();
                        class1.Teachers.Add(teacher);
                    }
                    DataContext.Events.Add(class1);
                }
                else if (model.EventType == EventType.Social)
                {
                    var social = ConvertToSocial(event1);
                    if (model.Role == RoleName.Promoter)
                    {
                        var promoter = DataContext.Promoters.Include("ApplicationUser").Where(x => x.ApplicationUser.Id == id).FirstOrDefault();
                        social.Promoters.Add(promoter);
                    }
                    DataContext.Events.Add(social);
                }
                DataContext.SaveChanges();
                //promoter.ContactEmail = model.Promoter.ContactEmail;
                //promoter.Website = model.Promoter.Website;
                //promoter.Facebook = model.Promoter.Facebook;

                //DataContext.Entry(promoter).State = EntityState.Modified;
                //DataContext.SaveChanges();
                //return RedirectToAction("Manage", "Account");
                if (Session["ReturnUrl"] != null)
                {
                    return Redirect(Session["ReturnUrl"].ToString());
                }
                else
                {
                    return View();
                }
            }
            else
            {
                var allErrors = ModelState.Values.SelectMany(v => v.Errors);
            }
            LoadModel(model);
            return View(model);
        }

        [Authorize]
        public PartialViewResult PostReview(EventViewModel model)
        {
            var userid = User.Identity.GetUserId();
            if (DataContext.Reviews.Where(r => r.Event.Id == model.Event.Id && r.Author.Id == userid).Count() == 0)
            {
                var auth = DataContext.Users.Where(x => x.Id == userid).FirstOrDefault();
                var ev = DataContext.Events.Where(e => e.Id == model.Event.Id).Include("Reviews").FirstOrDefault();
                ev.Reviews.Add(new Review() { ReviewText = model.Reviews.NewReview.ReviewText, ReviewDate = DateTime.Now, Like = model.Reviews.NewReview.Like, Author = auth });
                DataContext.SaveChanges();

                var reviews = DataContext.Reviews.Where(x => x.Event.Id == model.Event.Id);
                return PartialView("~/Views/Shared/Events/_ReviewsPartial.cshtml", reviews);
            }
            else
            {
                return PartialView();
            }
            
            //if (ModelState.IsValid)
            //{
            //    return RedirectToAction("Class", "Event", new { id = model.Event.Id});
            //}
            //else
            //{
            //    var allErrors = ModelState.Values.SelectMany(v => v.Errors);
            //}
            //return View(model);
        }

        [Authorize]
        public ActionResult AddFacebookLink(int id, EventType eventType)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var model = LoadEvent(id, eventType);
            model.EventType = eventType;

            if (model.Event == null)
            {
                return HttpNotFound();
            }

            return View(model);
        }

        [Authorize]
        public ActionResult GetAvailableFacebookEvents(int id, EventType eventType)
        {
            var model = new EventLinkedFacebookEventContainer();
            model.EventType = eventType;
            var userid = User.Identity.GetUserId();
            var user = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();
            var evt = DataContext.Events.Where(e => e.Id == id).FirstOrDefault();
            model.Event = evt;

            var facebookToken = user.FacebookToken;
            if (facebookToken != null)
            {
                //  Get Facebook Events
                var eventIds = DataContext.Events.Where(e => e.Id == id).Include("LinkedFacebookObjects").FirstOrDefault().LinkedFacebookObjects.Where(fo => fo.ObjectType == FacebookObjectType.Event).Select(ee => ee.Id).ToArray();
                if (Session["ExternalFacebookEvents"] == null)
                {
                    Session["ExternalFacebookEvents"] = FacebookHelper.GetEvents(facebookToken).Where(f => !eventIds.Any(e => e.Contains(f.Id))).ToList();
                }
                else
                {
                    Session["ExternalFacebookEvents"] = ((List<FacebookEvent>)Session["ExternalFacebookEvents"]).Where(f => !eventIds.Any(e => e.Contains(f.Id))).ToList();
                }
                model.FacebookEvents = (List<FacebookEvent>)Session["ExternalFacebookEvents"];
                //  Get Facebook Events
            }

            return PartialView("~/Views/Shared/Events/_AddLinkedFacebookEventsPartial.cshtml", model);
        }

        [Authorize]
        public ActionResult GetAvailableFacebookGroups(int id, EventType eventType)
        {
            var model = new EventLinkedFacebookGroupContainer();
            model.EventType = eventType;
            var userid = User.Identity.GetUserId();
            var user = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();
            var evt = DataContext.Events.Where(e => e.Id == id).FirstOrDefault();
            model.Event = evt;

            var facebookToken = user.FacebookToken;
            if (facebookToken != null)
            {
                //  Get Facebook Groups
                var groupIds = DataContext.Events.Where(e => e.Id == id).Include("LinkedFacebookObjects").FirstOrDefault().LinkedFacebookObjects.Where(fo => fo.ObjectType == FacebookObjectType.Group).Select(ee => ee.Id).ToArray();
                if (Session["ExternalFacebookGroups"] == null)
                {
                    Session["ExternalFacebookGroups"] = FacebookHelper.GetGroups(facebookToken).Where(f => !groupIds.Any(e => e.Contains(f.Id))).ToList();
                }
                else
                {
                    Session["ExternalFacebookGroups"] = ((List<FacebookGroup>)Session["ExternalFacebookGroups"]).Where(f => !groupIds.Any(e => e.Contains(f.Id))).ToList();
                }
                model.FacebookGroups = (List<FacebookGroup>)Session["ExternalFacebookGroups"];
                //  Get Facebook Groups
            }

            return PartialView("~/Views/Shared/Events/_AddLinkedFacebookGroupsPartial.cshtml", model);
        }

        [Authorize]
        public ActionResult RemoveLinkedFacebookObject(string id, string returnUrl)
        {
            DataContext.LinkedFacebookObjects.Remove(DataContext.LinkedFacebookObjects.Where(o => o.Id == id).FirstOrDefault());
            DataContext.SaveChanges();
            return Redirect(returnUrl);
        }

        [Authorize]
        public ActionResult LinkFacebookEvent(string id, int eventId, string returnUrl)
        {
            var fbEvent = ((List<FacebookEvent>)Session["ExternalFacebookEvents"]).Where(f => f.Id == id).FirstOrDefault();
            DataContext.Events.Where(e => e.Id == eventId).Include("LinkedFacebookObjects").FirstOrDefault().LinkedFacebookObjects.Add(new LinkedFacebookObject { Id = id, MediaSource = MediaSource.Facebook, Name = fbEvent.Name, Url = fbEvent.EventLink, ObjectType = FacebookObjectType.Event });
            DataContext.SaveChanges();
            return Redirect(returnUrl);
        }

        [Authorize]
        public ActionResult LinkFacebookGroup(string id, int eventId, string returnUrl)
        {
            var fbGroup = ((List<FacebookGroup>)Session["ExternalFacebookGroups"]).Where(f => f.Id == id).FirstOrDefault();
            DataContext.Events.Where(e => e.Id == eventId).Include("LinkedFacebookObjects").FirstOrDefault().LinkedFacebookObjects.Add(new LinkedFacebookObject { Id = id, MediaSource = MediaSource.Facebook, Name = fbGroup.Name, Url = "https://www.facebook.com/groups/" + fbGroup.Id, ObjectType = FacebookObjectType.Group });
            DataContext.SaveChanges();
            return Redirect(returnUrl);
        }

        public ActionResult GetRelatedEvents(int id, EventType eventType)
        {
            //  Get Events
            var model = new RelatedEvents();
            model.EventType = eventType;

            if (eventType == EventType.Class)
            {
                model.Events = DataContext.Events.OfType<Class>().Where(c => (c.Teachers.Any(t => t.Classes.Any(cl => cl.Id == id)) || c.Owners.Any(t => t.Classes.Any(cl => cl.Id == id))) && c.Id != id).Cast<Event>();
            }
            else
            {
                model.Events = DataContext.Events.OfType<Social>().Where(c => (c.Promoters.Any(t => t.Socials.Any(cl => cl.Id == id)) || c.Owners.Any(t => t.Socials.Any(cl => cl.Id == id))) && c.Id != id).Cast<Event>();
            }

            return PartialView("~/Views/Shared/Events/_RelatedEventsPartial.cshtml", model);
            //  Get Events
        }
        
        public Class ConvertToClass(Event event1)
        {
            var class1 = new Class()
            {
                Name = event1.Name,
                Description = event1.Description,
                FacebookLink = event1.FacebookLink,
                StartDate = event1.StartDate,
                EndDate = event1.EndDate,
                Price = event1.Price,
                IsAvailable = event1.IsAvailable,
                Recurring = event1.Recurring,
                Frequency = event1.Frequency,
                Interval = event1.Interval,
                Duration = event1.Duration,
                Place = event1.Place,
                Teachers = new List<Teacher>(),
                DanceStyles = event1.DanceStyles
            };

            return class1;
        }

        public Social ConvertToSocial(Event event1)
        {
            var social = new Social()
            {
                Name = event1.Name,
                Description = event1.Description,
                FacebookLink = event1.FacebookLink,
                StartDate = event1.StartDate,
                EndDate = event1.EndDate,
                Price = event1.Price,
                IsAvailable = event1.IsAvailable,
                Recurring = event1.Recurring,
                Frequency = event1.Frequency,
                Interval = event1.Interval,
                Duration = event1.Duration,
                Place = event1.Place,
                DanceStyles = event1.DanceStyles,
                Promoters = new List<Promoter>()
            };

            return social;
        }

        public Rehearsal ConvertToRehearsal(Event event1)
        {
            var rehearsal = new Rehearsal()
            {
                Name = event1.Name,
                Description = event1.Description,
                FacebookLink = event1.FacebookLink,
                StartDate = event1.StartDate,
                EndDate = event1.EndDate,
                Price = event1.Price,
                IsAvailable = event1.IsAvailable,
                Recurring = event1.Recurring,
                Frequency = event1.Frequency,
                Interval = event1.Interval,
                Duration = event1.Duration,
                Place = event1.Place,
                DanceStyles = event1.DanceStyles
            };

            return rehearsal;
        }
    }
}
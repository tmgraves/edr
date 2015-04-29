using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EDR.Controllers;
using System.Data.Entity;
using EDR.Utilities;
using System.IO;
using EDR.Models;
using Microsoft.AspNet.Identity;
using EDR.Enums;
using EDR.Areas.Admin.Models.ViewModels;

namespace EDR.Areas.Admin.Controllers
{
    public class DanceStyleController : BaseController
    {
        // GET: Admin/DanceStyle
        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            var styles = DataContext.DanceStyles;
            return View(styles);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Details(int id)
        {
            var style = DataContext.DanceStyles.Where(s => s.Id == id).Include("Videos").FirstOrDefault();
            return View(style);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            var style = new DanceStyle();

            if (id != null)
            {
                style = DataContext.DanceStyles.Where(s => s.Id == id).FirstOrDefault();
            }
            
            if (style == null)
            {
                return HttpNotFound();
            }
            else
            {
                return View(style);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult Edit(DanceStyle style)
        {
            if (ModelState.IsValid)
            {
                DataContext.DanceStyles.Add(style);
                DataContext.SaveChanges();

                return RedirectToAction("Details", new { id = style.Id });
            }
            else
            {
                return View(style);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult Details(DanceStyle postedStyle)
        {
            var style = DataContext.DanceStyles.Where(s => s.Id == postedStyle.Id).FirstOrDefault();
            style.Name = postedStyle.Name;
            style.Description = postedStyle.Description;
            style.SpotifyPlaylist = postedStyle.SpotifyPlaylist;
            DataContext.Entry(style).State = EntityState.Modified;
            DataContext.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult UploadPicture(HttpPostedFileBase file, int id)
        {
            UploadFile newFile = ApplicationUtility.LoadPicture(file);

            if (newFile.UploadStatus == "Success")
            {
                DataContext.DanceStyles.Where(s => s.Id == id).FirstOrDefault().PhotoUrl = newFile.FilePath;
                DataContext.SaveChanges();
                return RedirectToAction("Details", "DanceStyle", new { id = id } );
            }
            else
            {
                ViewBag.Message = newFile.UploadStatus;
                return RedirectToAction("Details", "DanceStyle", new { id = id });
            }
        }

        [Authorize(Roles = "Admin")]
        public ActionResult GetFacebookVideos(int id)
        {
            var model = new DanceStyleFacebookVideosViewModel();
            model.DanceStyle = DataContext.DanceStyles.Where(s => s.Id == id).FirstOrDefault();
            var videos = DataContext.Videos.OfType<DanceStyleVideo>().Where(s => s.Id == id);

            var userid = User.Identity.GetUserId();
            var user = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();
            var token = user.FacebookToken;
            if (token != null)
            {
                if (Session["StyleFacebookVideos"] == null)
                {
                    Session["StyleFacebookVideos"] = FacebookHelper.GetVideos(token);

                }
                var facebookIds = videos.Where(v => v.FacebookId != null).Select(v => v.FacebookId).ToArray();

                model.Videos = ((List<FacebookVideo>)Session["StyleFacebookVideos"]).Where(p => !facebookIds.Any(f => f.Contains(p.Id)));
            }

            return PartialView("~/Areas/Admin/Views/DanceStyle/_FacebookVideosPartial.cshtml", model);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult AddFacebookVideo(int id, string videoId)
        {
            var video = ((List<FacebookVideo>)Session["FacebookVideos"]).Where(x => x.Id == videoId).FirstOrDefault();
            var userid = User.Identity.GetUserId();
            var auth = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();

            var style = DataContext.DanceStyles.Where(s => s.Id == id).Include("Videos").FirstOrDefault();
            style.Videos.Add(new DanceStyleVideo() { Title = video.Name != null ? video.Name : "No Title", PublishDate = video.Created_Time, FacebookId = videoId, Author = auth, VideoUrl = "https://www.facebook.com/video.php?v=" + videoId, PhotoUrl = video.Picture, MediaSource = MediaSource.Facebook });
            DataContext.Entry(style).State = EntityState.Modified;
            DataContext.SaveChanges();
            ViewBag.Message = "Video was imported";
            return RedirectToAction("Details", new { id = id });
        }

        [Authorize(Roles = "Admin")]
        public ActionResult ImportYouTubeVideoUrl(string videoUrl, int id)
        {
            var ytVideo = YouTubeHelper.GetVideo(videoUrl);
            var userid = User.Identity.GetUserId();
            var auth = DataContext.Users.Where(u => u.Id == userid).FirstOrDefault();
            var style = DataContext.DanceStyles.Where(s => s.Id == id).Include("Videos").FirstOrDefault();

            style.Videos.Add(new DanceStyleVideo() { Title = ytVideo.Title, PublishDate = ytVideo.PubDate, YoutubeId = ytVideo.Id, Author = auth, VideoUrl = "https://www.youtube.com/watch?v=" + ytVideo.Id + "&feature=player_embedded", PhotoUrl = "https://img.youtube.com/vi/" + ytVideo.Id + "/mqdefault.jpg", MediaSource = MediaSource.YouTube });
            DataContext.Entry(style).State = EntityState.Modified;
            DataContext.SaveChanges();
            ViewBag.Message = "Video was imported";
            return RedirectToAction("Details", new { id = id });
        }

        [Authorize(Roles = "Admin")]
        public ActionResult DeleteVideo(int id, int videoId)
        {
            var style = DataContext.DanceStyles.Where(s => s.Id == id).Include("Videos").FirstOrDefault();
            style.Videos.Remove(style.Videos.Where(v => v.Id == videoId).FirstOrDefault());
            DataContext.Entry(style).State = EntityState.Modified;
            DataContext.SaveChanges();
            ViewBag.Message = "Video was Deleted";
            return RedirectToAction("Details", new { id = id });
        }
    
    }
}
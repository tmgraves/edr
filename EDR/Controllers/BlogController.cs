using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EDR.Models;
using EDR.Models.ViewModels;
using Microsoft.AspNet.Identity;
using System.Text.RegularExpressions;

namespace EDR.Controllers
{
    public class BlogController : BaseController
    {
        // GET: Blog
        [Route("Blog")]
        public ActionResult Index(BlogIndexViewModel model)
        {
            model.Blogs = DataContext.Blogs.ToList();
            if (User.Identity.IsAuthenticated)
            {
                model.NewBlog = new Blog();
                model.NewBlog.UserId = User.Identity.GetUserId();
            }

            if (model.Lat != null && model.Lng != null)
            {
                model.Blogs = model.Blogs.Where(e => (e.Longitude >= model.Lng - .5 && e.Longitude <= model.Lng + .5) && (e.Latitude >= model.Lat - 5 && e.Latitude <= model.Lat + 5)).ToList();
            }
            if (model.BlogDate != null)
            {
                model.Blogs = model.Blogs.Where(b => b.BlogDate == model.BlogDate).ToList();
            }
            if (model.DanceStyleId != null)
            {
                model.Blogs = model.Blogs.Where(b => b.Styles.Where(s => s.Id == model.DanceStyleId).Count() != 0).ToList();
            }

            return View(model);
        }

        [Authorize(Roles ="Admin,Blogger")]
        [Route("Blog/PostBlog")]
        [HttpPost]
        public ActionResult PostBlog(BlogIndexViewModel model)
        {
            if (ModelState.IsValidField("NewBlog"))
            {
                dynamic obj = UploadImage(model.NewImage);
                model.NewImage = null;
                if (obj.UploadStatus == "Success")
                {
                    model.NewBlog.PhotoUrl = obj.FilePath;
                    DataContext.Blogs.Add(model.NewBlog);
                    DataContext.SaveChanges();
                    model.Message = "Blog Posted!";
                    model.PostStatus = "Success";
                    model.Message = "Your Blog was posted!";
                    return RedirectToAction("Index", model);
                }
                else
                {
                    model.PostStatus = "Failed";
                    model.Message = "Blog post failed!";
                    return RedirectToAction("Index", model);
                }
            }
            else
            {
                model.PostStatus = "Failed";
                model.Message = "Invalid Post!";
                return RedirectToAction("Index", model);
            }
        }

        [Authorize(Roles = "Admin,Blogger")]
        [Route("Blog/Delete")]
        public ActionResult Delete(int id)
        {
            var blog = DataContext.Blogs.Single(b => b.Id == id);
            EDR.Utilities.ApplicationUtility.DeletePicture(new Picture() { Filename = blog.PhotoUrl });
            DataContext.Blogs.Remove(DataContext.Blogs.Single(b => b.Id == id));
            DataContext.SaveChanges();
            return RedirectToAction("Index");
        }

        private object UploadImage(string imageData)
        {
            var newFile = new UploadFile();
            if (string.IsNullOrEmpty(imageData))
                newFile.UploadStatus = "Failed";

            Match imageMatch = Regex.Match(imageData, @"^data:(?<mimetype>[^;]+);base64,(?<data>.+)$");
            if (!imageMatch.Success)
                newFile.UploadStatus = "Failed";

            string mimeType = imageMatch.Groups["mimetype"].Value;
            Match imageType = Regex.Match(mimeType, @"^[^/]+/(?<type>.+?)$");
            if (!imageType.Success)
                newFile.UploadStatus = "Failed";

            string fileExtension = imageType.Groups["type"].Value;
            byte[] data2 = Convert.FromBase64String(imageMatch.Groups["data"].Value);

            if (newFile.UploadStatus != "Failed")
            {
                newFile = EDR.Utilities.ApplicationUtility.UploadFromPath(imageData);
            }
            var objUpload = new { FilePath = Url.Content(newFile.FilePath), UploadStatus = newFile.UploadStatus };
            return objUpload;
        }

    }
}
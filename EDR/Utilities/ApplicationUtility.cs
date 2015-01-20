using EDR.Models;
using Facebook;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;

namespace EDR.Utilities
{
    public static class ApplicationUtility
    {
        public static DateTime GetNextDate(DateTime start, Frequency frequency, int interval, DayOfWeek day, DateTime? today = null)
        {
            DateTime date = start;
            DateTime begin = (DateTime)(today == null ? DateTime.Today : today);

            if (start < begin)
            {
                TimeSpan diff = Convert.ToDateTime(begin.ToShortDateString()) - Convert.ToDateTime(start.ToShortDateString());
                switch (frequency)
                {
                    case Frequency.Daily:
                        date = start.AddDays(Convert.ToInt32(diff.TotalDays));
                        break;
                    case Frequency.Weekly:
                        var weekdiff = Convert.ToInt32(diff.TotalDays / 7 * interval);
                        if ((diff.TotalDays % (7 * interval)) > 0)
                        {
                            weekdiff += 1;
                        }
                        int totalIntervals = Convert.ToInt32(weekdiff);
                        date = start.AddDays(7 * interval * totalIntervals);
                        break;
                    case Frequency.Monthly:
                        var nth = Convert.ToInt32(start.Day / 7);
                        DateTime month = new DateTime(begin.Year, begin.Month, 1);
                        date = month.AddDays((((int)day - (int)month.DayOfWeek + 7) % 7)).AddDays(nth * 7);
                        break;
                    case Frequency.Yearly:
                        var yearNth = Convert.ToInt32(start.Day / 7);
                        DateTime yearMonth = new DateTime(start.AddYears(interval).Year, start.Month, 1);
                        date = yearMonth.AddDays((((int)day - (int)yearMonth.DayOfWeek + 7) % 7)).AddDays(yearNth * 7);
                        break;
                }
            }

            return (date);
        }

        public static int CalculateTime(int days, int hours, int minutes)
        {
            return ((days * 24 * 60) + (hours * 60) + minutes);
        }

        public static UserPicture GetNoProfilePicture()
        {
            var picture = new UserPicture() { Filename = "~/Content/images/NoProfilePic.gif", Title = "No Profile Picture", ThumbnailFilename = "~/Content/images/NoPicThumb.gif" };
            return picture;
        }

        public static UploadFile LoadPicture(HttpPostedFileBase file)
        {
            UploadFile newFile = new UploadFile();

            if (file != null && file.ContentLength > 0)
                try
                {
                    string title = Path.GetRandomFileName();
                    string filename = title + Path.GetExtension(file.FileName);
                    string filenameSmall = title + "_small" + Path.GetExtension(file.FileName);
                    string contentType = file.ContentType;
                    int size = file.ContentLength;
                    WebImage img = new WebImage(file.InputStream);

                    if (!Directory.Exists(HttpContext.Current.Server.MapPath("~/MyUploads")))
                    {
                        Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/MyUploads"));
                    }
                    img.Save(Path.Combine(HttpContext.Current.Server.MapPath("~/MyUploads"), filename));

                    img.Resize(130, 130, true, true);
                    img.Save(Path.Combine(HttpContext.Current.Server.MapPath("~/MyUploads"), filenameSmall));

                    newFile.FileName = title;
                    newFile.FilePath = "~/MyUploads/" + filename;
                    newFile.ThumbnailFilePath = "~/MyUploads/" + filenameSmall;
                    newFile.ContentType = contentType;
                    newFile.Size = size;
                    newFile.UploadStatus = "Success";
                }
                catch (Exception ex)
                {
                    newFile.UploadStatus = "ERROR:" + ex.Message.ToString();
                }
            else
            {
                newFile.UploadStatus = "You have not specified a file.";
            }
            return newFile;
        }

        public static string DeletePicture(Picture picture)
        {
            string result = "";
            try
            {
                if (picture != null || picture.Filename != string.Empty)
                {
                    FileInfo file = new FileInfo(HttpContext.Current.Server.MapPath(picture.Filename));
                    if (file.Exists)
                    {
                        file.Delete();
                    }
                    FileInfo thumb = new FileInfo(HttpContext.Current.Server.MapPath(picture.ThumbnailFilename));
                    if (thumb.Exists)
                    {
                        thumb.Delete();
                    }
                    result = "Success";
                }
            }
            catch (Exception ex)
            {
                result = "Failed!";
            }

            return result;
        }

        public static string FrequencyTranslate(Frequency frequency, int interval)
        {
            string freq = "";
            switch (frequency)
            {
                case Frequency.Daily:
                    if (interval > 1)
                    {
                        freq = "Days";
                    }
                    else
                    {
                        freq = "Day";
                    }
                    break;
                case Frequency.Weekly:
                    if (interval > 1)
                    {
                        freq = "Weeks";
                    }
                    else
                    {
                        freq = "Week";
                    }
                    break;
                case Frequency.Monthly:
                    if (interval > 1)
                    {
                        freq = "Months";
                    }
                    else
                    {
                        freq = "Month";
                    }
                    break;
                case Frequency.Yearly:
                    if (interval > 1)
                    {
                        freq = "Years";
                    }
                    else
                    {
                        freq = "Year";
                    }
                    break;
            }

            var message =
                "<span class=\"text-warning\">" +
                    "(*Repeats Every" +
                    (interval > 1 ? interval.ToString() : "") + freq +
                "</span>";
        return message;
        }
    }
}
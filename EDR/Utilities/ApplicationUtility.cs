using EDR.Models;
using Facebook;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using EDR.Enums;

namespace EDR.Utilities
{
    public static class ApplicationUtility
    {
        public static DateTime GetNextDate(DateTime start, Frequency frequency, int interval, DayOfWeek day, DateTime? today = null, string monthDays = null)
        {
            try
            {
                if (monthDays == null)
                {
                    monthDays = "1";
                }
                DateTime date = start;
                DateTime begin = (DateTime)(today == null ? DateTime.Today : today);
                int[] daysarray = Array.ConvertAll(monthDays.Split(new char[] { '-' }), d => int.Parse(d));

                if (start < begin)
                {
                    TimeSpan diff = Convert.ToDateTime(begin.ToShortDateString()) - Convert.ToDateTime(start.ToShortDateString());
                    switch (frequency)
                    {
                        case Frequency.Daily:
                            date = start.AddDays(Convert.ToInt32(diff.TotalDays));
                            break;
                        case Frequency.Weekly:
                            var weekdiff = Convert.ToInt32(diff.TotalDays) / 7 * interval;
                            if ((diff.TotalDays % (7 * interval)) > 0)
                            {
                                weekdiff += 1;
                            }
                            int totalIntervals = Convert.ToInt32(weekdiff);
                            date = start.AddDays(7 * interval * totalIntervals);
                            break;
                        case Frequency.Monthly:
                            var nth = Convert.ToInt32(start.Day / 7);

                            var dates = new List<DateTime>();
                            DateTime month = new DateTime(begin.Year, begin.Month, 1);
                            //  Calc days for Current Month
                            for (int i = 0; i < daysarray.Count(); i++)
                            {
                                dates.Add(month.AddDays((((int)day - (int)month.DayOfWeek + 7) % 7)).AddDays((daysarray[i] - 1) * 7));
                            }
                            //  Calc days for Next Month
                            for (int i = 0; i < daysarray.Count(); i++)
                            {
                                dates.Add(month.AddMonths(1).AddDays((((int)day - (int)month.AddMonths(1).DayOfWeek + 7) % 7)).AddDays((daysarray[i] - 1) * 7));
                            }
                            date = dates.Where(d => d > begin).OrderBy(d => d).FirstOrDefault();
                            //  date = month.AddDays((((int)day - (int)month.DayOfWeek + 7) % 7)).AddDays(nth * 7);
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
            catch(Exception ex)
            {
                return (DateTime.Today);
            }

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

        public static UploadFile UploadFromPath(string imageData)
        {
            UploadFile newFile = new UploadFile();
            try
            {
                //  string dataWithoutJpegMarker = imageData.Replace("data:png;base64,", String.Empty);
                newFile.FileName = Guid.NewGuid().ToString() + ".png";
                string imageDataClean = imageData.Split(',')[1];
                byte[] filebytes = Convert.FromBase64String(imageDataClean);
                string writePath = Path.Combine(HttpContext.Current.Server.MapPath("~/MyUploads"), newFile.FileName);
                using (FileStream fs = new FileStream(writePath,
                                                FileMode.OpenOrCreate,
                                                FileAccess.Write,
                                                FileShare.None))
                {
                    fs.Write(filebytes, 0, filebytes.Length);
                }
                newFile.FilePath = "~/MyUploads/" + newFile.FileName;
                newFile.UploadStatus = "Success";
                return newFile;
            }
            catch (Exception ex)
            {
                newFile.UploadStatus = "Failed";
                return newFile;
            }
            
            //UploadFile newFile = new UploadFile();
            //try
            //{
            //    newFile.FileName = DateTime.Now.ToString().Replace("/", "-").Replace(" ", "- ").Replace(":", "") + ".png";
            //    string fileNameWitPath = HttpContext.Current.Server.MapPath("~/MyUploads") + newFile.FileName;
            //    using (FileStream fs = new FileStream(fileNameWitPath, FileMode.Create))
            //    {
            //        using (BinaryWriter bw = new BinaryWriter(fs))
            //        {
            //            byte[] data = Convert.FromBase64String(imageData);
            //            bw.Write(data);
            //            bw.Close();
            //        }
            //    }

            //    newFile.FilePath = "~/MyUploads/" + newFile.FileName;
            //    newFile.UploadStatus = "Success";
            //    return newFile;
            //}
            //catch (Exception ex)
            //{
            //    var t = ex.Message;
            //    return newFile;
            //}
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

        /// <summary>
        /// Finds web and email addresses in a string and surrounds then with the appropriate HTML anchor tags 
        /// </summary>
        /// <param name="s"></param>
        /// <returns>String</returns>
        public static string WithActiveLinks(this string text)
        {
            if (text != null)
            {
                text = Regex.Replace(text,
                                @"((http|ftp|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?)",
                                "<a target='_blank' href='$1'>$1</a>");
            }
            return text; 
        }
    }
}
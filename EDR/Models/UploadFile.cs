using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EDR.Models
{
    public class UploadFile
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string ThumbnailFilePath { get; set; }
        public int Size { get; set; }
        public string ContentType { get; set; }
        public string UploadStatus { get; set; }
    }
}
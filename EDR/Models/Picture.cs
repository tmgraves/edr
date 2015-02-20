using EDR.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EDR.Models
{
    public class Picture : Entity
    {
        public string Title { get; set; }
        public string Filename { get; set; }
        public string ThumbnailFilename { get; set; }
        public DateTime PhotoDate { get; set; }
        public string SourceLink { get; set; }
        public MediaSource MediaSource { get; set; }
        public string FacebookId { get; set; }
        public string InstagramId { get; set; }
    }
}
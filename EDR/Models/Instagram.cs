using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EDR.Models
{
    public class Instagram
    {
    }

    public class InstagramPicture
    {
        public string InstagramId { get; set; }
        public string Thumbnail { get; set; }
        public string Photo { get; set; }
        public DateTime PhotoDate { get; set; }
        public string Caption { get; set; }
        public string Link { get; set; }
    }
}
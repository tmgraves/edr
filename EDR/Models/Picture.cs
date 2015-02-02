﻿using EDR.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EDR.Models
{
    public abstract class Picture : Entity
    {
        public string Title { get; set; }
        public string Filename { get; set; }
        public string ThumbnailFilename { get; set; }
        public DateTime PhotoDate { get; set; }
        public MediaSource MediaSource { get; set; }
    }
}
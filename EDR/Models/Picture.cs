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
        public Album Album { get; set; }
    }

    public class EventPicture : Picture
    {
        public Event Event { get; set; }
        public bool MainPicture { get; set; }
        [Required]
        public ApplicationUser PostedBy { get; set; }
    }

    public class Album : Entity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string CoverPhoto { get; set; }
        public string CoverThumbnail { get; set; }
        public DateTime AlbumDate { get; set; }
        public string SourceLink { get; set; }
        public MediaSource MediaSource { get; set; }
        public string FacebookId { get; set; }
        public int PhotoCount { get; set; }
        public virtual ICollection<Picture> Pictures { get; set; }
    }

    public class EventAlbum : Album
    {
        public Event Event { get; set; }
        [Required]
        public ApplicationUser PostedBy { get; set; }
    }
}
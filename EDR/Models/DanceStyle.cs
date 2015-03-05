using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EDR.Models
{
    public class DanceStyle : Entity
    {
        [Index("NameIndex", IsUnique = true)]
        [StringLength(50)]
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        public string YouTubeVideoID { get; set; }
        [Display(Name = "Cover Photo")]
        public string PhotoUrl { get; set; }
        public string SpotifyPlaylist { get; set; }

        public ICollection<ApplicationUser> Dancers { get; set; }
        public ICollection<Event> Events { get; set; }
        public ICollection<Series> Series { get; set; }
        public ICollection<Teacher> Teachers { get; set; }
        public ICollection<DanceStyleVideo> Videos { get; set; }
    }

    public class DanceStyleVideo : Video
    {
        public DanceStyle DanceStyle { get; set; }
    }

    public class DanceStyleListItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsSelected { get; set; }
        public object Tags { get; set; }
    }

    public class PostedStyles
    {
        //this array will be used to POST values from the form to the controller
        public string[] DanceStyleIds { get; set; }
    }
}
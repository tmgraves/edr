using EDR.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EDR.Models
{
    public class Rehearsal : Entity
    {
        [Required]
        public int TeamId { get; set; }
        [ForeignKey("TeamId")]
        public Team Team { get; set; }
        [Required]
        [Display(Name = "Day of Week")]
        public DayOfWeek Day { get; set; }
        [Required]
        [Display(Name = "Start Time")]
        public DateTime Time { get; set; }
        [Required]
        [Display(Name = "Location")]
        public int PlaceId { get; set; }
        [ForeignKey("PlaceId")]
        public Place Place { get; set; }
    }
}
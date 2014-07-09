using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EDR.Models
{
    public class Event : Entity
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Summary { get; set; }

        public string Description { get; set; }

        [Display(Name = "Open for Registration")]
        public bool IsAvailable { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EDR.Models
{
    public class EventPicture : Picture
    {
        public Event Event { get; set; }
        public bool MainPicture { get; set; }
        [Required]
        public ApplicationUser PostedBy { get; set; }
    }
}
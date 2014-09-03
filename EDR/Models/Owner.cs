﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace EDR.Models
{
    public class Owner : Entity
    {
        public string ContactEmail { get; set; }
        public string Website { get; set; }
        public string Facebook { get; set; }
        public bool? Approved { get; set; }
        public DateTime? ApproveDate { get; set; }

        [Required]
        public virtual ApplicationUser ApplicationUser { get; set; }

        public virtual ICollection<Place> Places { get; set; }
    }
}
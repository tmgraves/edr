using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EDR.Models
{
    public class Review : Entity
    {
        [Required]
        public virtual ApplicationUser Author { get; set; }

        [Required]
        public string ReviewText { get; set; }

        public int Rating { get; set; }

        public DateTime ReviewDate { get; set; }

        public virtual Series Series { get; set; }

        public virtual Event Event { get; set; }

        public virtual Teacher Teacher { get; set; }

        public virtual Place Place { get; set; }
    }
}
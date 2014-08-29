using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EDR.Models
{
    public class Promoter : Entity
    {
        public string ContactEmail { get; set; }
        public string FacebookLink { get; set; }
        public string Website { get; set; }

        [Required]
        public virtual ApplicationUser ApplicationUser { get; set; }

        public virtual ICollection<Concert> Concerts { get; set; }
        public virtual ICollection<Conference> Conferences { get; set; }
        public virtual ICollection<Social> Socials { get; set; }
        public virtual ICollection<Workshop> Workshops { get; set; }
    }
}
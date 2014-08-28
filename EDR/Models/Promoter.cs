using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EDR.Models
{
    public class Promoter : Entity
    {
        [Required]
        public ApplicationUser ApplicationUser { get; set; }
        public string ContactEmail { get; set; }
        public string FacebookLink { get; set; }
        public string Website { get; set; }

        public ICollection<Concert> Concerts { get; set; }
        public ICollection<Conference> Conferences { get; set; }
        public ICollection<Social> Socials { get; set; }
        public ICollection<Workshop> Workshops { get; set; }
    }
}
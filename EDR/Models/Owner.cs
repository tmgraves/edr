using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace EDR.Models
{
    public class Owner : Entity
    {
        [Required]
        public ApplicationUser ApplicationUser { get; set; }
        public string ContactEmail { get; set; }

        public ICollection<Place> Places { get; set; }
    }
}
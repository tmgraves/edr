using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace EDR.Models
{
    public class Dancer : Entity
    {
        [Required]
        public ApplicationUser ApplicationUser { get; set; }
        public int Experience { get; set; }

        public ICollection<DanceStyle> DanceStyles { get; set; }
        public ICollection<Event> Events { get; set; }
    }
}
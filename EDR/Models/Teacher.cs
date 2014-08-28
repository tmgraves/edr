using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EDR.Models
{
    public class Teacher : Entity
    {
        public int Experience { get; set; }
        public string Resume { get; set; }
        public string FacebookLink { get; set; }
        public string Website { get; set; }

        [Required]
        public ApplicationUser ApplicationUser { get; set; }

        public ICollection<DanceStyle> DanceStyles { get; set; }
        public ICollection<Class> Classes { get; set; }
        public ICollection<ClassSeries> ClassSeries { get; set; }
        public ICollection<Review> Reviews { get; set; }
        public ICollection<Workshop> Workshops { get; set; }
        public ICollection<Rehearsal> Rehearsals { get; set; }
    }
}
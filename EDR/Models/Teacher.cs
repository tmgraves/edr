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
        public virtual ApplicationUser ApplicationUser { get; set; }

        public virtual ICollection<DanceStyle> DanceStyles { get; set; }
        public virtual ICollection<Class> Classes { get; set; }
        public virtual ICollection<ClassSeries> ClassSeries { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
        public virtual ICollection<Workshop> Workshops { get; set; }
        public virtual ICollection<Rehearsal> Rehearsals { get; set; }
    }
}
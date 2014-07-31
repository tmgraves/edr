using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EDR.Models
{
    public class Teacher : ApplicationUser
    {
        public int Experience { get; set; }
        public string Resume { get; set; }

        public ICollection<DanceStyle> DanceStyles { get; set; }
        public ICollection<Class> Classes { get; set; }
        public ICollection<ClassSeries> ClassSeries { get; set; }
        public ICollection<Review> Reviews { get; set; }
    }
}
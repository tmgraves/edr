using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EDR.Models
{
    public class ClassSeries : Series
    {
        public int SkillLevel { get; set; }

        public string Prerequisite { get; set; }

        public ICollection<Teacher> Teachers { get; set; }
    }
}
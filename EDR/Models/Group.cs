using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EDR.Models
{
    public class Group
    {
        public string GroupName { get; set; }
        public string GroupDescription { get; set; }
        public int SkillLevel { get; set; }
        public string FacebookLink { get; set; }
        public bool Public { get; set; }
        public Nullable<int> ParentGroupID { get; set; }
    }
}
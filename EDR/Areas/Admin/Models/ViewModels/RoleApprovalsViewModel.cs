using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EDR.Models.ViewModels
{
    public class RoleApprovalsViewModel
    {
        public IEnumerable<Teacher> Teachers { get; set; }
        public IEnumerable<Owner> Owners { get; set; }
        public IEnumerable<Promoter> Promoters { get; set; }
    }
}
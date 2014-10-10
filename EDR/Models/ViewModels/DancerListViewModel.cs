using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EDR.Models.ViewModels
{
    public class DancerListViewModel
    {
        public IEnumerable<ApplicationUser> Dancers { get; set; }
    }
}
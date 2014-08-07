using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EDR.Models.ViewModels
{
    public class HomeExploreViewModel
    {
        public IEnumerable<DanceStyle> DanceStyles { get; set; }
    }
}
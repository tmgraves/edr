using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EDR.Models.ViewModels
{
    public class HomeIndexViewModel
    {
        public IEnumerable<DanceStyle> DanceStyles { get; set; }
        public IEnumerable<Class> Classes { get; set; }
    }
}
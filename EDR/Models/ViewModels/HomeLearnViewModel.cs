using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EDR.Models.ViewModels
{
    public class HomeLearnViewModel
    {
        public IEnumerable<ClassSeries> ClassSeries { get; set; }
        public IEnumerable<Class> Classes { get; set; }
        public IEnumerable<DanceStyle> DanceStyles { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EDR.Models.ViewModels
{
    public class DanceStyleDetailViewModel
    {
        public DanceStyle DanceStyle { get; set; }
        public IEnumerable<Class> Classes { get; set; }
    }
}
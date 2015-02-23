using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EDR.Models;

namespace EDR.Areas.Admin.Models.ViewModels
{
    public class DanceStyleBaseViewModels
    {
        public DanceStyle DanceStyle { get; set; }
    }

    public class DanceStyleFacebookVideosViewModel : DanceStyleBaseViewModels
    {
        public IEnumerable<FacebookVideo> Videos { get; set; }
    }
}
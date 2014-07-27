using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EDR.Models.ViewModels
{
    public class HomeIndexViewModel
    {
        public IEnumerable<Event> Events { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EDR.Models.ViewModels
{
    public class ClassCreateViewModel
    {
        public Class Class { get; set; }
        public SelectList PlaceList { get; set; }
        public IEnumerable<DanceStyleListItem> AvailableStyles { get; set; }
        public PostedStyles PostedStyles { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EDR.Models.ViewModels
{
    public class TeacherListViewModel
    {
        public IEnumerable<Teacher> Teachers { get; set; }
        public List<SelectListItem> DanceStyleList { get; set; }
    }
}
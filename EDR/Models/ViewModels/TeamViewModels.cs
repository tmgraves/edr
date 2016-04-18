using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EDR.Models.ViewModels
{
    public class TeamBaseViewModel
    {
        public Team Team { get; set; }
    }

    public class TeamViewViewModel : TeamBaseViewModel
    {

    }

    public class TeamIndexViewModel
    {
        public IEnumerable<Team> Teams { get; set; }
    }

    public class TeamManageViewModel : TeamBaseViewModel
    {

    }

    public class TeamCreateViewModel : TeamBaseViewModel
    {

    }

    public class TeamDeleteViewModel : TeamBaseViewModel
    {

    }
}
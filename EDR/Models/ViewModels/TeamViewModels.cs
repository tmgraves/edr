using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        public Rehearsal NewRehearsal { get; set; }
        [Range(1, 12)]
        public int RehearsalHour { get; set; }
        [Range(0, 59)]
        public int RehearsalMinute { get; set; }
        public string RehearsalAMPM { get; set; }
    }

    public class TeamCreateViewModel : TeamBaseViewModel
    {

    }

    public class TeamDeleteViewModel : TeamBaseViewModel
    {

    }
}
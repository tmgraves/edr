using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.ComponentModel;

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
        public Audition NewAudition { get; set; }
        public Performance NewPerformance { get; set; }
        public int NewStyleId { get; set; }
    }

    public class TeamCreateViewModel : TeamBaseViewModel
    {
        public TeamCreateViewModel()
        {
            Team = new Team();
        }
    }

    public class TeamDeleteViewModel : TeamBaseViewModel
    {

    }

    public class TeamDanceStylesPartialViewModel
    {
        public IEnumerable<DanceStyle> DanceStyles { get; set; }
        public int TeamId { get; set; }
    }
}
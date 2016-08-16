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
        public OrganizationMember Member { get; set; }
    }

    public class TeamIndexViewModel
    {
        //  Search Fields
        [Display(Name = "Teacher:")]
        public string TeacherId { get; set; }
        public string Teacher { get; set; }
        [Display(Name = "Dance Style:")]
        public int? DanceStyleId { get; set; }
        public string Style { get; set; }
        public Address SearchAddress { get; set; }
        public string Location { get; set; }
        [Display(Name = "Skill Level:")]
        public int? SkillLevel { get; set; }

        //  Map Settings
        public double? NELat { get; set; }
        public double? NELng { get; set; }
        public double? SWLat { get; set; }
        public double? SWLng { get; set; }
        public double? CenterLat { get; set; }
        public double? CenterLng { get; set; }
        public int? Zoom { get; set; }

        //  Results
        public IEnumerable<Team> Teams { get; set; }
    }

    public class NewRehearsal : Rehearsal
    {
        private DateTime _date = DateTime.Today;
        [Foolproof.LessThanOrEqualTo("StartDate", ErrorMessage ="Start Date must be greater than Today.")]
        public DateTime Today
        {
            get { return _date; }
        }
    }

    public class TeamManageViewModel : TeamBaseViewModel
    {
        public NewRehearsal NewRehearsal { get; set; }
        public Audition NewAudition { get; set; }
        public Performance NewPerformance { get; set; }
        public int NewStyleId { get; set; }
        public string NewMemberId { get; set; }
        public string NewFacebookEventId { get; set; }
        public Uri NewYoutubePlayList { get; set; }
        [RegularExpression("http[s]?://(www.youtube.com|youtu.?be)/.+$", ErrorMessage = "Please enter a valid YouTube link")]
        public Uri NewYouTubeVideo { get; set; }

        public TeamManageViewModel()
        {
            NewRehearsal = new NewRehearsal();
        }
    }

    public class TeamCreateViewModel : TeamBaseViewModel
    {
        public int TeacherId { get; set; }
        public int? SchoolId { get; set; }
        [Display(Name = "Facebook Page")]
        [RegularExpression("http[s]?://(www.facebook.com)/?[a-zA-Z0-9/\\-\\.]*", ErrorMessage = "Please enter a valid facebook page.")]
        public Uri FacebookLink { get; set; }
        public string FacebookId { get; set; }
        public string[] DanceStyleId { get; set; }
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
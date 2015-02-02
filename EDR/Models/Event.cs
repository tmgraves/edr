using EDR.Utilities;
using Microsoft.Linq.Translations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EDR.Models
{
    public class Event : Entity
    {
        [Required]
        public string Name { get; set; }
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }
        [Display(Name = "Facebook Page")]
        [RegularExpression("http[s]?://(www.facebook.com)/?[a-zA-Z0-9/\\-\\.]*", ErrorMessage = "Please enter a valid facebook page.")]
        public string FacebookLink { get; set; }
        public string FacebookId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? EndDate { get; set; }

        [DataType(DataType.Currency)]
        public Nullable<decimal> Price { get; set; }

        public bool IsAvailable { get; set; }

        [Required]
        public bool Recurring { get; set; }
        public Frequency Frequency { get; set; }
        public int? Interval { get; set; }
        public DayOfWeek Day { get; set; }
        public int Duration { get; set; }

        public string PhotoUrl { get; set; }

        [Display(Name = "Next Date")]
        public DateTime NextDate
        {
            get
            {
                if (Recurring)
                {
                    return ApplicationUtility.GetNextDate(StartDate, Frequency, (int)Interval, Day);
                }
                else
                {
                    return StartDate;
                }
            }
        }

        [Display(Name = "End Date/Time")]
        public DateTime EndDateTime
        {
            get
            {
                return NextDate.AddMinutes(Duration);
            }
        }

        public virtual Place Place { get; set; }
        public ApplicationUser Creator { get; set; }
        public ICollection<EventMember> EventMembers { get; set; }

        public ICollection<DanceStyle> DanceStyles { get; set; }

        public virtual Series Series { get; set; }

        public ICollection<Review> Reviews { get; set; }

        public ICollection<ApplicationUser> Users { get; set; }

        public ICollection<EventPicture> Pictures { get; set; }
        public ICollection<EventVideo> Videos { get; set; }
        public ICollection<EventPlaylist> Playlists { get; set; }
    }
}
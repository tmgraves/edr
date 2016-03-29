using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EDR.Models
{
    public class Teacher : Entity
    {
        [DataType(DataType.Date)]
        [Display(Name = "Started Teaching")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? StartDate { get; set; }
        public int Experience
        {
            get
            {
                return StartDate != null ? (int)((DateTime.Today - (DateTime)StartDate).Days / 365) : 0;
            }
        }
        public string Resume { get; set; }
        [Display(Name = "Facebook Page")]
        [RegularExpression("http[s]?://(www.facebook.com)/?[a-zA-Z0-9/\\-\\.]*", ErrorMessage = "Please enter a valid facebook page.")]
        public string FacebookLink { get; set; }
        [Display(Name = "Owner Website")]
        [Url(ErrorMessage = "Please enter a valid webiste address")]
        public string Website { get; set; }
        [Display(Name = "Contact Email")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string ContactEmail { get; set; }
        public bool? Approved { get; set; }
        public DateTime? ApproveDate { get; set; }

        [Required]
        public virtual ApplicationUser ApplicationUser { get; set; }

        public virtual ICollection<DanceStyle> DanceStyles { get; set; }
        public virtual ICollection<Class> Classes { get; set; }
        public virtual ICollection<ClassSeries> ClassSeries { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
        public virtual ICollection<Workshop> Workshops { get; set; }
        public virtual ICollection<Place> Places { get; set; }
        public virtual ICollection<Student> Students { get; set; }
        public ICollection<ClassTeacherInvitation> ClassTeacherInvitations { get; set; }
        public ICollection<School> Schools { get; set; }
    }
}
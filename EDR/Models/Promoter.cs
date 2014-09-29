using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EDR.Models
{
    public class Promoter : Entity
    {
        [Display(Name = "Contact Email")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string ContactEmail { get; set; }
        [Display(Name = "Facebook Page")]
        [RegularExpression("http[s]?://(www.facebook.com)/?[a-zA-Z0-9/\\-\\.]*", ErrorMessage = "Please enter a valid facebook page.")]
        public string Facebook { get; set; }
        [Display(Name = "Owner Website")]
        [Url(ErrorMessage = "Please enter a valid webiste address")]
        public string Website { get; set; }
        public bool? Approved { get; set; }
        public DateTime? ApproveDate { get; set; }

        [Required]
        public virtual ApplicationUser ApplicationUser { get; set; }

        public virtual ICollection<Event> Events { get; set; }
    }
}
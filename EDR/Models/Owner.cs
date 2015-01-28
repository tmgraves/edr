using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace EDR.Models
{
    public class Owner : Entity
    {
        [Display(Name = "Contact Email")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string ContactEmail { get; set; }
        [Display(Name = "Owner Website")]
        [Url(ErrorMessage = "Please enter a valid webiste address")]
        public string Website { get; set; }
        [Display(Name = "Facebook Page")]
        [RegularExpression("http[s]?://(www.facebook.com)/?[a-zA-Z0-9/\\-\\.]*", ErrorMessage = "Please enter a valid facebook page.")]
        public string Facebook { get; set; }
        public bool? Approved { get; set; }
        public DateTime? ApproveDate { get; set; }

        [Required]
        public virtual ApplicationUser ApplicationUser { get; set; }
        public virtual ICollection<Place> Places { get; set; }
        public ICollection<PlaceOwnerInvitation> PlaceOwnerInvitations { get; set; }
        public virtual ICollection<Class> Classes { get; set; }
        public virtual ICollection<Social> Socials { get; set; }
    }
}
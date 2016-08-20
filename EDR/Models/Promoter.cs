using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EDR.Models
{
    public class Promoter : Entity
    {
        public string Resume { get; set; }
        [Required(ErrorMessage = "Contact Email is required")]
        [Display(Name = "Contact Email")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string ContactEmail { get; set; }
        [StringLength(24)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:###-###-####}")]
        public string Phone { get; set; }
        [Display(Name = "Facebook Page")]
        [RegularExpression("http[s]?://(www.facebook.com)/?[a-zA-Z0-9/\\-\\.]*", ErrorMessage = "Please enter a valid facebook page.")]
        public string Facebook { get; set; }
        [Display(Name = "Owner Website")]
        [Url(ErrorMessage = "Enter a valid website(e.g. https://www.google.com)")]
        public string Website { get; set; }
        public bool? Approved { get; set; }
        public DateTime? ApproveDate { get; set; }

        [Required]
        public virtual ApplicationUser ApplicationUser { get; set; }

        public virtual ICollection<Social> Socials { get; set; }
        public virtual ICollection<Place> Places { get; set; }
        public ICollection<SocialPromoterInvitation> SocialPromoterInvitations { get; set; }
        public virtual ICollection<DanceStyle> DanceStyles { get; set; }
        public virtual ICollection<Feed> Feeds { get; set; }
        public virtual ICollection<PromoterGroup> PromoterGroups { get; set; }
    }

    public class PromoterGroup : Organization
    {
        public virtual ICollection<Promoter> Promoters { get; set; }
        public ICollection<Social> Socials { get; set; }
        public virtual ICollection<FinancialTransaction> FinancialTransactions { get; set; }
    }
}
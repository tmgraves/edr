using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace EDR.Models.ViewModels
{
    public class PromoterApplyViewModel
    {
        public Promoter Promoter { get; set; }
        [Display(Name = "Click here to agree to the Site Terms.")]
        [RegularExpression("True|true", ErrorMessage = "***You must agree to the site terms")]
        public bool TermsAndConditions { get; set; }
    }
}
using System;
using System.ComponentModel.DataAnnotations;

namespace EDR.Enums
{
    public enum SocialType
    {
        [Display(Name = "Social")]
        Social,
        [Display(Name = "Concert")]
        Concert,
        [Display(Name = "Conference")]
        Conference,
        [Display(Name = "OpenHouse")]
        OpenHouse,
        [Display(Name = "Party")]
        Party
    }
}
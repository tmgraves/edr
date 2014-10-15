using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EDR.Enums
{
    public enum PlaceType
    {
        [Display(Name = "Conference Center")]
        ConferenceCenter,
        [Display(Name = "Hotel")]
        Hotel,
        [Display(Name = "Nightclub")]
        Nightclub,
        [Display(Name = "Other Place")]
        OtherPlace,
        [Display(Name = "Restaurant")]
        Restaurant,
        [Display(Name = "Studio")]
        Studio,
        [Display(Name = "Theater")]
        Theater
    }
}
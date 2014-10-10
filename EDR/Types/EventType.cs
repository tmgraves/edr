using System;
using System.ComponentModel.DataAnnotations;

namespace EDR.Enums
{
    public enum EventType
    {
        [Display(Name = "Concert")]
        Concert,
        [Display(Name = "Conference")]
        Conference,
        [Display(Name = "Social")]
        Social,
        [Display(Name = "Party")]
        Party,
        [Display(Name = "Rehearsal")]
        Rehearsal,
        [Display(Name = "Workshop")]
        Workshop,
        [Display(Name = "Class")]
        Class
    }
}
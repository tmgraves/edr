using System;
using System.ComponentModel.DataAnnotations;

namespace EDR.Enums
{
    public enum ClassType
    {
        [Display(Name = "Class")]
        Class,
        [Display(Name = "Workshop")]
        Workshop
    }
}
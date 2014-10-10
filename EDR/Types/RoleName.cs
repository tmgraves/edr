using System;
using System.ComponentModel.DataAnnotations;

namespace EDR.Enums
{
    public enum RoleName
    {
        [Display(Name = "Teacher")]
        Teacher,
        [Display(Name = "Owner")]
        Owner,
        [Display(Name = "Promoter")]
        Promoter
    }
}
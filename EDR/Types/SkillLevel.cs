using System;
using System.ComponentModel.DataAnnotations;

namespace EDR.Enums
{
    public enum SkillLevel
    {
        [Display(Name = "Beginner")]
        Beginner,
        [Display(Name = "Beginner/Intermediate")]
        BeginnerIntermediate,
        [Display(Name = "Intermediate")]
        Intermediate,
        [Display(Name = "Intermediate/Advanced")]
        IntermediateAdvanced,
        [Display(Name = "Advanced")]
        Advanced
    }
}
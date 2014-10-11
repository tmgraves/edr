using System;
using System.ComponentModel.DataAnnotations;

namespace EDR.Enums
{
    public enum SkillLevel
    {
        [Display(Name = "Beginner")]
        Beginner=1,
        [Display(Name = "Beginner/Intermediate")]
        BeginnerIntermediate=2,
        [Display(Name = "Intermediate")]
        Intermediate=3,
        [Display(Name = "Intermediate/Advanced")]
        IntermediateAdvanced=4,
        [Display(Name = "Advanced")]
        Advanced=5
    }
}
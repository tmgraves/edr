using System.ComponentModel.DataAnnotations;

namespace EDR.Models
{
    public class Rehearsal : Event
    {
        [Display(Name = "Open to Public")]
        public bool IsPublic { get; set; }
    }
}
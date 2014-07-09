using System.ComponentModel.DataAnnotations;

namespace EDR.Models
{
    public class Concert : Event
    {
        [Display(Name = "Artist Name")]
        public string ArtistName { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;

namespace EDR.Models
{
    public abstract class Entity
    {
        [Key]
        [ScaffoldColumn(false)]
        public int Id { get; set; }
    }
}
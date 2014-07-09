using System.ComponentModel.DataAnnotations;

namespace EDR.Models
{
    public abstract class Entity
    {
        [Key]
        public int Id { get; set; }
    }
}
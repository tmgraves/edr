using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EDR.Models
{
    public class Student : Entity
    {
        [Required]
        [Index("IX_Student", 1, IsUnique = true)]
        public int TeacherId { get; set; }
        [ForeignKey("TeacherId")]
        public Teacher Teacher { get; set; }
        [Required]
        [Index("IX_Student", 2, IsUnique = true)]
        public string DancerId { get; set; }
        [ForeignKey("DancerId")]
        public ApplicationUser Dancer { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EDR.Models
{
    public abstract class Event : Entity
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Summary { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Nullable<decimal> Price { get; set; }
        public bool IsAvailable { get; set; }
    }
}
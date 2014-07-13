using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EDR.Models
{
    public class Event : Entity
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Summary { get; set; }
        public string Description { get; set; }
        public bool IsAvailable { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool AllDay { get; set; }
        public string Link { get; set; }
        public Nullable<decimal> Cost { get; set; }
        public Nullable<int> ParentEventID { get; set; }
        public Nullable<int> Frequency { get; set; }
        public string DaysOfWeek { get; set; }
        public Nullable<int> GroupID { get; set; }
    }
}
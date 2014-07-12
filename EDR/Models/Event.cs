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

        [Display(Name = "Open for Registration")]
        public bool IsAvailable { get; set; }

        public System.DateTime StartDate { get; set; }
        public System.DateTime EndDate { get; set; }
        public Nullable<System.TimeSpan> StartTime { get; set; }
        public Nullable<System.TimeSpan> EndTime { get; set; }
        public bool AllDay { get; set; }
        public string Link { get; set; }
        public Nullable<decimal> Cost { get; set; }
        public string LocationType { get; set; }
        public string Address { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Country { get; set; }
        public Nullable<int> ParentEventID { get; set; }
        public Nullable<int> Frequency { get; set; }
        public string DaysOfWeek { get; set; }
        public Nullable<int> GroupID { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.EnterpriseServices;
using System.Linq;
using System.Web;
using EDR.Enums;

namespace EDR.Models
{
    public class Series : Entity
    {
        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime EndDate { get; set; }

        public Frequency Frequency { get; set; }

        public DayOfWeek Day { get; set; }

        public TimeSpan Time { get; set; }

        public TimeSpan EndTime { get; set; }

        [DataType(DataType.Currency)]
        public Nullable<decimal> Price { get; set; }

        public bool IsAvailable { get; set; }

        public virtual Place Place { get; set; }

        public ICollection<DanceStyle> DanceStyles { get; set; }

        public ICollection<Event> Events { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EDR.Models
{
    public class EventMember : Entity
    {
        [Required]
        [Index("IX_EventMembers", 1, IsUnique = true)]
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public ApplicationUser Member { get; set; }
        [Required]
        [Index("IX_EventMembers", 2, IsUnique = true)]
        public int EventId { get; set; }
        [ForeignKey("EventId")]
        public Event Event { get; set; }
        private DateTime _date = DateTime.Now;
        public DateTime Joined
        {
            get { return _date; }
            set { _date = value; }
        }
    }
}
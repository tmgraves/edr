using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EDR.Models.ViewModels
{
    public class EventSignupViewModel
    {
        public int EventId { get; set; }
        public string UserId { get; set; }
        public Event Event { get; set; }
    }
}
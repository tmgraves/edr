using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EDR.Models.ViewModels
{
    public class EventReviewViewModel
    {
        public int EventId { get; set; }

        public Review Review { get; set; }
    }
}
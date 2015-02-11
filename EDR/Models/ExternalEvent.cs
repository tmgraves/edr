using EDR.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EDR.Models
{
    public class ExternalEvent
    {
        public string Id { get; set; }
        public MediaSource MediaSource { get; set; }
        public string Url { get; set; }
        public string Name { get; set; }
        public virtual Event Event { get; set; }
    }
}
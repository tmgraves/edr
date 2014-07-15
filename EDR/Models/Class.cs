using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EDR.Models
{
    public class Class : Event
    {
        public virtual DanceStyle DanceStyle { get; set; }
    }
}
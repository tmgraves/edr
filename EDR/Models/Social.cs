﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EDR.Models
{
    public class Social : Event
    {
        public ICollection<Promoter> Promoters { get; set; }
    }
}
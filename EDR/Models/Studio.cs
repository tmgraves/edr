﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EDR.Models
{
    public class Studio : Place
    {
        public ICollection<School> Schools { get; set; }
    }
}
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EDR.Models
{
    public class DanceStyle : Entity
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public ICollection<Class> Classes { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EDR.Models
{
    [Table("Teams")]
    public class Team : Group
    {
        public string TeamManagerName { get; set; }
    }
}
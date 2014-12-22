using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EDR.Models
{
    public class Friend : Entity
    {
        public ApplicationUser User { get; set; }
        public ApplicationUser FriendUser { get; set; }
    }
}
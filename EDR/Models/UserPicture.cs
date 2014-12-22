using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EDR.Models
{
    public class UserPicture : Picture
    {
        public ApplicationUser User { get; set; }
        public bool ProfilePicture { get; set; }
    }
}
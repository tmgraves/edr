using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EDR.Models.ViewModels
{
    public class DancerViewViewModel
    {
        // TODO: FILL IN PROPERTIES NEEDED FOR VIEW

        public ApplicationUser Dancer { get; set; }
        public IEnumerable<FacebookFriendViewModel> FriendList { get; set; }
    }
}
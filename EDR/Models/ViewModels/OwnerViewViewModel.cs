using EDR.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EDR.Models.ViewModels
{
    public class OwnerViewViewModel
    {
        // TODO: FILL IN PROPERTIES NEEDED FOR VIEW

        public Owner Owner { get; set; }
        public Address Address { get; set; }
        public List<RoleName> Roles { get; set; }
    }
}
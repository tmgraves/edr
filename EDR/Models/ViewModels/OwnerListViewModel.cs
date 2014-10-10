using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EDR.Models.ViewModels
{
    public class OwnerListViewModel
    {
        public IEnumerable<Owner> Owners { get; set; }
    }
}
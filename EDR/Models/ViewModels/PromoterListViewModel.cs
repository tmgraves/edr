using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EDR.Models.ViewModels
{
    public class PromoterListViewModel
    {
        public IEnumerable<Promoter> Promoters { get; set; }
    }
}
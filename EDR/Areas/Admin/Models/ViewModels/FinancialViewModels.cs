using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDR.Models;

namespace EDR.Areas.Admin.Models.ViewModels
{
    public class FinancialViewModel
    {
        public int[] PartnerIds { get; set; }
        public decimal? AmountDue { get; set; }
        public List<Organization> Partners { get; set; }
        public List<PaymentBatch> PaymentBatches { get; set; }
    }
}

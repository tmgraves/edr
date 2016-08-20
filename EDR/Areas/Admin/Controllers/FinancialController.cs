using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EDR.Areas.Admin.Models.ViewModels;
using EDR.Controllers;
using EDR.Models;
using EDR.Models.ViewModels;
using System.Data.Entity;

namespace EDR.Areas.Admin.Controllers
{
    public class FinancialController : BaseController
    {
        // GET: Admin/Financial
        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            var model = new FinancialViewModel();
            model.Partners = new List<Organization>();
            model.Partners.AddRange(DataContext.Schools.Include("FinancialTransactions"));
            model.Partners.AddRange(DataContext.PromoterGroups.Include("FinancialTransactions"));
            model.PaymentBatches = DataContext.PaymentBatches.ToList();
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult Index(FinancialViewModel model)
        {
            var x = model.PartnerIds;

            model.Partners = new List<Organization>();
            var schools = DataContext.Schools.Include("FinancialTransactions").AsEnumerable();
            if (model.AmountDue != null)
            {
                schools = schools.AsQueryable().Where(s => s.FinancialTransactions.Sum(t => t.Amount) >= model.AmountDue);
            }
            model.Partners.AddRange(schools);

            var promotergroups = DataContext.PromoterGroups.Include("FinancialTransactions").AsEnumerable();
            if (model.AmountDue != null)
            {
                promotergroups = promotergroups.AsQueryable().Where(s => s.FinancialTransactions.Sum(t => t.Amount) >= model.AmountDue);
            }
            model.Partners.AddRange(promotergroups);
            model.PaymentBatches = DataContext.PaymentBatches.ToList();
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult CreateBatch()
        {
            var batch = new PaymentBatch();
            batch.FinancialTransactions = new List<FinancialTransaction>();
            var partners = DataContext.Schools.Include("FinancialTransactions").Where(s => s.FinancialTransactions.Sum(f => f.Amount) >= GlobalVariables.PaymentThreshold);

            if (partners.Count() > 0)
            {
                foreach (var p in DataContext.Schools.Include("FinancialTransactions").Where(s => s.FinancialTransactions.Sum(f => f.Amount) >= GlobalVariables.PaymentThreshold))
                {
                    var tran = new FinancialTransaction() { Amount = -p.FinancialTransactions.Sum(f => f.Amount), SchoolId = p.Id, PaymentType = Enums.PaymentType.BillPay, TranType = "Distribution" };
                    //  DataContext.FinancialTransactions.Add(tran);
                    batch.FinancialTransactions.Add(tran);
                }
                foreach (var p in DataContext.PromoterGroups.Include("FinancialTransactions").Where(s => s.FinancialTransactions.Sum(f => f.Amount) >= GlobalVariables.PaymentThreshold))
                {
                    var tran = new FinancialTransaction() { Amount = -p.FinancialTransactions.Sum(f => f.Amount), PromoterId = p.Id, PaymentType = Enums.PaymentType.BillPay, TranType = "Distribution" };
                    //  DataContext.FinancialTransactions.Add(tran);
                    batch.FinancialTransactions.Add(tran);
                }
                DataContext.PaymentBatches.Add(batch);
                DataContext.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        public ActionResult BatchTransactionsPartial(int batchId)
        {
            var model = DataContext.FinancialTransactions.Where(t => t.PaymentBatchId == batchId).ToList();
            return PartialView("~/Areas/Admin/Views/Shared/_TransactionsPartial.cshtml", model);
            //  Media Updates

        }

        public ActionResult PartnerTransactionsPartial(int? promotergroupId, int? schoolId)
        {
            if (promotergroupId != null)
            {
                var model = DataContext.FinancialTransactions.Where(t => t.PromoterGroupId == promotergroupId).ToList();
                return PartialView("~/Areas/Admin/Views/Shared/_TransactionsPartial.cshtml", model);
            }
            else
            {
                var model = DataContext.FinancialTransactions.Where(t => t.SchoolId == schoolId).ToList();
                return PartialView("~/Areas/Admin/Views/Shared/_TransactionsPartial.cshtml", model);
            }
            //  Media Updates

        }

        public ActionResult CommitBatch(int batchId)
        {
            var now = DateTime.Now;
            var batch = DataContext.PaymentBatches.Where(b => b.Id == batchId).FirstOrDefault();
            if (batch != null && batch.CommitDate == null)
            {
                DataContext.FinancialTransactions.Where(t => t.PaymentBatchId == batchId).ToList().ForEach(t => t.Committed = now);
                batch.CommitDate = now;
                batch.PaymentType = Enums.PaymentType.BillPay;
                DataContext.Entry(batch).State = EntityState.Modified;
                DataContext.SaveChanges();
            }
            return RedirectToAction("Index");
        }
        public ActionResult DeleteBatch(int batchId)
        {
            var batch = DataContext.PaymentBatches.Single(b => b.Id == batchId);
            if (batch != null)
            {
                DataContext.FinancialTransactions.RemoveRange(batch.FinancialTransactions);
                batch.FinancialTransactions.Clear();
                DataContext.PaymentBatches.Remove(batch);
                DataContext.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}
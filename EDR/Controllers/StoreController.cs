using EDR.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EDR.Controllers
{
    public class StoreController : BaseController
    {
        // GET: /StoreItems/
        public ActionResult Index()
        {
            // Set up our ViewModel
            var viewModel = new StoreItemsViewModel
            {
                DancePacks = DataContext.DancePacks.ToList()
            };
            // Return the view
            return View(viewModel);
        }
    }
}
using EDR.Models.ViewModels;
using System.Linq;
using System.Web.Mvc;

namespace EDR.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            var classes = DataContext.Classes.Where(x => x.IsAvailable == true).ToList();

            return View(classes);
        }
    }
}
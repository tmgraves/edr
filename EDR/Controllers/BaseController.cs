using EDR.Data;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace EDR.Controllers
{
    public class BaseController : Controller
    {
        private ApplicationDbContext _dataContext;
        private ApplicationUserManager _userManager;

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        public ApplicationDbContext DataContext
        {
            get
            {
                return _dataContext ?? HttpContext.GetOwinContext().Get<ApplicationDbContext>();
            }
            private set
            {
                _dataContext = value;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && DataContext != null)
            {
                DataContext.Dispose();
                DataContext = null;
            }

            if (disposing && UserManager != null)
            {
                UserManager.Dispose();
                UserManager = null;
            }
            base.Dispose(disposing);
        }
    }
}
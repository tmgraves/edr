using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace EDR.Attributes
{
    public class AccessDeniedAuthorizeAttribute : AuthorizeAttribute
    {
        public string AccessDeniedController { get; set; }
        public string AccessDeniedAction { get; set; }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);

            if (filterContext.HttpContext.User.Identity.IsAuthenticated &&
                filterContext.Result is HttpUnauthorizedResult)
            {
                if (String.IsNullOrWhiteSpace(AccessDeniedController) || String.IsNullOrWhiteSpace(AccessDeniedAction))
                {
                    AccessDeniedController = "Account";
                    AccessDeniedAction = "Login";
                }

                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { Controller = AccessDeniedController, Action = AccessDeniedAction }));
            }
        }
    }   
}
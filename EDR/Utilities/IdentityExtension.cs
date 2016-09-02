using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EDR.Models;
using System.Threading.Tasks;
using System.Security.Principal;
using System.Security.Claims;

namespace EDR.Utilities
{
    public static class IdentityExtensions
    {
        public static async Task<ApplicationUser> FindByNameOrEmailAsync
            (this UserManager<ApplicationUser> userManager, string usernameOrEmail, string password)
        {
            var username = usernameOrEmail;
            if (usernameOrEmail.Contains("@"))
            {
                var userForEmail = await userManager.FindByEmailAsync(usernameOrEmail);
                if (userForEmail != null)
                {
                    username = userForEmail.UserName;
                }
            }
            return await userManager.FindAsync(username, password);
        }

        public static string GetPhotoUrl(this IIdentity identity)
        {
            var claim = ((ClaimsIdentity)identity).FindFirst("PhotoUrl");
            // Test for null to avoid issues during local testing
            return (claim != null) ? claim.Value : string.Empty;
        }

        public static string GetFirstName(this IIdentity identity)
        {
            var claim = ((ClaimsIdentity)identity).FindFirst("FirstName");
            // Test for null to avoid issues during local testing
            return (claim != null) ? claim.Value : string.Empty;
        }

        public static string GetLastName(this IIdentity identity)
        {
            var claim = ((ClaimsIdentity)identity).FindFirst("LastName");
            // Test for null to avoid issues during local testing
            return (claim != null) ? claim.Value : string.Empty;
        }

        public static bool GetNewPassword(this IIdentity identity)
        {
            var claim = ((ClaimsIdentity)identity).FindFirst("NewPassword");
            // Test for null to avoid issues during local testing
            return (claim != null) ? Convert.ToBoolean(claim.Value) : false;
        }
    }
}
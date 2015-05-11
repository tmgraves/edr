using EDR.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EDR.Areas.Admin.Models.ViewModels
{
    public class RolesViewModel
    {
        [Display(Name = "Role")]
        public IdentityRole Role { get; set; }
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        public List<IdentityRole> Roles { get; set; }
        public IEnumerable<ApplicationUser> Users { get; set; }
    }

    public class AddUserViewModel
    {
        public string RoleId { get; set; }
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        public List<IdentityRole> Roles { get; set; }
        public IEnumerable<ApplicationUser> SearchUsers { get; set; }
    }

    public class NewRoleUserViewModel
    {
        public string RoleId { get; set; }
        public string UserId { get; set; }
    }
}
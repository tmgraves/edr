using Microsoft.AspNet.Facebook;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EDR.Models.ViewModels
{
    public class FacebookFriendViewModel
    {
        [Required]
        [Display(Name = "Friend's name")]
        public string Name { get; set; }
        public string Id { get; set; }
        public string Link { get; set; }
        public string ImageURL { get; set; }
        public string Email { get; set; }
    }
}
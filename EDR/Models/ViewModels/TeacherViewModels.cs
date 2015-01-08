using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EDR.Models.ViewModels
{
    public class TeacherViewViewModel
    {
        public Teacher Teacher { get; set; }
        public Address Address { get; set; }
        public ClassNewViewModel NewClassModel { get; set; }
    }
}
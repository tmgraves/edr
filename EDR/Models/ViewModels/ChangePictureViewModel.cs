using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EDR.Models.ViewModels
{
    public class ChangePictureViewModel
    {
        public ApplicationUser Dancer { get; set; }
        public IEnumerable<FacebookPhoto> FacebookPictures { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace EDR.Models.ViewModels
{
    public class BlogIndexViewModel
    {
        public string PostStatus { get; set; }
        public string Message { get; set; }
        public List<Blog> Blogs { get; set; }
        [Display(Name = "Location:")]
        public string Location { get; set; }
        public double? Lat { get; set; }
        public double? Lng { get; set; }
        [Display(Name = "Blog Date:")]
        public DateTime? BlogDate { get; set; }
        [Display(Name = "Dance Style:")]
        public int? DanceStyleId { get; set; }
        public string Style { get; set; }
        public Blog NewBlog { get; set; }
        public string NewImage { get; set; }

        public BlogIndexViewModel()
        {
            Blogs = new List<Blog>();
            NewBlog = new Blog();
        }
    }
}

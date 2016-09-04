using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EDR.Models
{
    public class Blog : Entity
    {
        [Required]
        [Display(Name ="Title")]
        public string Title { get; set; }
        [Required]
        [Display(Name = "Description")]
        public string Description { get; set; }
        public string PhotoUrl { get; set; }
        private DateTime _blogdate = DateTime.Now;
        public DateTime BlogDate
        {
            get { return _blogdate; }
            set { _blogdate = value; }
        }
        [Required]
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual ApplicationUser Author { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string State { get; set; }
        [Required]
        public string Country { get; set; }
        [Required]
        public double Latitude { get; set; }
        [Required]
        public double Longitude { get; set; }
        public virtual ICollection<DanceStyle> Styles { get; set; }
        public int? EventId { get; set; }
        [ForeignKey("EventId")]
        public virtual Event Event { get; set; }
    }

    public class BlogReply : Entity
    {
        [Required]
        public int BlogId { get; set; }
        [ForeignKey("BlogId")]
        public Blog Blog { get; set; }
        [Required]
        public string Text { get; set; }
        private DateTime _replydate = DateTime.Now;
        public DateTime ReplyDate
        {
            get { return _replydate; }
            set { _replydate = value; }
        }
        [Required]
        public ApplicationUser Author { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string State { get; set; }
        [Required]
        public string Country { get; set; }
    }
}

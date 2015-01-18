using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EDR.Models
{
    public class ClassTeacherInvitation: Entity
    {
        [Required]
        [Index("IX_ClassTeacherInvitation", 1, IsUnique = true)]
        public int ClassId { get; set; }
        [ForeignKey("ClassId")]
        public Class Class { get; set; }

        [Required]
        [Index("IX_ClassTeacherInvitation", 2, IsUnique = true)]
        public int TeacherId { get; set; }
        [ForeignKey("TeacherId")]
        public Teacher Teacher { get; set; }

        public bool? Approved { get; set; }
    }

    public class SocialPromoterInvitation : Entity
    {
        [Required]
        [Index("IX_SocialPromoterInvitation", 1, IsUnique = true)]
        public int SocialId { get; set; }
        [ForeignKey("SocialId")]
        public Social Social { get; set; }

        [Required]
        [Index("IX_SocialPromoterInvitation", 2, IsUnique = true)]
        public int PromoterId { get; set; }
        [ForeignKey("PromoterId")]
        public Teacher Promoter { get; set; }

        public bool? Approved { get; set; }
    }

    public class PlaceOwnerInvitation : Entity
    {
        [Required]
        [Index("IX_PlaceOwnerInvitation", 1, IsUnique = true)]
        public int PlaceId { get; set; }
        [ForeignKey("PlaceId")]
        public Place Place { get; set; }

        [Required]
        [Index("IX_PlaceOwnerInvitation", 2, IsUnique = true)]
        public int OwnerId { get; set; }
        [ForeignKey("OwnerId")]
        public Owner Owner { get; set; }
        public bool? Approved { get; set; }
    }
}
using EDR.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EDR.Models
{
    public class Social : Event
    {
        public SocialType SocialType { get; set; }
        public virtual ICollection<Promoter> Promoters { get; set; }
        public ICollection<SocialPromoterInvitation> SocialPromoterInvitations { get; set; }
        public ICollection<Owner> Owners { get; set; }
        public MusicType MusicType { get; set; }
        public int? PromoterGroupId { get; set; }
        [ForeignKey("PromoterGroupId")]
        public virtual PromoterGroup PromoterGroup { get; set; }
    }
}
using EDR.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EDR.Models
{
    public class Social : Event
    {
        public SocialType SocialType { get; set; }
        public ICollection<Promoter> Promoters { get; set; }
        public ICollection<SocialPromoterInvitation> SocialPromoterInvitations { get; set; }
        public ICollection<Owner> Owners { get; set; }
        public MusicType MusicType { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EDR.Models
{
    public abstract class Group : Entity
    {
        public string GroupName { get; set; }
        public string GroupDescription { get; set; }
        public int SkillLevel { get; set; }
        public string FacebookLink { get; set; }
        public bool Public { get; set; }
        public Nullable<int> ParentGroupID { get; set; }
    }

    [Table("Teams")]
    public class Team : Group
    {
        public string TeamManagerName { get; set; }
        public ICollection<Rehearsal> Rehearsals { get; set; }
    }

    public class School : Group
    {
        public ICollection<Teacher> Teachers { get; set; }
        public ICollection<Studio> Studios { get; set; }
    }

    public class OrganizationMember : Entity
    {
        [Required]
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
        public bool Admin { get; set; }
    }
}
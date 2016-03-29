using EDR.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EDR.Models
{
    public class Class : Event
    {
        public int SkillLevel { get; set; }
        public string Prerequisite { get; set; }
        public ClassType ClassType { get; set; }
        [Required]
        public int SchoolId { get; set; }
        [ForeignKey("SchoolId")]
        public School School { get; set; }
        public ICollection<Teacher> Teachers { get; set; }
        public ICollection<ClassTeacherInvitation> ClassTeacherInvitations { get; set; }
        public ICollection<Owner> Owners { get; set; }
    }
}
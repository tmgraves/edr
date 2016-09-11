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
        public SkillLevel SkillLevel { get; set; }
        public string Prerequisite { get; set; }
        public ClassType ClassType { get; set; }
        [Required]
        public int SchoolId { get; set; }
        [ForeignKey("SchoolId")]
        public virtual School School { get; set; }
        public virtual ICollection<Teacher> Teachers { get; set; }
        public ICollection<ClassTeacherInvitation> ClassTeacherInvitations { get; set; }
        public ICollection<Owner> Owners { get; set; }

        public Class()
        {
            Teachers = new List<Teacher>();
            ClassTeacherInvitations = new List<ClassTeacherInvitation>();
            Owners = new List<Owner>();
        }
    }
}
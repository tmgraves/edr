using EDR.Enums;
using EDR.Utilities.Validators;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EDR.Models.ViewModels
{
    //public class ViewTicketViewModel
    //{
    //    public Ticket Ticket { get; set; }

    //    public IEnumerable<ListItem> AvailableClasses { get; set; }
    //    public IEnumerable<ListItem> SelectedClasses { get; set; }
    //    [RequiredStringArrayValue(ErrorMessage = "Select at least (1) Class")]
    //    public string[] PostedClasses { get; set; }
    //    public List<EventTicketPlaceholder> EventTickets { get; set; }
    //}

    public class ListItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsSelected { get; set; }
        public object Tags { get; set; }
    }

    public class CreateSchoolViewModel
    {
        public School School { get; set; }
        public RoleName? Role { get; set; }
        [Required(ErrorMessage ="Please enter the School's Location")]
        public string Location { get; set; }

        public CreateSchoolViewModel()
        {

        }

        public CreateSchoolViewModel(RoleName? role)
        {
            Role = role;
        }
    }

    public class ViewSchoolViewModel
    {
        public School School { get; set; }
        public OrganizationMember Member { get; set; }
        public List<UserTicket> UserTickets { get; set; }

        public ViewSchoolViewModel(School school)
        {
            School = school;
        }
    }

    public class ManageSchoolViewModel
    {
        public School School { get; set; }
        public string NewMemberId { get; set; }
        public Team NewTeam { get; set; }

        public ManageSchoolViewModel()
        {

        }
        public ManageSchoolViewModel(School school)
        {
            School = school;
        }
    }

    public class ListSchoolViewModel
    {
        public IEnumerable<School> Schools { get; set; }
        [Display(Name = "Teacher:")]
        public string TeacherId { get; set; }
        [Display(Name = "Dance Style:")]
        public int? DanceStyleId { get; set; }
        public Address SearchAddress { get; set; }
        public string Location { get; set; }
        public string Teacher { get; set; }
        public string Style { get; set; }
        public double? NELat { get; set; }
        public double? NELng { get; set; }
        public double? SWLat { get; set; }
        public double? SWLng { get; set; }
        public double? CenterLat { get; set; }
        public double? CenterLng { get; set; }
        public int? Zoom { get; set; }
    }
}
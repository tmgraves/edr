﻿using EDR.Enums;
using EDR.Utilities.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EDR.Models.ViewModels
{
    public class ViewTicketViewModel
    {
        public Ticket Ticket { get; set; }

        public IEnumerable<ListItem> AvailableClasses { get; set; }
        public IEnumerable<ListItem> SelectedClasses { get; set; }
        [RequiredStringArrayValue(ErrorMessage = "Select at least (1) Class")]
        public string[] PostedClasses { get; set; }
        public List<EventTicketPlaceholder> EventTickets { get; set; }
    }

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

        public CreateSchoolViewModel()
        {

        }

        public CreateSchoolViewModel(RoleName? role)
        {
            Role = role;
        }
    }
}
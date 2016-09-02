using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EDR.Models.ViewModels
{
    public class StoreItemsViewModel
    {
        public List<DancePack> DancePacks { get; set; }
    }

    public class ShoppingCartViewModel
    {
        public List<Cart> CartItems { get; set; }
        public decimal CartTotal { get; set; }
    }

    public class ShoppingCartRemoveViewModel
    {
        public string Message { get; set; }
        public decimal CartTotal { get; set; }
        public int CartCount { get; set; }
        public int ItemCount { get; set; }
        public int DeleteId { get; set; }
    }

    public class ConfirmationViewModel
    {
        public Order Order { get; set; }
    }

    public class OrderViewModel
    {
        public ICollection<Ticket> Tickets { get; set; }
        [Required(ErrorMessage = "Please pick a Quantity of Tickets")]
        [DefaultValue(1)]
        [Range(1, 100,ErrorMessage="Please select a Ticket Quantity")]
        [Display(Name = "Quantity of Tickets")]
        public int Quantity { get; set; }
        [Required(ErrorMessage = "Please pick a Ticket")]
        public int TicketId { get; set; }
        public int? EventInstanceId { get; set; }
        public EventInstance EventInstance { get; set; }
        public EDR.Enums.EventType? Type { get; set; }
        public int? SchoolId { get; set; }
        public School School { get; set; }
        public Order Order { get; set; }
        [Required(ErrorMessage = "Email Address is required")]
        [DisplayName("Email Address")]
        [RegularExpression(@"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,4}", ErrorMessage = "Email is is not valid.")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [Required]
        [EmailAddress]
        [Display(Name = "Confirm Email")]
        [Compare("Email", ErrorMessage = "The Email and Confirm Email do not match.")]
        public string ConfirmEmail { get; set; }
        [Required(ErrorMessage = "Please enter a Credit Card Number")]
        [Display(Name = "Credit Card Number:")]
        public string CCNumber { get; set; }
        [Required(ErrorMessage = "Please enter an Credit Card Expiration Month")]
        [Display(Name = "Exp Month:")]
        public string CCMonth { get; set; }
        public List<string> Years { get; set; }
        [Required(ErrorMessage = "Please enter an Credit Card Expiration Year")]
        [Display(Name = "Exp Year:")]
        public string CCYear { get; set; }
        [Required(ErrorMessage = "Please enter a Credit Card Security Code")]
        [Display(Name = "CVC:")]
        public string SecurityCode { get; set; }
        [Display(Name = "Order Amount:")]
        public decimal Amount { get; set; }
        public string Result { get; set; }
        public string Message { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }

        //public OrderViewModel(List<Ticket> tickets)
        //{
        //    Quantity = 1;
        //    Tickets = tickets;
        //    Years = new List<string>();
        //    for (int i = DateTime.Today.Year; i <= DateTime.Today.Year + 10; i++)
        //    {
        //        Years.Add(i.ToString());
        //    }
        //}

    }
    public class AttendeesViewModel
    {
        public Order Order { get; set; }
        public int EventInstanceId { get; set; }
        public EventInstance EventInstance { get; set; }
        public int Available { get; set; }
        public ICollection<Attendee> Attendees { get; set; }
    }

    public class Attendee
    {
        [Required(ErrorMessage ="Please enter a First Name")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "Please enter a Last Name")]
        public string LastName { get; set; }
        public string UserId { get; set; }
    }
}
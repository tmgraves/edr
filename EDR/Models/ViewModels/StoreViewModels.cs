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

    public class OrderViewModel
    {
        public ICollection<Ticket> Tickets { get; set; }
        [Required]
        [DefaultValue(1)]
        [Range(1, 100)]
        [Display(Name = "Quantity of Tickets")]
        public int Quantity { get; set; }
        [Required(ErrorMessage = "Please pick a Ticket")]
        public int TicketId { get; set; }
        public int? EventInstanceId { get; set; }
        public int? EventId { get; set; }
        public EDR.Enums.EventType? Type { get; set; }
        public int? SchoolId { get; set; }
        public Order Order { get; set; }
        public string CCNumber { get; set; }
        public string CCMonth { get; set; }
        public string CCYear { get; set; }
        public string SecurityCode { get; set; }
        public decimal Amount { get; set; }
        public OrderViewModel()
        {
            Quantity = 1;
        }
        public OrderViewModel(List<Ticket> tickets)
        {
            Quantity = 1;
            Tickets = tickets;
        }
    }
}
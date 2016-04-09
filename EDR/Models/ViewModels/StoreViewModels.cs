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

    public class BuyTicketViewModel
    {
        public Ticket Ticket { get; set; }
        [Required]
        [DefaultValue(1)]
        [Range(1, 100)]
        [Display(Name = "Quantity of Tickets")]
        public int Quantity { get; set; }

        public BuyTicketViewModel()
        {

        }
        public BuyTicketViewModel(Ticket ticket)
        {
            Ticket = ticket;
        }
    }
}
using EDR.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Net;
using System.ComponentModel;
using AuthorizeNet.Api.Controllers;
using AuthorizeNet.Api.Contracts.V1;
using AuthorizeNet.Api.Controllers.Bases;
using EDR.Enums;
using System.Configuration;

namespace EDR.Models
{
    public class DancePack : Entity
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public ICollection<Cart> Carts { get; set; }
        public ICollection<OrderDetail> OrderDetails { get; set; }
    }

    public class Cart : Entity
    {
        public string   CartIdentifier      { get; set; }
        public int      Count       { get; set; }
        public System.DateTime DateCreated { get; set; }
        [Required]
        [Index("IX_Cart", 2, IsUnique = true)]
        public int DancePackId { get; set; }
        [ForeignKey("DancePackId")]
        public virtual DancePack DancePack { get; set; }
    }

    public partial class Order : Entity
    {
        private DateTime _date = DateTime.Now;
        [ScaffoldColumn(false)]
        [Required]
        public DateTime OrderDate
        {
            get { return _date; }
            set { _date = value; }
        }
        [ScaffoldColumn(false)]
        [Required(ErrorMessage = "First Name is required")]
        [DisplayName("First Name")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "Last Name is required")]
        [DisplayName("Last Name")]
        public string LastName { get; set; }
        [Required(ErrorMessage = "Address is required")]
        public string Address { get; set; }
        [Required(ErrorMessage = "City is required")]
        public string City { get; set; }
        [Required(ErrorMessage = "State is required")]
        public string State { get; set; }
        [Required(ErrorMessage = "Postal Code is required")]
        [DisplayName("Postal Code")]
        public string PostalCode { get; set; }
        [Required(ErrorMessage = "Country is required")]
        public string Country { get; set; }
        public string Phone { get; set; }
        [Required(ErrorMessage = "Email Address is required")]
        [DisplayName("Email Address")]
        [RegularExpression(@"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,4}", ErrorMessage = "Email is is not valid.")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        public string Username { get; set; }
        [ScaffoldColumn(false)]
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
        public decimal Total { get; set; }
        //  Tran ID that comes back from Authorize.net
        public string OrderTransactionId { get; set; }
        public int? EventInstanceId { get; set; }
        [ForeignKey("EventInstanceId")]
        public EventInstance EventInstance { get; set; }
        public ICollection<OrderDetail> OrderDetails { get; set; }
        public ICollection<OrderTransaction> OrderTransactions { get; set; }
    }

    [Bind(Exclude = "Id")]
    public class OrderDetail : Entity
    {
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public int? TicketId { get; set; }
        [ForeignKey("TicketId")]
        public virtual Ticket Ticket { get; set; }
        [Required]
        public int OrderId { get; set; }
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; }
    }

    [Bind(Exclude = "Id")]
    public class OrderTransaction : Entity
    {
        [Required]
        public int OrderId { get; set; }
        [ForeignKey("OrderId")]
        public Order Order { get; set; }
        private DateTime _date = DateTime.Now;
        [ScaffoldColumn(false)]
        [Required]
        public DateTime TransactionDate
        {
            get { return _date; }
            set { _date = value; }
        }
        public transactionTypeEnum TransactionType { get; set; }
        public string ResponseCode { get; set; }
        public string ResponseMessage { get; set; }
        public string AccountNumber { get; set; }
        public string AccountType { get; set; }
        public bool Success { get; set; }
        public string TransactionId { get; set; }
    }

    [Bind(Exclude = "Id")]
    public class FinancialTransaction : Entity
    {
        private DateTime _date = DateTime.Now;
        [ScaffoldColumn(false)]
        [Required]
        public DateTime TranDate
        {
            get { return _date; }
            set { _date = value; }
        }
        public decimal Amount { get; set; }
        public string TranType { get; set; }
        public int? OrderId { get; set; }
        [ForeignKey("OrderId")]
        public Order Order { get; set; }
        public string OrderTransactionId { get; set; }
        public PaymentType PaymentType { get; set; }
        public string PayeeInfo { get; set; }
        public int? SchoolId { get; set; }
        [ForeignKey("SchoolId")]
        public virtual School School { get; set; }
        public int? PromoterId { get; set; }
        [ForeignKey("PromoterId")]
        public virtual Promoter Promoter { get; set; }
        public int? PromoterGroupId { get; set; }
        [ForeignKey("PromoterGroupId")]
        public virtual PromoterGroup PromoterGroup { get; set; }
        public int? PaymentBatchId { get; set; }
        [ForeignKey("PaymentBatchId")]
        public virtual PaymentBatch PaymentBatch { get; set; }
        public DateTime? Committed { get; set; }
        public int? SettlementBatchItemId { get; set; }
        [ForeignKey("SettlementBatchItemId")]
        public virtual SettlementBatchItem SettlementBatchItem { get; set; }
        public string SettlementStatus { get; set; }
        public DateTime? SettlementDate { get; set; }
        public bool Valid
        {
            get
            {
                if (TranType == "Purchase Order" && Committed != null && SettlementStatus == "settledSuccessfully" && SettlementDate != null && SettlementDate <= DateTime.Today.AddDays(-GlobalVariables.SettlementPeriod))
                {
                    return true;
                }
                else if (TranType != "Purchase Order" && Committed != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }

    [Bind(Exclude = "Id")]
    public class SettlementBatch : Entity
    {
        [Required]
        public string GatewayBatchId { get; set; }
        [Required]
        public DateTime BatchDate { get; set; }
        public string Status { get; set; }
        public virtual ICollection<SettlementBatchItem> SettlementBatchItems { get; set; }

        [Authorize(Roles = "Admin")]
        public static ANetApiResponse GetBatches()
        {
            ApplicationDbContext context = new ApplicationDbContext();
            if (context.FinancialTransactions.Where(t => t.SettlementStatus == null && t.TranType == "Purchase Order").Count() > 0)
            {
                if (ConfigurationManager.AppSettings["AuthorizeEnvironment"] == "Production")
                {
                    ApiOperationBase<ANetApiRequest, ANetApiResponse>.RunEnvironment = AuthorizeNet.Environment.PRODUCTION;
                }
                else
                {
                    ApiOperationBase<ANetApiRequest, ANetApiResponse>.RunEnvironment = AuthorizeNet.Environment.SANDBOX;
                }
                // define the merchant information (authentication / transaction id)
                ApiOperationBase<ANetApiRequest, ANetApiResponse>.MerchantAuthentication = new merchantAuthenticationType()
                {
                    name = ConfigurationManager.AppSettings["AuthorizeNetApiLoginID"],
                    ItemElementName = ItemChoiceType.transactionKey,
                    Item = ConfigurationManager.AppSettings["AuthorizeNetApiTransactionKey"],
                };

                //Get today's date
                //  Made some hacks to accomodate for different time zones
                var start = context.FinancialTransactions.Where(t => t.SettlementStatus == null && t.TranType == "Purchase Order").Min(d => d.TranDate).AddDays(-2);
                var firstSettlementDate = new DateTime(start.Year, start.Month, start.Day).ToLocalTime();
                var lastSettlementDate = DateTime.Today.ToLocalTime();
                //Console.WriteLine("First settlement date: {0} Last settlement date:{1}", firstSettlementDate,
                //    lastSettlementDate);

                var request = new getSettledBatchListRequest();
                request.firstSettlementDate = firstSettlementDate;
                request.lastSettlementDate = lastSettlementDate;
                request.includeStatistics = true;

                // instantiate the controller that will call the service
                var controller = new getSettledBatchListController(request);
                controller.Execute();

                // get the response from the service (errors contained if any)
                var response = controller.GetApiResponse();

                if (response != null && response.messages.resultCode == messageTypeEnum.Ok)
                {
                    if (response.batchList == null)
                        return response;

                    foreach (var batch in response.batchList)
                    {
                        if (context.SettlementBatches.Where(b => b.GatewayBatchId == batch.batchId).Count() == 0)
                        {
                            var bch = new SettlementBatch() { GatewayBatchId = batch.batchId, BatchDate = batch.settlementTimeLocal, Status = batch.settlementState };
                            context.SettlementBatches.Add(bch);
                            context.SaveChanges();

                            bch.SettlementBatchItems = new List<SettlementBatchItem>();

                            var listrequest = new getTransactionListRequest();
                            listrequest.batchId = batch.batchId;

                            var listcontroller = new getTransactionListController(listrequest);
                            listcontroller.Execute();

                            var listresponse = listcontroller.GetApiResponse();

                            if (listresponse != null && listresponse.messages.resultCode == messageTypeEnum.Ok)
                            {
                                if (listresponse.transactions != null)
                                {
                                    foreach (var transaction in listresponse.transactions)
                                    {
                                        bch.SettlementBatchItems.Add(new SettlementBatchItem() { TranId = transaction.transId, Amount = transaction.settleAmount, FirstName = transaction.firstName, LastName = transaction.lastName, Status = transaction.transactionStatus, SubmitDate = transaction.submitTimeLocal });
                                        context.Entry(bch).State = EntityState.Modified;

                                        //Console.WriteLine("Transaction Id: {0}", transaction.transId);
                                        //Console.WriteLine("Submitted on (Local): {0}", transaction.submitTimeLocal);
                                        //Console.WriteLine("Status: {0}", transaction.transactionStatus);
                                        //Console.WriteLine("Settle amount: {0}", transaction.settleAmount);
                                    }
                                    context.SaveChanges();

                                    //  Update Transaction Items
                                    var items = context.SettlementBatchItems.Where(i => i.SettlementBatchId == bch.Id).ToList();
                                    foreach (var i in items)
                                    {
                                        var tran = context.FinancialTransactions.Where(t => t.OrderTransactionId == i.TranId).FirstOrDefault();
                                        tran.SettlementBatchItemId = i.Id;
                                        tran.SettlementStatus = i.Status;
                                        tran.SettlementDate = batch.settlementTimeLocal;
                                        context.Entry(tran).State = EntityState.Modified;
                                    }
                                    context.SaveChanges();
                                }
                            }
                            else if (listresponse != null)
                            {
                                //Console.WriteLine("Error: " + listresponse.messages.message[0].code + "  " +
                                //                  listresponse.messages.message[0].text);
                            }
                        }
                        //Console.WriteLine("Batch Id: {0}", batch.batchId);
                        //Console.WriteLine("Batch settled on (UTC): {0}", batch.settlementTimeUTC);
                        //Console.WriteLine("Batch settled on (Local): {0}", batch.settlementTimeLocal);
                        //Console.WriteLine("Batch settlement state: {0}", batch.settlementState);
                        //Console.WriteLine("Batch market type: {0}", batch.marketType);
                        //Console.WriteLine("Batch product: {0}", batch.product);
                        //foreach (var statistics in batch.statistics)
                        //{
                        //    Console.WriteLine(
                        //        "Account type: {0} Total charge amount: {1} Charge count: {2} Refund amount: {3} Refund count: {4} Void count: {5} Decline count: {6} Error amount: {7}",
                        //        statistics.accountType, statistics.chargeAmount, statistics.chargeCount,
                        //        statistics.refundAmount, statistics.refundCount,
                        //        statistics.voidCount, statistics.declineCount, statistics.errorCount);
                        //}
                    }
                }
                else if (response != null)
                {
                    //Console.WriteLine("Error: " + response.messages.message[0].code + "  " +
                    //                  response.messages.message[0].text);
                }

                return response;
            }
            else
            {
                return null;
            }
        }
    }

    [Bind(Exclude = "Id")]
    public class SettlementBatchItem : Entity
    {
        [Required]
        public int SettlementBatchId { get; set; }
        [ForeignKey("SettlementBatchId")]
        public virtual SettlementBatch SettlementBatch { get; set; }
        public string TranId { get; set; }
        public DateTime SubmitDate { get; set; }
        public string Status { get; set; }
        public decimal Amount { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    [Bind(Exclude = "Id")]
    public class PaymentBatch : Entity
    {
        private DateTime _date = DateTime.Now;
        [ScaffoldColumn(false)]
        [Required]
        public DateTime BatchDate
        {
            get { return _date; }
            set { _date = value; }
        }
        public PaymentType PaymentType { get; set; }
        public DateTime? CommitDate { get; set; }
        public virtual ICollection<FinancialTransaction> FinancialTransactions { get; set; }
    }

    [Bind(Exclude = "Id")]
    public class UserTicket : Entity
    {
        [Required]
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
        [Required]
        public int TicketId { get; set; }
        [ForeignKey("TicketId")]
        public virtual Ticket Ticket { get; set; }
        [Required]
        public int Quantity { get; set; }
        private DateTime _date = DateTime.Now;
        [Required]
        public DateTime DatePurchased
        {
            get { return _date; }
            set { _date = value; }
        }
        public ICollection<EventRegistration> EventRegistrations { get; set; }
    }

    public partial class ShoppingCart
    {
        ApplicationDbContext context = new ApplicationDbContext();
        string ShoppingCartId { get; set; }
        public const string CartSessionKey = "CartId";
        public static ShoppingCart GetCart(HttpContextBase context)
        {
            var cart = new ShoppingCart();
            cart.ShoppingCartId = cart.GetCartId(context);
            return cart;
        }
        // Helper method to simplify shopping cart calls
        public static ShoppingCart GetCart(Controller controller)
        {
            return GetCart(controller.HttpContext);
        }
        public void AddToCart(DancePack dancePack)
        {
            // Get the matching cart and album instances
            var cartItem = context.Carts.SingleOrDefault(
                c => c.CartIdentifier == ShoppingCartId
                && c.DancePack.Id == dancePack.Id);

            if (cartItem == null)
            {
                // Create a new cart item if no cart item exists
                cartItem = new Cart
                {
                    DancePackId = dancePack.Id,
                    CartIdentifier = ShoppingCartId,
                    Count = 1,
                    DateCreated = DateTime.Now
                };
                context.Carts.Add(cartItem);
            }
            else
            {
                // If the item does exist in the cart, 
                // then add one to the quantity
                cartItem.Count++;
            }
            // Save changes
            context.SaveChanges();
        }
        public int RemoveFromCart(int id)
        {
            // Get the cart
            var cartItem = context.Carts.Single(
                cart => cart.CartIdentifier == ShoppingCartId
                && cart.Id == id);

            int itemCount = 0;

            if (cartItem != null)
            {
                if (cartItem.Count > 1)
                {
                    cartItem.Count--;
                    itemCount = cartItem.Count;
                }
                else
                {
                    context.Carts.Remove(cartItem);
                }
                // Save changes
                context.SaveChanges();
            }
            return itemCount;
        }
        public void EmptyCart()
        {
            var cartItems = context.Carts.Where(
                cart => cart.CartIdentifier == ShoppingCartId);

            foreach (var cartItem in cartItems)
            {
                context.Carts.Remove(cartItem);
            }
            // Save changes
            context.SaveChanges();
        }
        public List<Cart> GetCartItems()
        {
            return context.Carts.Where(cart => cart.CartIdentifier == ShoppingCartId).Include("DancePack").ToList();
        }
        public int GetCount()
        {
            // Get the count of each item in the cart and sum them up
            int? count = (from cartItems in context.Carts
                          where cartItems.CartIdentifier == ShoppingCartId
                          select (int?)cartItems.Count).Sum();
            // Return 0 if all entries are null
            return count ?? 0;
        }
        public decimal GetTotal()
        {
            // Multiply album price by count of that album to get 
            // the current price for each of those albums in the cart
            // sum all album price totals to get the cart total
            decimal? total = (from cartItems in context.Carts
                              where cartItems.CartIdentifier == ShoppingCartId
                              select (int?)cartItems.Count *
                              cartItems.DancePack.Price).Sum();

            return total ?? decimal.Zero;
        }
        public int CreateOrder(Order order)
        {
            decimal orderTotal = 0;

            var cartItems = GetCartItems();
            // Iterate over the items in the cart, 
            // adding the order details for each
            foreach (var item in cartItems)
            {
                var orderDetail = new OrderDetail
                {
                    //  DancePack = item.DancePack,
                    OrderId = order.Id,
                    UnitPrice = item.DancePack.Price,
                    Quantity = item.Count
                };
                // Set the order total of the shopping cart
                orderTotal += (item.Count * item.DancePack.Price);

                context.OrderDetails.Add(orderDetail);

            }
            // Set the order's total to the orderTotal count
            order.Total = orderTotal;

            // Save the order
            context.SaveChanges();
            // Empty the shopping cart
            EmptyCart();
            // Return the OrderId as the confirmation number
            return order.Id;
        }
        // We're using HttpContextBase to allow access to cookies.
        public string GetCartId(HttpContextBase context)
        {
            if (context.Session[CartSessionKey] == null)
            {
                if (!string.IsNullOrWhiteSpace(context.User.Identity.Name))
                {
                    context.Session[CartSessionKey] =
                        context.User.Identity.Name;
                }
                else
                {
                    // Generate a new random GUID using System.Guid class
                    Guid tempCartId = Guid.NewGuid();
                    // Send tempCartId back to client as a cookie
                    context.Session[CartSessionKey] = tempCartId.ToString();
                }
            }
            return context.Session[CartSessionKey].ToString();
        }
        // When a user has logged in, migrate their shopping cart to
        // be associated with their username
        public void MigrateCart(string userName)
        {
            var shoppingCart = context.Carts.Where(
                c => c.CartIdentifier == ShoppingCartId);

            foreach (Cart item in shoppingCart)
            {
                item.CartIdentifier = userName;
            }
            context.SaveChanges();
        }
    }
}
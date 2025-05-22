using EcommerceApi.Api.Contracts.Orders;

namespace EcommerceApi.Api.Contracts.Checkout
{
    public class CheckoutResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public OrderResponse Order { get; set; }
        public PaymentResponse Payment { get; set; }
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal Total { get; set; }
        public string PromotionApplied { get; set; }
        public DateTime CheckoutDate { get; set; }
    }
}

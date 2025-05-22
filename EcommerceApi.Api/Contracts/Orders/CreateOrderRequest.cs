namespace EcommerceApi.Api.Contracts.Orders
{
    public class CreateOrderRequest
    {
        public string ShippingAddress { get; set; }
        public List<OrderItemRequest> Items { get; set; }
        public PaymentRequest Payment { get; set; }
    }
}

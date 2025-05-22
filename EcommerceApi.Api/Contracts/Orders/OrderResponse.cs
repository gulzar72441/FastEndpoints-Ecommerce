namespace EcommerceApi.Api.Contracts.Orders
{
    public class OrderResponse
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public string ShippingAddress { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public List<OrderItemResponse> Items { get; set; }
        public PaymentResponse Payment { get; set; }
    }
}

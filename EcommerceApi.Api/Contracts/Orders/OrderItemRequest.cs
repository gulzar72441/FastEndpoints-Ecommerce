namespace EcommerceApi.Api.Contracts.Orders
{
    public class OrderItemRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}

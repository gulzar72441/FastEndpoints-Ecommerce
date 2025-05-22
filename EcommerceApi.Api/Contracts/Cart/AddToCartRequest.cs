namespace EcommerceApi.Api.Contracts.Cart
{
    public class AddToCartRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}

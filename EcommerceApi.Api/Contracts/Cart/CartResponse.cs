namespace EcommerceApi.Api.Contracts.Cart
{
    public class CartResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public List<CartItemResponse> Items { get; set; }
        public decimal SubTotal { get; set; }
        public string AppliedPromotionCode { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal Total { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}

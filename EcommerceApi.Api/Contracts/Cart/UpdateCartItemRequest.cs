﻿namespace EcommerceApi.Api.Contracts.Cart
{
    public class UpdateCartItemRequest
    {
        public int CartItemId { get; set; }
        public int Quantity { get; set; }
    }
}

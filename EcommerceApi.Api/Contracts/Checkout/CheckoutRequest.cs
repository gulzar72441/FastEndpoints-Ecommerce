using System.ComponentModel.DataAnnotations;

namespace EcommerceApi.Api.Contracts.Checkout
{
    public class CheckoutRequest
    {
        [Required]
        public string ShippingAddress { get; set; }

        [Required]
        public PaymentInfo Payment { get; set; }

        public string PromotionCode { get; set; }
    }

    public class PaymentInfo
    {
        [Required]
        public string PaymentMethod { get; set; }

        public string CardNumber { get; set; }

        public string CardHolderName { get; set; }

        public string ExpiryDate { get; set; }

        public string Cvv { get; set; }
    }
}

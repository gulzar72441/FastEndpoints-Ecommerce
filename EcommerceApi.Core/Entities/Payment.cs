using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceApi.Core.Entities
{
    public class Payment
    {
        public int Id { get; set; }
        public string PaymentMethod { get; set; } // Credit Card, PayPal, etc.
        public decimal Amount { get; set; }
        public string Status { get; set; } // Pending, Completed, Failed, Refunded
        public string TransactionId { get; set; }
        public DateTime PaymentDate { get; set; }

        // Foreign keys
        public int OrderId { get; set; }

        // Navigation properties
        public virtual Order Order { get; set; }
    }
}

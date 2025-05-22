using EcommerceApi.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceApi.Core.Interfaces
{
    public interface IPaymentRepository : IGenericRepository<Payment>
    {
        Task<Payment> GetPaymentByOrderIdAsync(int orderId);
        Task<bool> UpdatePaymentStatusAsync(int paymentId, string status);
    }
}

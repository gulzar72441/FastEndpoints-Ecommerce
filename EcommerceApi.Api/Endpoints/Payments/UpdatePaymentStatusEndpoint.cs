using EcommerceApi.Core.Interfaces;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;

namespace EcommerceApi.Api.Endpoints.Payments
{
    public class UpdatePaymentStatusEndpoint : Endpoint<UpdatePaymentStatusRequest, UpdatePaymentStatusResponse>
    {
        private readonly IPaymentRepository _paymentRepository;

        public UpdatePaymentStatusEndpoint(IPaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository;
        }

        public override void Configure()
        {
            Patch("/api/payments/{Id}/status");
            Roles("Admin");
            Summary(s =>
            {
                s.Summary = "Update a payment's status";
                s.Description = "This endpoint updates the status of a payment (Admin only)";
                s.Response<UpdatePaymentStatusResponse>(StatusCodes.Status200OK, "Payment status updated successfully");
                s.Response(StatusCodes.Status400BadRequest, "Invalid request");
                s.Response(StatusCodes.Status401Unauthorized, "Unauthorized");
                s.Response(StatusCodes.Status403Forbidden, "Forbidden");
                s.Response(StatusCodes.Status404NotFound, "Payment not found");
            });
        }

        public override async Task HandleAsync(UpdatePaymentStatusRequest req, CancellationToken ct)
        {
            // Validate status
            if (string.IsNullOrWhiteSpace(req.Status))
            {
                AddError(r => r.Status, "Status is required");
                await SendErrorsAsync(StatusCodes.Status400BadRequest, ct);
                return;
            }

            // Validate status value
            var validStatuses = new[] { "Pending", "Completed", "Failed", "Refunded" };
            if (!System.Array.Exists(validStatuses, s => s == req.Status))
            {
                AddError(r => r.Status, $"Status must be one of: {string.Join(", ", validStatuses)}");
                await SendErrorsAsync(StatusCodes.Status400BadRequest, ct);
                return;
            }

            // Get payment
            var payment = await _paymentRepository.GetByIdAsync(req.Id);
            if (payment == null)
            {
                await SendNotFoundAsync(ct);
                return;
            }

            // Update payment status
            var success = await _paymentRepository.UpdatePaymentStatusAsync(req.Id, req.Status);
            if (!success)
            {
                ThrowError("Failed to update payment status", StatusCodes.Status500InternalServerError);
                return;
            }

            // Return response
            await SendAsync(new UpdatePaymentStatusResponse
            {
                Id = req.Id,
                Status = req.Status,
                Message = "Payment status updated successfully"
            }, cancellation: ct);
        }
    }

    public class UpdatePaymentStatusRequest
    {
        public int Id { get; set; }
        public string Status { get; set; }
    }

    public class UpdatePaymentStatusResponse
    {
        public int Id { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
    }
}

using EcommerceApi.Core.Interfaces;
using FastEndpoints;

namespace EcommerceApi.Api.Endpoints.Orders
{
    public class UpdateOrderStatusEndpoint : Endpoint<UpdateOrderStatusRequest, UpdateOrderStatusResponse>
    {
        private readonly IOrderRepository _orderRepository;

        public UpdateOrderStatusEndpoint(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public override void Configure()
        {
            Patch("/api/orders/{Id}/status");
            Roles("Admin");
            Summary(s =>
            {
                s.Summary = "Update an order's status";
                s.Description = "This endpoint updates the status of an order (Admin only)";
                s.Response<UpdateOrderStatusResponse>(StatusCodes.Status200OK, "Order status updated successfully");
                s.Response(StatusCodes.Status400BadRequest, "Invalid request");
                s.Response(StatusCodes.Status401Unauthorized, "Unauthorized");
                s.Response(StatusCodes.Status403Forbidden, "Forbidden");
                s.Response(StatusCodes.Status404NotFound, "Order not found");
            });
        }

        public override async Task HandleAsync(UpdateOrderStatusRequest req, CancellationToken ct)
        {
            // Validate status
            if (string.IsNullOrWhiteSpace(req.Status))
            {
                AddError(r => r.Status, "Status is required");
                await SendErrorsAsync(StatusCodes.Status400BadRequest, ct);
                return;
            }

            // Validate status value
            var validStatuses = new[] { "Pending", "Processing", "Shipped", "Delivered", "Cancelled" };
            if (!System.Array.Exists(validStatuses, s => s == req.Status))
            {
                AddError(r => r.Status, $"Status must be one of: {string.Join(", ", validStatuses)}");
                await SendErrorsAsync(StatusCodes.Status400BadRequest, ct);
                return;
            }

            // Get order
            var order = await _orderRepository.GetByIdAsync(req.Id);
            if (order == null)
            {
                await SendNotFoundAsync(ct);
                return;
            }

            // Update order status
            var success = await _orderRepository.UpdateOrderStatusAsync(req.Id, req.Status);
            if (!success)
            {
                ThrowError("Failed to update order status", StatusCodes.Status500InternalServerError);
                return;
            }

            // Return response
            await SendAsync(new UpdateOrderStatusResponse
            {
                Id = req.Id,
                Status = req.Status,
                Message = "Order status updated successfully"
            }, cancellation: ct);
        }
    }

    public class UpdateOrderStatusRequest
    {
        public int Id { get; set; }
        public string Status { get; set; }
    }

    public class UpdateOrderStatusResponse
    {
        public int Id { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
    }
}

using EcommerceApi.Core.Interfaces;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;

namespace EcommerceApi.Api.Endpoints.Promotions
{
    public class DeletePromotionEndpoint : Endpoint<DeletePromotionRequest>
    {
        private readonly IPromotionRepository _promotionRepository;

        public DeletePromotionEndpoint(IPromotionRepository promotionRepository)
        {
            _promotionRepository = promotionRepository;
        }

        public override void Configure()
        {
            Delete("/api/promotions/{Id}");
            Roles("Admin");
            Summary(s =>
            {
                s.Summary = "Delete a promotion";
                s.Description = "This endpoint deletes a promotion (Admin only)";
                s.Response(StatusCodes.Status204NoContent, "Promotion deleted successfully");
                s.Response(StatusCodes.Status401Unauthorized, "Unauthorized");
                s.Response(StatusCodes.Status403Forbidden, "Forbidden");
                s.Response(StatusCodes.Status404NotFound, "Promotion not found");
            });
        }

        public override async Task HandleAsync(DeletePromotionRequest req, CancellationToken ct)
        {
            // Get promotion by ID
            var promotion = await _promotionRepository.GetByIdAsync(req.Id);
            if (promotion == null)
            {
                await SendNotFoundAsync(ct);
                return;
            }

            // Delete promotion
            await _promotionRepository.DeleteAsync(promotion);

            await SendNoContentAsync(ct);
        }
    }

    public class DeletePromotionRequest
    {
        public int Id { get; set; }
    }
}

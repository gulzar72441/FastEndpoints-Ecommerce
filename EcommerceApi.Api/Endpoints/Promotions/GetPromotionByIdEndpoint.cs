using EcommerceApi.Api.Contracts.Promotions;
using EcommerceApi.Core.Interfaces;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EcommerceApi.Api.Endpoints.Promotions
{
    public class GetPromotionByIdEndpoint : Endpoint<GetPromotionByIdRequest, PromotionResponse>
    {
        private readonly IPromotionRepository _promotionRepository;

        public GetPromotionByIdEndpoint(IPromotionRepository promotionRepository)
        {
            _promotionRepository = promotionRepository;
        }

        public override void Configure()
        {
            Get("/api/promotions/{Id}");
            Roles("Admin");
            Summary(s =>
            {
                s.Summary = "Get a promotion by its ID";
                s.Description = "This endpoint returns a promotion by its ID (Admin only)";
                s.Response<PromotionResponse>(StatusCodes.Status200OK, "Promotion retrieved successfully");
                s.Response(StatusCodes.Status401Unauthorized, "Unauthorized");
                s.Response(StatusCodes.Status403Forbidden, "Forbidden");
                s.Response(StatusCodes.Status404NotFound, "Promotion not found");
            });
        }

        public override async Task HandleAsync(GetPromotionByIdRequest req, CancellationToken ct)
        {
            var promotion = await _promotionRepository.GetByIdAsync(req.Id);
            if (promotion == null)
            {
                await SendNotFoundAsync(ct);
                return;
            }

            var response = new PromotionResponse
            {
                Id = promotion.Id,
                Name = promotion.Name,
                Description = promotion.Description,
                Code = promotion.Code,
                DiscountType = promotion.DiscountType,
                DiscountValue = promotion.DiscountValue,
                MinimumOrderAmount = promotion.MinimumOrderAmount,
                IsActive = promotion.IsActive,
                StartDate = promotion.StartDate,
                EndDate = promotion.EndDate,
                Products = promotion.PromotionProducts?.Select(pp => new PromotionProductResponse
                {
                    ProductId = pp.ProductId,
                    ProductName = pp.Product?.Name ?? "Unknown Product"
                }).ToList(),
                Categories = promotion.PromotionCategories?.Select(pc => new PromotionCategoryResponse
                {
                    CategoryId = pc.CategoryId,
                    CategoryName = pc.Category?.Name ?? "Unknown Category"
                }).ToList()
            };

            await SendAsync(response, cancellation: ct);
        }
    }

    public class GetPromotionByIdRequest
    {
        public int Id { get; set; }
    }
}

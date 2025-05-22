using EcommerceApi.Api.Contracts.Promotions;
using EcommerceApi.Core.Interfaces;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EcommerceApi.Api.Endpoints.Promotions
{
    public class GetPromotionByCodeEndpoint : Endpoint<GetPromotionByCodeRequest, PromotionResponse>
    {
        private readonly IPromotionRepository _promotionRepository;

        public GetPromotionByCodeEndpoint(IPromotionRepository promotionRepository)
        {
            _promotionRepository = promotionRepository;
        }

        public override void Configure()
        {
            Get("/api/promotions/code/{Code}");
            AllowAnonymous();
            Summary(s =>
            {
                s.Summary = "Get a promotion by its code";
                s.Description = "This endpoint returns a promotion by its code";
                s.Response<PromotionResponse>(StatusCodes.Status200OK, "Promotion retrieved successfully");
                s.Response(StatusCodes.Status404NotFound, "Promotion not found");
            });
        }

        public override async Task HandleAsync(GetPromotionByCodeRequest req, CancellationToken ct)
        {
            var promotion = await _promotionRepository.GetPromotionByCodeAsync(req.Code);
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

    public class GetPromotionByCodeRequest
    {
        public string Code { get; set; }
    }
}

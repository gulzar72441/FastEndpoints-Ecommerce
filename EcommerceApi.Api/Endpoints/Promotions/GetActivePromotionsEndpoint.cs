using EcommerceApi.Api.Contracts.Promotions;
using EcommerceApi.Core.Interfaces;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EcommerceApi.Api.Endpoints.Promotions
{
    public class GetActivePromotionsEndpoint : EndpointWithoutRequest<List<PromotionResponse>>
    {
        private readonly IPromotionRepository _promotionRepository;

        public GetActivePromotionsEndpoint(IPromotionRepository promotionRepository)
        {
            _promotionRepository = promotionRepository;
        }

        public override void Configure()
        {
            Get("/api/promotions/active");
            AllowAnonymous();
            Summary(s =>
            {
                s.Summary = "Get all active promotions";
                s.Description = "This endpoint returns all active promotions";
                s.Response<List<PromotionResponse>>(StatusCodes.Status200OK, "Active promotions retrieved successfully");
            });
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var promotions = await _promotionRepository.GetActivePromotionsAsync();

            var response = promotions.Select(p => new PromotionResponse
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Code = p.Code,
                DiscountType = p.DiscountType,
                DiscountValue = p.DiscountValue,
                MinimumOrderAmount = p.MinimumOrderAmount,
                IsActive = p.IsActive,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                Products = p.PromotionProducts?.Select(pp => new PromotionProductResponse
                {
                    ProductId = pp.ProductId,
                    ProductName = pp.Product?.Name ?? "Unknown Product"
                }).ToList(),
                Categories = p.PromotionCategories?.Select(pc => new PromotionCategoryResponse
                {
                    CategoryId = pc.CategoryId,
                    CategoryName = pc.Category?.Name ?? "Unknown Category"
                }).ToList()
            }).ToList();

            await SendAsync(response, cancellation: ct);
        }
    }
}

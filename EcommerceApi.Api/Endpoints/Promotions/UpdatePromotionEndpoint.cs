using EcommerceApi.Api.Contracts.Promotions;
using EcommerceApi.Core.Interfaces;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EcommerceApi.Api.Endpoints.Promotions
{
    public class UpdatePromotionEndpoint : Endpoint<UpdatePromotionRequest, PromotionResponse>
    {
        private readonly IPromotionRepository _promotionRepository;
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public UpdatePromotionEndpoint(
            IPromotionRepository promotionRepository,
            IProductRepository productRepository,
            ICategoryRepository categoryRepository)
        {
            _promotionRepository = promotionRepository;
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        public override void Configure()
        {
            Put("/api/promotions/{Id}");
            Roles("Admin");
            Summary(s =>
            {
                s.Summary = "Update an existing promotion";
                s.Description = "This endpoint updates an existing promotion (Admin only)";
                s.Response<PromotionResponse>(StatusCodes.Status200OK, "Promotion updated successfully");
                s.Response(StatusCodes.Status400BadRequest, "Invalid request");
                s.Response(StatusCodes.Status401Unauthorized, "Unauthorized");
                s.Response(StatusCodes.Status403Forbidden, "Forbidden");
                s.Response(StatusCodes.Status404NotFound, "Promotion not found");
            });
        }

        public override async Task HandleAsync(UpdatePromotionRequest req, CancellationToken ct)
        {
            // Get promotion by ID
            var promotion = await _promotionRepository.GetByIdAsync(req.Id);
            if (promotion == null)
            {
                await SendNotFoundAsync(ct);
                return;
            }

            // Validate request
            if (string.IsNullOrWhiteSpace(req.Code))
            {
                AddError(r => r.Code, "Promotion code is required");
            }
            else if (req.Code != promotion.Code)
            {
                // Check if new promotion code already exists
                var existingPromotion = await _promotionRepository.GetPromotionByCodeAsync(req.Code);
                if (existingPromotion != null && existingPromotion.Id != req.Id)
                {
                    AddError(r => r.Code, "Promotion code already exists");
                }
            }

            // Validate discount type
            if (string.IsNullOrWhiteSpace(req.DiscountType) ||
                (req.DiscountType != "Percentage" && req.DiscountType != "FixedAmount"))
            {
                AddError(r => r.DiscountType, "Discount type must be either 'Percentage' or 'FixedAmount'");
            }

            // Validate discount value
            if (req.DiscountValue <= 0)
            {
                AddError(r => r.DiscountValue, "Discount value must be greater than 0");
            }
            else if (req.DiscountType == "Percentage" && req.DiscountValue > 100)
            {
                AddError(r => r.DiscountValue, "Percentage discount cannot exceed 100%");
            }

            // Validate dates
            if (req.StartDate >= req.EndDate)
            {
                AddError(r => r.EndDate, "End date must be after start date");
            }

            // If there are validation errors, return bad request
            if (ValidationFailed)
            {
                await SendErrorsAsync(StatusCodes.Status400BadRequest, ct);
                return;
            }

            // Update promotion
            promotion.Name = req.Name;
            promotion.Description = req.Description;
            promotion.Code = req.Code;
            promotion.DiscountType = req.DiscountType;
            promotion.DiscountValue = req.DiscountValue;
            promotion.MinimumOrderAmount = req.MinimumOrderAmount;
            promotion.IsActive = req.IsActive;
            promotion.StartDate = req.StartDate;
            promotion.EndDate = req.EndDate;
            promotion.UpdatedAt = DateTime.UtcNow;

            // Save promotion
            await _promotionRepository.UpdateAsync(promotion);

            // Clear existing products and categories
            await _promotionRepository.ClearPromotionProductsAsync(promotion.Id);
            await _promotionRepository.ClearPromotionCategoriesAsync(promotion.Id);

            // Add products to promotion
            if (req.ProductIds != null && req.ProductIds.Any())
            {
                foreach (var productId in req.ProductIds)
                {
                    // Check if product exists
                    var product = await _productRepository.GetByIdAsync(productId);
                    if (product != null)
                    {
                        await _promotionRepository.AddProductToPromotionAsync(promotion.Id, productId);
                    }
                }
            }

            // Add categories to promotion
            if (req.CategoryIds != null && req.CategoryIds.Any())
            {
                foreach (var categoryId in req.CategoryIds)
                {
                    // Check if category exists
                    var category = await _categoryRepository.GetByIdAsync(categoryId);
                    if (category != null)
                    {
                        await _promotionRepository.AddCategoryToPromotionAsync(promotion.Id, categoryId);
                    }
                }
            }

            // Get updated promotion with products and categories
            var updatedPromotion = await _promotionRepository.GetByIdAsync(promotion.Id);

            // Prepare response
            var response = new PromotionResponse
            {
                Id = updatedPromotion.Id,
                Name = updatedPromotion.Name,
                Description = updatedPromotion.Description,
                Code = updatedPromotion.Code,
                DiscountType = updatedPromotion.DiscountType,
                DiscountValue = updatedPromotion.DiscountValue,
                MinimumOrderAmount = updatedPromotion.MinimumOrderAmount,
                IsActive = updatedPromotion.IsActive,
                StartDate = updatedPromotion.StartDate,
                EndDate = updatedPromotion.EndDate,
                Products = updatedPromotion.PromotionProducts?.Select(pp => new PromotionProductResponse
                {
                    ProductId = pp.ProductId,
                    ProductName = pp.Product?.Name ?? "Unknown Product"
                }).ToList(),
                Categories = updatedPromotion.PromotionCategories?.Select(pc => new PromotionCategoryResponse
                {
                    CategoryId = pc.CategoryId,
                    CategoryName = pc.Category?.Name ?? "Unknown Category"
                }).ToList()
            };

            await SendAsync(response, cancellation: ct);
        }
    }

    public class UpdatePromotionRequest
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Code { get; set; }
        public string DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal MinimumOrderAmount { get; set; }
        public bool IsActive { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int[] ProductIds { get; set; }
        public int[] CategoryIds { get; set; }
    }
}

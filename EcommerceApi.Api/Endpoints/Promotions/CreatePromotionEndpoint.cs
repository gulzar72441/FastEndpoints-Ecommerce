using EcommerceApi.Api.Contracts.Promotions;
using EcommerceApi.Core.Entities;
using EcommerceApi.Core.Interfaces;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EcommerceApi.Api.Endpoints.Promotions
{
    public class CreatePromotionEndpoint : Endpoint<CreatePromotionRequest, PromotionResponse>
    {
        private readonly IPromotionRepository _promotionRepository;
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public CreatePromotionEndpoint(
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
            Post("/api/promotions");
            Roles("Admin");
            Summary(s =>
            {
                s.Summary = "Create a new promotion";
                s.Description = "This endpoint creates a new promotion (Admin only)";
                s.Response<PromotionResponse>(StatusCodes.Status201Created, "Promotion created successfully");
                s.Response(StatusCodes.Status400BadRequest, "Invalid request");
                s.Response(StatusCodes.Status401Unauthorized, "Unauthorized");
                s.Response(StatusCodes.Status403Forbidden, "Forbidden");
            });
        }

        public override async Task HandleAsync(CreatePromotionRequest req, CancellationToken ct)
        {
            // Validate request
            if (string.IsNullOrWhiteSpace(req.Code))
            {
                AddError(r => r.Code, "Promotion code is required");
            }
            else
            {
                // Check if promotion code already exists
                var existingPromotion = await _promotionRepository.GetPromotionByCodeAsync(req.Code);
                if (existingPromotion != null)
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

            // Create new promotion
            var promotion = new Promotion
            {
                Name = req.Name,
                Description = req.Description,
                Code = req.Code,
                DiscountType = req.DiscountType,
                DiscountValue = req.DiscountValue,
                MinimumOrderAmount = req.MinimumOrderAmount,
                IsActive = req.IsActive,
                StartDate = req.StartDate,
                EndDate = req.EndDate,
                CreatedAt = DateTime.UtcNow,
                PromotionProducts = new List<PromotionProduct>(),
                PromotionCategories = new List<PromotionCategory>()
            };

            // Save promotion
            await _promotionRepository.AddAsync(promotion);

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
            var updatedPromotion = await _promotionRepository.GetPromotionByCodeAsync(promotion.Code);

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

            await SendAsync(response, StatusCodes.Status201Created, ct);
        }
    }
}

using EcommerceApi.Api.Contracts.Products;
using EcommerceApi.Core.Entities;
using EcommerceApi.Core.Interfaces;
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EcommerceApi.Api.Endpoints.Products
{
    public class CreateProductEndpoint : Endpoint<CreateProductRequest, ProductResponse>
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public CreateProductEndpoint(IProductRepository productRepository, ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        public override void Configure()
        {
            Post("/api/products");
            Roles("Admin");
            Summary(s =>
            {
                s.Summary = "Create a new product";
                s.Description = "This endpoint creates a new product (Admin only)";
                s.Response<ProductResponse>(StatusCodes.Status201Created, "Product created successfully");
                s.Response(StatusCodes.Status400BadRequest, "Invalid request");
                s.Response(StatusCodes.Status401Unauthorized, "Unauthorized");
                s.Response(StatusCodes.Status403Forbidden, "Forbidden");
            });
        }

        public override async Task HandleAsync(CreateProductRequest req, CancellationToken ct)
        {
            // Validate category exists
            var category = await _categoryRepository.GetByIdAsync(req.CategoryId);
            if (category == null)
            {
                AddError(r => r.CategoryId, "Category not found");
                await SendErrorsAsync(StatusCodes.Status400BadRequest, ct);
                return;
            }

            // Create new product
            var product = new Product
            {
                Name = req.Name,
                Description = req.Description,
                Price = req.Price,
                StockQuantity = req.StockQuantity,
                ImageUrl = req.ImageUrl,
                CategoryId = req.CategoryId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            // Save product to database
            await _productRepository.AddAsync(product);

            // Return response
            var response = new ProductResponse
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                ImageUrl = product.ImageUrl,
                IsActive = product.IsActive,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt,
                CategoryId = product.CategoryId,
                CategoryName = category.Name
            };

            await SendAsync(response, StatusCodes.Status201Created, ct);
        }
    }
}

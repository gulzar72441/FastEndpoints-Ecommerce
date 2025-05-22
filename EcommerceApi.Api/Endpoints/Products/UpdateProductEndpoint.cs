using EcommerceApi.Api.Contracts.Products;
using EcommerceApi.Core.Interfaces;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EcommerceApi.Api.Endpoints.Products
{
    public class UpdateProductEndpoint : Endpoint<UpdateProductRequest, ProductResponse>
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public UpdateProductEndpoint(IProductRepository productRepository, ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        public override void Configure()
        {
            Put("/api/products/{Id}");
            Roles("Admin");
            Summary(s =>
            {
                s.Summary = "Update an existing product";
                s.Description = "This endpoint updates an existing product (Admin only)";
                s.Response<ProductResponse>(StatusCodes.Status200OK, "Product updated successfully");
                s.Response(StatusCodes.Status400BadRequest, "Invalid request");
                s.Response(StatusCodes.Status401Unauthorized, "Unauthorized");
                s.Response(StatusCodes.Status403Forbidden, "Forbidden");
                s.Response(StatusCodes.Status404NotFound, "Product not found");
            });
        }

        public override async Task HandleAsync(UpdateProductRequest req, CancellationToken ct)
        {
            // Get product by ID
            var product = await _productRepository.GetByIdAsync(req.Id);
            if (product == null)
            {
                await SendNotFoundAsync(ct);
                return;
            }

            // Validate category exists
            var category = await _categoryRepository.GetByIdAsync(req.CategoryId);
            if (category == null)
            {
                AddError(r => r.CategoryId, "Category not found");
                await SendErrorsAsync(StatusCodes.Status400BadRequest, ct);
                return;
            }

            // Update product properties
            product.Name = req.Name;
            product.Description = req.Description;
            product.Price = req.Price;
            product.StockQuantity = req.StockQuantity;
            product.ImageUrl = req.ImageUrl;
            product.CategoryId = req.CategoryId;
            product.IsActive = req.IsActive;
            product.UpdatedAt = DateTime.UtcNow;

            // Save changes to database
            await _productRepository.UpdateAsync(product);

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

            await SendAsync(response, cancellation: ct);
        }
    }
}

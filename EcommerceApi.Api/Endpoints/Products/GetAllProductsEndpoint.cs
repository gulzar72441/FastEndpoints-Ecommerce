using EcommerceApi.Api.Contracts.Products;
using EcommerceApi.Core.Interfaces;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EcommerceApi.Api.Endpoints.Products
{
    public class GetAllProductsEndpoint : EndpointWithoutRequest<List<ProductResponse>>
    {
        private readonly IProductRepository _productRepository;

        public GetAllProductsEndpoint(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public override void Configure()
        {
            Get("/api/products");
            AllowAnonymous();
            Summary(s =>
            {
                s.Summary = "Get all products";
                s.Description = "This endpoint returns all products";
                s.Response<List<ProductResponse>>(StatusCodes.Status200OK, "Products retrieved successfully");
            });
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var products = await _productRepository.GetAllAsync();

            var response = products.Select(p => new ProductResponse
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                ImageUrl = p.ImageUrl,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                CategoryId = p.CategoryId,
                CategoryName = p.Category?.Name
            }).ToList();

            await SendAsync(response, cancellation: ct);
        }
    }
}

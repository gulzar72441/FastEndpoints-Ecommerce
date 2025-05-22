using EcommerceApi.Api.Contracts.Products;
using EcommerceApi.Core.Interfaces;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;

namespace EcommerceApi.Api.Endpoints.Products
{
    public class GetProductByIdEndpoint : Endpoint<GetProductByIdRequest, ProductResponse>
    {
        private readonly IProductRepository _productRepository;

        public GetProductByIdEndpoint(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public override void Configure()
        {
            Get("/api/products/{Id}");
            AllowAnonymous();
            Summary(s =>
            {
                s.Summary = "Get a product by ID";
                s.Description = "This endpoint returns a product by its ID";
                s.Response<ProductResponse>(StatusCodes.Status200OK, "Product retrieved successfully");
                s.Response(StatusCodes.Status404NotFound, "Product not found");
            });
        }

        public override async Task HandleAsync(GetProductByIdRequest req, CancellationToken ct)
        {
            var product = await _productRepository.GetByIdAsync(req.Id);

            if (product == null)
            {
                await SendNotFoundAsync(ct);
                return;
            }

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
                CategoryName = product.Category?.Name
            };

            await SendAsync(response, cancellation: ct);
        }
    }

    public class GetProductByIdRequest
    {
        public int Id { get; set; }
    }
}

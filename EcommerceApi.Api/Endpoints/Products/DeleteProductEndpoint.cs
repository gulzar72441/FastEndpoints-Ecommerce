using EcommerceApi.Core.Interfaces;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;

namespace EcommerceApi.Api.Endpoints.Products
{
    public class DeleteProductEndpoint : Endpoint<DeleteProductRequest>
    {
        private readonly IProductRepository _productRepository;

        public DeleteProductEndpoint(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public override void Configure()
        {
            Delete("/api/products/{Id}");
            Roles("Admin");
            Summary(s =>
            {
                s.Summary = "Delete a product";
                s.Description = "This endpoint deletes a product (Admin only)";
                s.Response(StatusCodes.Status204NoContent, "Product deleted successfully");
                s.Response(StatusCodes.Status401Unauthorized, "Unauthorized");
                s.Response(StatusCodes.Status403Forbidden, "Forbidden");
                s.Response(StatusCodes.Status404NotFound, "Product not found");
            });
        }

        public override async Task HandleAsync(DeleteProductRequest req, CancellationToken ct)
        {
            // Get product by ID
            var product = await _productRepository.GetByIdAsync(req.Id);
            if (product == null)
            {
                await SendNotFoundAsync(ct);
                return;
            }

            // Delete product from database
            await _productRepository.DeleteAsync(product);

            // Return no content response
            await SendNoContentAsync(ct);
        }
    }

    public class DeleteProductRequest
    {
        public int Id { get; set; }
    }
}

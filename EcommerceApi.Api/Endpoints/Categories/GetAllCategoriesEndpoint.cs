using EcommerceApi.Api.Contracts.Categories;
using EcommerceApi.Core.Interfaces;
using FastEndpoints;

namespace EcommerceApi.Api.Endpoints.Categories
{
    public class GetAllCategoriesEndpoint : EndpointWithoutRequest<List<CategoryResponse>>
    {
        private readonly ICategoryRepository _categoryRepository;

        public GetAllCategoriesEndpoint(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public override void Configure()
        {
            Get("/api/categories");
            AllowAnonymous();
            Summary(s =>
            {
                s.Summary = "Get all categories";
                s.Description = "This endpoint returns all categories";
                s.Response<List<CategoryResponse>>(StatusCodes.Status200OK, "Categories retrieved successfully");
            });
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var categories = await _categoryRepository.GetAllAsync();

            var response = categories.Select(c => new CategoryResponse
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                Products = null // Don't include products in the list view
            }).ToList();

            await SendAsync(response, cancellation: ct);
        }
    }
}

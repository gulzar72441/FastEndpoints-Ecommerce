using EcommerceApi.Api.Contracts.Categories;
using EcommerceApi.Core.Entities;
using EcommerceApi.Core.Interfaces;
using FastEndpoints;

namespace EcommerceApi.Api.Endpoints.Categories
{
    public class CreateCategoryEndpoint : Endpoint<CreateCategoryRequest, CategoryResponse>
    {
        private readonly ICategoryRepository _categoryRepository;

        public CreateCategoryEndpoint(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public override void Configure()
        {
            Post("/api/categories");
            Roles("Admin");
            Summary(s =>
            {
                s.Summary = "Create a new category";
                s.Description = "This endpoint creates a new category (Admin only)";
                s.Response<CategoryResponse>(StatusCodes.Status201Created, "Category created successfully");
                s.Response(StatusCodes.Status400BadRequest, "Invalid request");
                s.Response(StatusCodes.Status401Unauthorized, "Unauthorized");
                s.Response(StatusCodes.Status403Forbidden, "Forbidden");
            });
        }

        public override async Task HandleAsync(CreateCategoryRequest req, CancellationToken ct)
        {
            // Create new category
            var category = new Category
            {
                Name = req.Name,
                Description = req.Description,
                CreatedAt = DateTime.UtcNow
            };

            // Save category to database
            await _categoryRepository.AddAsync(category);

            // Return response
            var response = new CategoryResponse
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt,
                Products = null
            };

            await SendAsync(response, StatusCodes.Status201Created, ct);
        }
    }
}

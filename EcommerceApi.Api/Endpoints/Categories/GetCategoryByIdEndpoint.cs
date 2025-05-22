using EcommerceApi.Api.Contracts.Categories;
using EcommerceApi.Api.Contracts.Products;
using EcommerceApi.Core.Interfaces;
using FastEndpoints;

namespace EcommerceApi.Api.Endpoints.Categories
{
    public class GetCategoryByIdEndpoint : Endpoint<GetCategoryByIdRequest, CategoryResponse>
    {
        private readonly ICategoryRepository _categoryRepository;

        public GetCategoryByIdEndpoint(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public override void Configure()
        {
            Get("/api/categories/{Id}");
            AllowAnonymous();
            Summary(s =>
            {
                s.Summary = "Get a category by ID";
                s.Description = "This endpoint returns a category by its ID including its products";
                s.Response<CategoryResponse>(StatusCodes.Status200OK, "Category retrieved successfully");
                s.Response(StatusCodes.Status404NotFound, "Category not found");
            });
        }

        public override async Task HandleAsync(GetCategoryByIdRequest req, CancellationToken ct)
        {
            var category = await _categoryRepository.GetCategoryWithProductsAsync(req.Id);

            if (category == null)
            {
                await SendNotFoundAsync(ct);
                return;
            }

            var response = new CategoryResponse
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt,
                Products = category.Products?.Select(p => new ProductResponse
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
                    CategoryName = category.Name
                }).ToList()
            };

            await SendAsync(response, cancellation: ct);
        }
    }

    public class GetCategoryByIdRequest
    {
        public int Id { get; set; }
    }
}

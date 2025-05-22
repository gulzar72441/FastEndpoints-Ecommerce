using Bogus;
using EcommerceApi.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceApi.Infrastructure.Data
{
    public class DataSeeder
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DataSeeder> _logger;

        public DataSeeder(ApplicationDbContext context, ILogger<DataSeeder> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task SeedDataAsync()
        {
            try
            {
                // Apply any pending migrations
                await _context.Database.MigrateAsync();

                // Seed data only if the database is empty
                if (!_context.Users.Any())
                {
                    _logger.LogInformation("Starting database seeding...");

                    // Seed categories
                    var categories = await SeedCategoriesAsync();

                    // Seed products
                    var products = await SeedProductsAsync(categories);

                    // Seed users
                    var users = await SeedUsersAsync();

                    // Seed orders and payments
                    await SeedOrdersAndPaymentsAsync(users, products);

                    // Seed carts
                    await SeedCartsAsync(users, products);

                    // Seed promotions
                    await SeedPromotionsAsync(categories, products);

                    _logger.LogInformation("Database seeding completed successfully.");
                }
                else
                {
                    _logger.LogInformation("Database already contains data. Skipping seeding.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while seeding the database.");
                throw;
            }
        }

        private async Task<List<Category>> SeedCategoriesAsync()
        {
            var categoryFaker = new Faker<Category>()
                .RuleFor(c => c.Name, f => f.Commerce.Categories(1)[0])
                .RuleFor(c => c.Description, f => f.Commerce.ProductDescription())
                .RuleFor(c => c.CreatedAt, f => f.Date.Past(1));

            var categories = categoryFaker.Generate(5);

            // Ensure unique category names
            categories = categories.GroupBy(c => c.Name)
                .Select(g => g.First())
                .ToList();

            await _context.Categories.AddRangeAsync(categories);
            await _context.SaveChangesAsync();

            return categories;
        }

        private async Task<List<Product>> SeedProductsAsync(List<Category> categories)
        {
            _logger.LogInformation("Starting to seed 10,000 products...");

            // Create a list to hold all products
            var allProducts = new List<Product>();

            // Define product types and adjectives for more variety
            var productTypes = new[] { "Laptop", "Phone", "Tablet", "Camera", "Headphones", "Speaker", "Watch", "TV",
                "Monitor", "Keyboard", "Mouse", "Printer", "Router", "Hard Drive", "SSD", "Memory Card", "USB Drive",
                "Cable", "Charger", "Case", "Screen Protector", "Stand", "Dock", "Adapter", "Battery", "Power Bank",
                "Game Console", "Controller", "VR Headset", "Drone", "Fitness Tracker", "Smart Home Device", "Microphone",
                "Webcam", "Scanner", "Projector", "E-reader", "Graphics Card", "Processor", "Motherboard", "Fan",
                "Cooling System", "Software", "Antivirus", "Office Suite", "Game", "App", "Subscription", "Service" };

            var adjectives = new[] { "Premium", "Deluxe", "Professional", "Elite", "Ultimate", "Advanced", "Smart",
                "Wireless", "Portable", "Compact", "Slim", "Ultra", "Super", "Mega", "Mini", "Micro", "Nano", "Pro",
                "Max", "Plus", "Lite", "Basic", "Standard", "Custom", "Special", "Limited Edition", "Exclusive",
                "Signature", "Classic", "Vintage", "Modern", "Next-Gen", "Innovative", "Revolutionary", "High-End",
                "Budget", "Value", "Economy", "Performance", "Gaming", "Business", "Home", "Office", "Travel", "Outdoor" };

            var brands = new[] { "TechMaster", "ElectroPro", "DigiLife", "SmartTech", "FutureTech", "InnovateTech",
                "NextWave", "PrimeTech", "UltraElectronics", "MaxiGadget", "PowerTech", "EliteGear", "ProSystems",
                "OptimaTech", "VisionTech", "CoreTech", "AlphaTech", "OmegaElectronics", "DeltaGadgets", "ZenithTech" };

            // Create a Faker instance for products
            var productFaker = new Faker<Product>()
                .RuleFor(p => p.Name, f => $"{f.PickRandom(brands)} {f.PickRandom(adjectives)} {f.PickRandom(productTypes)} {f.Random.AlphaNumeric(3).ToUpper()}")
                .RuleFor(p => p.Description, f => f.Commerce.ProductDescription() + " " + f.Lorem.Paragraph(2))
                .RuleFor(p => p.Price, f => decimal.Parse(f.Commerce.Price(10, 2000, 2)))
                .RuleFor(p => p.StockQuantity, f => f.Random.Int(5, 500))
                .RuleFor(p => p.ImageUrl, f => f.Image.PicsumUrl())
                .RuleFor(p => p.IsActive, f => f.Random.Bool(0.9f)) // 90% of products are active
                .RuleFor(p => p.CreatedAt, f => f.Date.Between(DateTime.UtcNow.AddYears(-2), DateTime.UtcNow))
                .RuleFor(p => p.CategoryId, f => f.PickRandom(categories).Id);

            // Generate products in batches to avoid memory issues
            const int batchSize = 500;
            const int totalProducts = 10000;

            for (int i = 0; i < totalProducts; i += batchSize)
            {
                int currentBatchSize = Math.Min(batchSize, totalProducts - i);
                _logger.LogInformation($"Generating products batch {i / batchSize + 1} of {(totalProducts + batchSize - 1) / batchSize} (products {i + 1}-{i + currentBatchSize})...");

                var productBatch = productFaker.Generate(currentBatchSize);

                // Add the batch to the context
                await _context.Products.AddRangeAsync(productBatch);

                // Save the batch to the database
                await _context.SaveChangesAsync();

                // Add to our complete list for return
                allProducts.AddRange(productBatch);

                _logger.LogInformation($"Saved batch {i / batchSize + 1} to database.");
            }

            _logger.LogInformation($"Successfully seeded {allProducts.Count} products.");

            // For subsequent operations that need products, return a subset to avoid memory issues
            return allProducts.OrderBy(p => Guid.NewGuid()).Take(200).ToList();
        }

        private async Task<List<User>> SeedUsersAsync()
        {
            // Create admin user
            var adminUser = new User
            {
                Username = "admin",
                Email = "admin@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                FirstName = "Admin",
                LastName = "User",
                PhoneNumber = "1234567890",
                Address = "Admin Address",
                Role = "Admin",
                CreatedAt = DateTime.UtcNow
            };

            // Generate regular users
            var userFaker = new Faker<User>()
                .RuleFor(u => u.Username, f => f.Internet.UserName())
                .RuleFor(u => u.Email, f => f.Internet.Email())
                .RuleFor(u => u.PasswordHash, f => BCrypt.Net.BCrypt.HashPassword("Password@123"))
                .RuleFor(u => u.FirstName, f => f.Name.FirstName())
                .RuleFor(u => u.LastName, f => f.Name.LastName())
                .RuleFor(u => u.PhoneNumber, f => f.Phone.PhoneNumber())
                .RuleFor(u => u.Address, f => f.Address.FullAddress())
                .RuleFor(u => u.Role, f => "Customer")
                .RuleFor(u => u.CreatedAt, f => f.Date.Past(1));

            var users = userFaker.Generate(20);
            users.Add(adminUser);

            // Ensure unique usernames and emails
            users = users.GroupBy(u => u.Email)
                .Select(g => g.First())
                .ToList();

            await _context.Users.AddRangeAsync(users);
            await _context.SaveChangesAsync();

            return users;
        }

        private async Task SeedOrdersAndPaymentsAsync(List<User> users, List<Product> products)
        {
            var random = new Random();
            var customerUsers = users.Where(u => u.Role == "Customer").ToList();

            var orderFaker = new Faker<Order>()
                .RuleFor(o => o.OrderNumber, f => $"ORD-{f.Random.AlphaNumeric(8).ToUpper()}")
                .RuleFor(o => o.Status, f => f.PickRandom(new[] { "Pending", "Processing", "Shipped", "Delivered" }))
                .RuleFor(o => o.ShippingAddress, f => f.Address.FullAddress())
                .RuleFor(o => o.OrderDate, f => f.Date.Past(1))
                .RuleFor(o => o.DeliveryDate, (f, o) => o.Status == "Delivered" ? f.Date.Future(1, o.OrderDate) : null)
                .RuleFor(o => o.UserId, f => f.PickRandom(customerUsers).Id);

            var orders = orderFaker.Generate(30);

            foreach (var order in orders)
            {
                // Generate 1-5 order items for each order
                var numItems = random.Next(1, 6);
                var orderItems = new List<OrderItem>();
                decimal totalAmount = 0;

                var selectedProducts = products
                    .OrderBy(x => Guid.NewGuid())
                    .Take(numItems)
                    .ToList();

                foreach (var product in selectedProducts)
                {
                    var quantity = random.Next(1, 4);
                    var unitPrice = product.Price;
                    var totalPrice = unitPrice * quantity;

                    orderItems.Add(new OrderItem
                    {
                        OrderId = order.Id,
                        ProductId = product.Id,
                        Quantity = quantity,
                        UnitPrice = unitPrice,
                        TotalPrice = totalPrice
                    });

                    totalAmount += totalPrice;
                }

                order.TotalAmount = totalAmount;
                order.Items = orderItems;

                // Create payment for the order
                var payment = new Payment
                {
                    OrderId = order.Id,
                    PaymentMethod = new Faker().PickRandom(new[] { "Credit Card", "PayPal", "Bank Transfer" }),
                    Amount = totalAmount,
                    Status = order.Status == "Pending" ? "Pending" : "Completed",
                    TransactionId = new Faker().Random.AlphaNumeric(12).ToUpper(),
                    PaymentDate = order.OrderDate.AddHours(1)
                };

                order.Payment = payment;
            }

            await _context.Orders.AddRangeAsync(orders);
            await _context.SaveChangesAsync();
        }

        private async Task SeedCartsAsync(List<User> users, List<Product> products)
        {
            var random = new Random();
            var customerUsers = users.Where(u => u.Role == "Customer").ToList();

            // Create carts for some customers (not all)
            var selectedUsers = customerUsers
                .OrderBy(x => Guid.NewGuid())
                .Take(10)
                .ToList();

            var carts = new List<Cart>();

            foreach (var user in selectedUsers)
            {
                // Create cart
                var cart = new Cart
                {
                    UserId = user.Id,
                    CreatedAt = DateTime.UtcNow,
                    Items = new List<CartItem>()
                };

                // Add 1-3 random products to cart
                var numItems = random.Next(1, 4);
                var selectedProducts = products
                    .OrderBy(x => Guid.NewGuid())
                    .Take(numItems)
                    .ToList();

                foreach (var product in selectedProducts)
                {
                    var quantity = random.Next(1, 3);
                    var unitPrice = product.Price;
                    var totalPrice = unitPrice * quantity;

                    cart.Items.Add(new CartItem
                    {
                        CartId = cart.Id,
                        ProductId = product.Id,
                        Quantity = quantity,
                        UnitPrice = unitPrice,
                        TotalPrice = totalPrice,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                carts.Add(cart);
            }

            await _context.Carts.AddRangeAsync(carts);
            await _context.SaveChangesAsync();
        }

        private async Task SeedPromotionsAsync(List<Category> categories, List<Product> products)
        {
            // Create some promotions
            var promotions = new List<Promotion>
            {
                new Promotion
                {
                    Name = "Summer Sale",
                    Description = "Get 15% off on all products",
                    Code = "SUMMER15",
                    DiscountType = "Percentage",
                    DiscountValue = 15,
                    MinimumOrderAmount = 50,
                    IsActive = true,
                    StartDate = DateTime.UtcNow.AddDays(-10),
                    EndDate = DateTime.UtcNow.AddDays(20),
                    CreatedAt = DateTime.UtcNow
                },
                new Promotion
                {
                    Name = "Welcome Discount",
                    Description = "Get $10 off on your first order",
                    Code = "WELCOME10",
                    DiscountType = "FixedAmount",
                    DiscountValue = 10,
                    MinimumOrderAmount = 30,
                    IsActive = true,
                    StartDate = DateTime.UtcNow.AddDays(-30),
                    EndDate = DateTime.UtcNow.AddDays(60),
                    CreatedAt = DateTime.UtcNow
                },
                new Promotion
                {
                    Name = "Electronics Sale",
                    Description = "Get 20% off on all electronics",
                    Code = "ELEC20",
                    DiscountType = "Percentage",
                    DiscountValue = 20,
                    MinimumOrderAmount = null,
                    IsActive = true,
                    StartDate = DateTime.UtcNow.AddDays(-5),
                    EndDate = DateTime.UtcNow.AddDays(25),
                    CreatedAt = DateTime.UtcNow,
                    PromotionCategories = new List<PromotionCategory>()
                },
                new Promotion
                {
                    Name = "Flash Sale",
                    Description = "Get 25% off on selected products",
                    Code = "FLASH25",
                    DiscountType = "Percentage",
                    DiscountValue = 25,
                    MinimumOrderAmount = null,
                    IsActive = true,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddDays(3),
                    CreatedAt = DateTime.UtcNow,
                    PromotionProducts = new List<PromotionProduct>()
                }
            };

            await _context.Promotions.AddRangeAsync(promotions);
            await _context.SaveChangesAsync();

            // Add categories to category-specific promotion
            var electronicsPromotion = promotions.FirstOrDefault(p => p.Code == "ELEC20");
            if (electronicsPromotion != null)
            {
                // Find electronics category or any category if not found
                var electronicsCategory = categories.FirstOrDefault(c =>
                    c.Name.ToLower().Contains("electronics") ||
                    c.Name.ToLower().Contains("tech")) ??
                    categories.FirstOrDefault();

                if (electronicsCategory != null)
                {
                    electronicsPromotion.PromotionCategories.Add(new PromotionCategory
                    {
                        PromotionId = electronicsPromotion.Id,
                        CategoryId = electronicsCategory.Id
                    });
                }
            }

            // Add products to product-specific promotion
            var flashSalePromotion = promotions.FirstOrDefault(p => p.Code == "FLASH25");
            if (flashSalePromotion != null)
            {
                // Select 5 random products for the flash sale
                var flashSaleProducts = products
                    .OrderBy(x => Guid.NewGuid())
                    .Take(5)
                    .ToList();

                foreach (var product in flashSaleProducts)
                {
                    flashSalePromotion.PromotionProducts.Add(new PromotionProduct
                    {
                        PromotionId = flashSalePromotion.Id,
                        ProductId = product.Id
                    });
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}

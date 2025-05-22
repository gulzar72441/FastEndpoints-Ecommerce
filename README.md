# E-commerce API

A comprehensive e-commerce API built with ASP.NET Core, FastEndpoints, Entity Framework Core, and SQL Server.

## Features

- **Authentication & Authorization**: JWT-based authentication with user roles (Admin, Customer)
- **Product Management**: CRUD operations for products with category organization
- **Order Processing**: Complete order workflow from creation to delivery
- **Payment Handling**: Payment processing with multiple status tracking
- **Shopping Cart**: Cart management with item addition, removal, and quantity updates
- **Checkout Process**: Seamless checkout flow from cart to order creation
- **Promotions Engine**: Flexible promotion system with percentage and fixed amount discounts
- **Repository Pattern**: Clean architecture with separation of concerns
- **Data Seeding**: Sample data generation using Bogus library

## Project Structure

- **EcommerceApi.Core**: Contains domain entities and repository interfaces
- **EcommerceApi.Infrastructure**: Contains database context, repository implementations, and data seeding
- **EcommerceApi.Api**: Contains API endpoints, DTOs, and services

## Getting Started

### Prerequisites

- .NET 8.0 SDK or later
- SQL Server (or SQL Server LocalDB)
- Visual Studio 2022 or any preferred IDE

### Setup

1. Clone the repository
2. Update the connection string in `appsettings.json` if needed
3. Run the application

```bash
cd EcommerceApi
dotnet run --project EcommerceApi.Api
```

The application will automatically apply migrations and seed the database with sample data on first run.

### Default Admin User

- Email: admin@example.com
- Password: Admin@123

## API Endpoints

### Authentication

- `POST /api/auth/register` - Register a new user
- `POST /api/auth/login` - Login and get JWT token

### Products

- `GET /api/products` - Get all products
- `GET /api/products/{id}` - Get product by ID
- `POST /api/products` - Create a new product (Admin)
- `PUT /api/products/{id}` - Update a product (Admin)
- `DELETE /api/products/{id}` - Delete a product (Admin)

### Categories

- `GET /api/categories` - Get all categories
- `GET /api/categories/{id}` - Get category by ID with products
- `POST /api/categories` - Create a new category (Admin)

### Cart

- `GET /api/cart` - Get current user's cart
- `POST /api/cart/items` - Add item to cart
- `PUT /api/cart/items` - Update cart item quantity
- `DELETE /api/cart/items/{cartItemId}` - Remove item from cart
- `DELETE /api/cart` - Clear cart
- `POST /api/cart/apply-promotion` - Apply promotion code to cart

### Checkout

- `POST /api/checkout` - Process checkout from cart to order

### Orders

- `GET /api/orders/my-orders` - Get current user's orders
- `GET /api/orders/{id}` - Get order by ID
- `POST /api/orders` - Create a new order
- `PATCH /api/orders/{id}/status` - Update order status (Admin)

### Promotions

- `GET /api/promotions` - Get all promotions (Admin)
- `GET /api/promotions/active` - Get all active promotions
- `GET /api/promotions/{id}` - Get promotion by ID (Admin)
- `GET /api/promotions/code/{code}` - Get promotion by code
- `POST /api/promotions` - Create a new promotion (Admin)
- `PUT /api/promotions/{id}` - Update a promotion (Admin)
- `DELETE /api/promotions/{id}` - Delete a promotion (Admin)

### Payments

- `PATCH /api/payments/{id}/status` - Update payment status (Admin)

## Technologies Used

- ASP.NET Core 8.0
- FastEndpoints
- Entity Framework Core
- SQL Server
- JWT Authentication
- Bogus (for data seeding)
- BCrypt.Net-Next (for password hashing)

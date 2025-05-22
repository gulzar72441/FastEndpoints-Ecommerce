using EcommerceApi.Api.Contracts.Auth;
using EcommerceApi.Api.Services;
using EcommerceApi.Core.Entities;
using EcommerceApi.Core.Interfaces;
using FastEndpoints;

namespace EcommerceApi.Api.Endpoints.Auth
{
    public class RegisterEndpoint : Endpoint<RegisterRequest, LoginResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly JwtService _jwtService;

        public RegisterEndpoint(IUserRepository userRepository, JwtService jwtService)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
        }

        public override void Configure()
        {
            Post("/api/auth/register");
            AllowAnonymous();
            Summary(s =>
            {
                s.Summary = "Register a new user";
                s.Description = "This endpoint registers a new user and returns a JWT token";
                s.Response<LoginResponse>(StatusCodes.Status201Created, "Registration successful");
                s.Response(StatusCodes.Status400BadRequest, "Email or username already exists");
            });
        }

        public override async Task HandleAsync(RegisterRequest req, CancellationToken ct)
        {
            // Check if email already exists
            if (await _userRepository.CheckEmailExistsAsync(req.Email))
            {
                AddError(r => r.Email, "Email already exists");
                await SendErrorsAsync(StatusCodes.Status400BadRequest, ct);
                return;
            }

            // Check if username already exists
            if (await _userRepository.CheckUsernameExistsAsync(req.Username))
            {
                AddError(r => r.Username, "Username already exists");
                await SendErrorsAsync(StatusCodes.Status400BadRequest, ct);
                return;
            }

            // Create new user
            var user = new User
            {
                Username = req.Username,
                Email = req.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
                FirstName = req.FirstName,
                LastName = req.LastName,
                PhoneNumber = req.PhoneNumber,
                Address = req.Address,
                Role = "Customer", // Default role for new users
                CreatedAt = DateTime.UtcNow
            };

            // Save user to database
            await _userRepository.AddAsync(user);

            // Generate JWT token
            var token = _jwtService.GenerateToken(user);

            // Return response
            await SendAsync(new LoginResponse
            {
                Token = token,
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role
            }, StatusCodes.Status201Created, ct);
        }
    }
}

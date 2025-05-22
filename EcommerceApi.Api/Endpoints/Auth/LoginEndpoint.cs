using EcommerceApi.Api.Contracts.Auth;
using EcommerceApi.Api.Services;
using EcommerceApi.Core.Interfaces;
using FastEndpoints;

namespace EcommerceApi.Api.Endpoints.Auth
{
    public class LoginEndpoint : Endpoint<LoginRequest, LoginResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly JwtService _jwtService;

        public LoginEndpoint(IUserRepository userRepository, JwtService jwtService)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
        }

        public override void Configure()
        {
            Post("/api/auth/login");
            AllowAnonymous();
            Summary(s =>
            {
                s.Summary = "Authenticate a user and generate a JWT token";
                s.Description = "This endpoint authenticates a user with email and password and returns a JWT token";
                s.Response<LoginResponse>(StatusCodes.Status200OK, "Authentication successful");
                s.Response(StatusCodes.Status401Unauthorized, "Invalid credentials");
            });
        }

        public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
        {
            // Find user by email
            var user = await _userRepository.GetUserByEmailAsync(req.Email);

            // Check if user exists and password is correct
            if (user == null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            {
                ThrowError("Invalid email or password", StatusCodes.Status401Unauthorized);
                return;
            }

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
            }, cancellation: ct);
        }
    }
}

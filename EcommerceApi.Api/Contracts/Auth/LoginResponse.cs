namespace EcommerceApi.Api.Contracts.Auth
{
    public class LoginResponse
    {
        public string Token { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
    }
}

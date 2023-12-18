namespace SecuringWebAPI.Model.DTO
{
    public class LoginResponse
    {
        public string? Name { get; set; }
        public string? UserName { get; set; }
        public string? AccessToken { get; set; }
        public  string? RefreshToken { get; set; }
        public DateTime Expiration { get; set; }
        public Status? Status { get; set; }
    }
}

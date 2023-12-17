namespace SecuringWebAPI.Model.Domain
{
    public class TokenInfo
    {
        public int id { get; set; }
        public string? RefreshToken { get; set; }
        public string? UserName { get; set; }
        public DateTime RefreshTokenExpiry { get; set; }
    }
}

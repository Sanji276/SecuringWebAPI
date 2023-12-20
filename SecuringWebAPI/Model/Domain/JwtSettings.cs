namespace SecuringWebAPI.Model.Domain
{
    public class JwtSettings
    {
        public string? SecretKey { get; set; }
        public TimeSpan TokenExpiryTime { get; set; }
    }
}

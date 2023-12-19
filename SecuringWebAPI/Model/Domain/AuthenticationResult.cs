namespace SecuringWebAPI.Model.Domain
{
    public class AuthenticationResult
    {
        public string? Token { get; set; }
        public string? Success { get; set; }
        public string? ErrorMessage { get; set; }
        public IEnumerable<string>? Errors { get; set; }
    }
}

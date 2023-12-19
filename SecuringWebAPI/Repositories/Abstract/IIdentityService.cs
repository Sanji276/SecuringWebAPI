namespace SecuringWebAPI.Repositories.Abstract
{
    public interface IIdentityService
    {
        Task RegisterAsync(string? email, string? password);
    }
}

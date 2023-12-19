using Microsoft.AspNetCore.Identity;
using SecuringWebAPI.Model.Domain;

namespace SecuringWebAPI.Repositories.Abstract
{
    public interface IIdentityService
    {
        Task<AuthenticationResult> RegisterAsync(string? email, string? password);
    }
}

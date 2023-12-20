using Microsoft.AspNetCore.Identity;
using SecuringWebAPI.Model.Domain;
using SecuringWebAPI.Model.DTO;

namespace SecuringWebAPI.Repositories.Abstract
{
    public interface IIdentityService
    {
        Task<AuthenticationResult> LoginUser(LoginModel model);
        Task<AuthenticationResult> RegisterAsync(string? email, string? password);
        Task<AuthenticationResult> RequestRefreshTokenAsync(string token, string refreshtoken);
    }
}

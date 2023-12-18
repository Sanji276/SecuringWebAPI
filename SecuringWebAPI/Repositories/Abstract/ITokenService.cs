using SecuringWebAPI.Model.DTO;
using System.Security.Claims;

namespace SecuringWebAPI.Repositories.Abstract
{
    public interface ITokenService
    {
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
        TokenResponse GetToken(IEnumerable<Claim> Userclaim);
        string GetRefreshToken();
    }
}
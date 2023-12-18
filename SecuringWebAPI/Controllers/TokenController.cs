using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SecuringWebAPI.Data;
using SecuringWebAPI.Model.DTO;
using SecuringWebAPI.Repositories.Abstract;

namespace SecuringWebAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly SecuringWebAPIContext _dbContext;
        private readonly ITokenService _tokenService;

        public TokenController(SecuringWebAPIContext dbContext, ITokenService tokenService)
        {
            _dbContext = dbContext;
            _tokenService = tokenService;
        }
        [HttpPost]
        public IActionResult RefreshToken([FromForm] RefreshTokenRequest refreshTokenModel)
        {
            if (refreshTokenModel == null)
            {
                return BadRequest("Invalid client request");
            }
            else
            {
                string accessToken = refreshTokenModel.AccessToken;
                string refreshToken = refreshTokenModel.RefreshToken;
                var claimsPrincipal = _tokenService.GetPrincipalFromExpiredToken(accessToken);
                var username = claimsPrincipal.Identity.Name;

                var user = _dbContext.TokenInfos.SingleOrDefault(x => x.UserName == username);
                if (user is null || user.RefreshToken != refreshToken || user.RefreshTokenExpiry <= DateTime.Now)
                {
                    return BadRequest("Invalid credentials");
                }
                else
                {
                    var newAccessToken = _tokenService.GetToken(claimsPrincipal.Claims);
                    var newRefreshToken = _tokenService.GetRefreshToken();
                    user.RefreshToken = newRefreshToken;
                    _dbContext.SaveChanges();

                    return Ok(new RefreshTokenRequest
                    {
                        AccessToken = newAccessToken.TokenString,
                        RefreshToken = newRefreshToken

                    });
                }
            }
        }

        //revoke is used to remove the token entry from db
        [HttpPost, Authorize]
        public IActionResult Revoke()
        {
            var userName = User.Identity.Name;
            var user = _dbContext.TokenInfos.Where(x => x.UserName == userName).FirstOrDefault();
            if (user is null)
            {
                return BadRequest();
            }
            user.RefreshToken = null;
            _dbContext.SaveChanges();
            return Ok(true);
        }
    }
}

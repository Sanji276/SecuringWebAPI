using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SecuredWebAPIBestPractices.Model.Domain;
using SecuringWebAPI.Model.DTO;
using SecuringWebAPI.Repositories.Abstract;

namespace SecuringWebAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class IdentityController : ControllerBase
    {
        private readonly IIdentityService _identityService;

        public IdentityController(IIdentityService identityService)
        {
            _identityService = identityService;
        }

        [HttpPost]
        public async Task<IActionResult> RegisterAsync([FromBody] RegistrationModel model)
        {
            var authResponse = await _identityService.RegisterAsync(model.Email, model.Password);
            if (!authResponse.Success)
            {
                return BadRequest(new AuthFailedResponse
                {
                    Errors = authResponse.Errors
                });
            }
            return Ok(new AuthSuccessResponse
            {
                RefreshToken = authResponse.RefreshToken,
                Token = authResponse.Token
            });
        }

        [HttpPost]
        public async Task<IActionResult> LoginAsync([FromBody] LoginModel model)
        {
            var authResponse = await _identityService.LoginUser(model);
            if (!authResponse.Success)
            {
                return BadRequest(new AuthFailedResponse
                {
                    Errors = authResponse.Errors
                });
            }
            return Ok(new AuthSuccessResponse
            {
                Token = authResponse.Token,
                RefreshToken = authResponse.RefreshToken
            });
        }

        [HttpPost]
        public async Task<IActionResult> RefreshTokenAsync([FromBody] RequestRefreshTokenModel model)
        {
            var authResponse = await _identityService.RequestRefreshTokenAsync(model.Token,model.RefreshToken);
            if (!authResponse.Success)
            {
                return BadRequest(new AuthFailedResponse
                {
                    Errors = authResponse.Errors
                });
            }
            return Ok(new AuthSuccessResponse
            {
                Token = authResponse.Token,
                RefreshToken = authResponse.RefreshToken
            });
        }
    }
}

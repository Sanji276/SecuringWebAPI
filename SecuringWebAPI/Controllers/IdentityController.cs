using Microsoft.AspNetCore.Mvc;
using SecuringWebAPI.Model.DTO;
using SecuringWebAPI.Repositories.Abstract;

namespace SecuringWebAPI.Controllers
{
    public class IdentityController : Controller
    {
        private readonly IIdentityService _identityService;

        public IdentityController(IIdentityService identityService)
        {
            _identityService = identityService;
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Register([FromBody] RegistrationModel model)
        {
            var authResponse = await _identityService.RegisterAsync(model.Email, model.Password);
            return Ok();
        }
    }
}

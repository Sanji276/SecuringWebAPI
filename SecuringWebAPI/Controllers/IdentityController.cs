using Microsoft.AspNetCore.Mvc;
using SecuringWebAPI.Model.DTO;

namespace SecuringWebAPI.Controllers
{
    public class IdentityController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Register([FromBody] RegistrationModel model)
        {
            var authResponse = 
            return Ok();
        }
    }
}

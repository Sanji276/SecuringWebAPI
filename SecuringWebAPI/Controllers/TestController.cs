using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SecuringWebAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [Authorize(Roles ="User",AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult Ind()
        {
            string[] car =
            {
                "Audi","Hundai","Mercedez"
            };
            return Ok(car);
        }
    }
}

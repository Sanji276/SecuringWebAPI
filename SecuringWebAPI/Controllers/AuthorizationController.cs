using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SecuringWebAPI.Data;
using SecuringWebAPI.Model;
using SecuringWebAPI.Model.Domain;
using SecuringWebAPI.Model.DTO;
using SecuringWebAPI.Repositories.Abstract;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;

namespace SecuringWebAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AuthorizationController : ControllerBase
    {
        private readonly SecuringWebAPIContext _dbContext;
        private readonly ITokenService _tokenService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AuthorizationController> _logger;

        public AuthorizationController(SecuringWebAPIContext dbContext, ITokenService tokenService
            , UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole> roleManager
            ,ILogger<AuthorizationController> logger)
        {
            _dbContext = dbContext;
            _tokenService = tokenService;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> GetToken([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);
            _logger.LogInformation($"Email: {model.UserName}, Password: {model.Password}");
            var pwd = await _userManager.CheckPasswordAsync(user, model.Password);
            _logger.LogInformation($"Password Check Result: {pwd}");

            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var userRole = await _userManager.GetRolesAsync(user);
                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, model.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                foreach (var roles in userRole)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, roles));
                }
                
                var token = _tokenService.GetToken(authClaims);
                var refreshToken = _tokenService.GetRefreshToken();

                var tokenInfo = _dbContext.TokenInfos.Where(x => x.UserName == user.UserName).FirstOrDefault();
                if (tokenInfo == null)
                {
                    var info = new TokenInfo
                    {
                        RefreshToken = refreshToken,
                        RefreshTokenExpiry = DateTime.Now.AddDays(1),
                        UserName = user.UserName
                    };
                    await _dbContext.TokenInfos.AddAsync(info);
                }
                else
                {
                    tokenInfo.RefreshToken = refreshToken;
                    tokenInfo.RefreshTokenExpiry = DateTime.UtcNow.AddDays(1);
                }
                
                await _dbContext.SaveChangesAsync();

                return Ok(
                    new LoginResponse
                    {
                        Name = user.FirstName,
                        UserName = user.UserName,
                        AccessToken = token.TokenString,
                        RefreshToken = refreshToken,
                        Expiration = DateTime.UtcNow.AddMinutes(1),
                        Status = new Status
                        {
                            StatusCode = Convert.ToInt32(HttpStatusCode.OK),
                            StatusMessage = "Successfully Logged"
                        }
                    }
                    ); 

            }
            return Unauthorized(
                new LoginResponse
                {
                    Status = new Status
                    {
                        StatusCode = Convert.ToInt32(HttpStatusCode.Unauthorized),
                        StatusMessage = "Not Authorized!!!"
                    }
                });
        }

        [HttpPost]
        public async Task<IActionResult> UserRegistration([FromBody] RegistrationModel model)
        {
            try
            {
                var userExist = await _userManager.FindByEmailAsync(model.Email);
                if (userExist != null)
                {
                    return BadRequest("User with this mail is already created!!");
                }
                else
                {
                    if (ModelState.IsValid)
                    {
                        var User = new ApplicationUser
                        {
                            FirstName = model.FirstName,
                            LastName = model.LastName,
                            UserName = model.UserName,
                            Email = model.Email,
                            SecurityStamp = Guid.NewGuid().ToString()
                        };
                        var result = await _userManager.CreateAsync(User, model.Password);
                        if (result.Succeeded)
                        {
                            if (!await _roleManager.RoleExistsAsync(UserRoles.User))
                                await _roleManager.CreateAsync(new IdentityRole(UserRoles.User));
                            if (await _roleManager.RoleExistsAsync(UserRoles.User))
                                await _userManager.AddToRoleAsync(User, UserRoles.User);

                            return CreatedAtAction(nameof(UserRegistration), new { UserName = model.UserName, Result = "User created Successfully." });
                        }

                        return Conflict();
                    }
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Login()
        {
            return Ok(new RegistrationModel
            {
                UserName = "",
                Email = "",
                Password = ""
            });
        }

        [HttpPost]
        [Consumes("application/x-www-form-urlencoded")]
        public async Task<IActionResult> fromform([FromForm] LoginUserModel model)
        {
            var jsonresult = JsonConvert.SerializeObject(model);
            var data = JsonConvert.DeserializeObject<LoginUserModel>(jsonresult);
            return Ok(data);
        }

       
    }
}

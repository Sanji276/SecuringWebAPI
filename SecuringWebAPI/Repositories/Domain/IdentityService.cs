using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SecuringWebAPI.Model.Domain;
using SecuringWebAPI.Model.DTO;
using SecuringWebAPI.Repositories.Abstract;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SecuredWebAPIBestPractices.Repositories.Domain
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOptions<JwtSettings> _jwtSettings;

        public IdentityService(UserManager<ApplicationUser> userManager, IOptions<JwtSettings> jwtSettings)
        {
            _userManager = userManager;
            _jwtSettings = jwtSettings;
        }
        public async Task<AuthenticationResult> RegisterAsync(string? email, string? password)
        {
            var userExist = await _userManager.FindByEmailAsync(email);
            if (userExist != null)
            {
                return new AuthenticationResult
                {
                    Errors = new[] { "User with this mail already exist!" }
                };
            }

            var newUser = new ApplicationUser
            {
                Email = email,
                UserName = email
            };
            var result = await _userManager.CreateAsync(newUser, password);
            if (!result.Succeeded)
            {
                return new AuthenticationResult
                {
                    Errors = result.Errors.Select(x => x.Description)
                };
            }

            return GenerateAuthenticationResultForUser(newUser);
        }

        private AuthenticationResult GenerateAuthenticationResultForUser(ApplicationUser User)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var SecretKey = Encoding.UTF8.GetBytes(_jwtSettings.Value.SecretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, User.Email),

                    //jti is for token revalidation or generating refresh token
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, User.Email),
                    new Claim("Id",User.Id)
                }),
                Expires = DateTime.UtcNow.AddMinutes(10),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(SecretKey)
                                                    , SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return new AuthenticationResult
            {
                Success = true,
                Token = tokenHandler.WriteToken(token)
            };
        }

        public async Task<AuthenticationResult> LoginUser(LoginModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.UserName);
            var pwd = await _userManager.CheckPasswordAsync(user, model.Password);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                return GenerateAuthenticationResultForUser(user);
            }
            else
            {
                return new AuthenticationResult
                {
                    Errors = new[] { "Invalid Credintials" }
                };
            }

        }
    }
}

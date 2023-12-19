using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using SecuringWebAPI.Model.Domain;
using SecuringWebAPI.Repositories.Abstract;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SecuringWebAPI.Repositories.Domain
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JwtSettings _jwtSettings;

        public IdentityService(UserManager<ApplicationUser> userManager, JwtSettings jwtSettings)
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

            var tokenHandler = new JwtSecurityTokenHandler();
            var SecretKey = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, newUser.Email),
                    //jti is for token revalidation or generating refresh token
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, newUser.Email),
                    new Claim("Id",newUser.Id)
                }),
                Expires = DateTime.UtcNow.AddMinutes(10),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(SecretKey)
                                                    , SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return new AuthenticationResult
            {

            };
        }
    }
}

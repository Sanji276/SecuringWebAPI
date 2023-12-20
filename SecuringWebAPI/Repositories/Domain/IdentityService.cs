using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SecuredWebAPIBestPractices.Model.Domain;
using SecuringWebAPI.Data;
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
        private readonly TokenValidationParameters _tokenValidationParams;
        private readonly SecuringWebAPIContext _context;

        public IdentityService(UserManager<ApplicationUser> userManager, IOptions<JwtSettings> jwtSettings, TokenValidationParameters tokenValidationParams,
                                    SecuringWebAPIContext context)
        {
            _userManager = userManager;
            _jwtSettings = jwtSettings;
            _tokenValidationParams = tokenValidationParams;
            _context = context;
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

            return await GenerateAuthenticationResultForUserAsync(newUser);
        }

        private async Task<AuthenticationResult> GenerateAuthenticationResultForUserAsync(ApplicationUser User)
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
                Expires = DateTime.UtcNow.Add(_jwtSettings.Value.TokenExpiryTime),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(SecretKey)
                                                    , SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            var refreshToken = new RefreshTokenModel
            {

                JwtId = token.Id,
                UserId = User.Id,
                CreationDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddHours(1),
            };

            await _context.RefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();
            return new AuthenticationResult
            {
                Success = true,
                Token = tokenHandler.WriteToken(token),
                RefreshToken = refreshToken.Token
            };
        }

        public async Task<AuthenticationResult> LoginUser(LoginModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.UserName);
            var pwd = await _userManager.CheckPasswordAsync(user, model.Password);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                return await GenerateAuthenticationResultForUserAsync(user);
            }
            else
            {
                return new AuthenticationResult
                {
                    Errors = new[] { "Invalid Credintials" }
                };
            }

        }

        public async Task<AuthenticationResult> RequestRefreshTokenAsync(string token, string refreshtoken)
        {
            var validatedToken = GetPrincipalFromToken(token);
            if (validatedToken == null)
            {
                return new AuthenticationResult { Errors = new[] { "Invalid Token" } };
            }

            var expiryDateUnix = long.Parse(validatedToken.Claims.Single(x => x.Type == JwtRegisteredClaimNames.Exp).Value);

            var expiryDateUtc = new DateTime(1970, 1, 1, 1, 0, 0, 0,DateTimeKind.Utc).AddSeconds(expiryDateUnix);

            if(expiryDateUtc> DateTime.UtcNow)
            {
                return new AuthenticationResult { Errors = new[] { "Cannot refresh Token. Token hasn't expired yet" } };
            }

            var jti = validatedToken.Claims.Single(x => x.Type == JwtRegisteredClaimNames.Jti).Value;

            var storedRefreshToken = await _context.RefreshTokens.SingleOrDefaultAsync(x => x.Token == refreshtoken);

            if (storedRefreshToken == null)
                return new AuthenticationResult { Errors = new[] { "Invalid Refresh Token" } };

            if(DateTime.UtcNow >  storedRefreshToken.ExpiryDate)
                return new AuthenticationResult { Errors = new[] { "Refresh Token is expired" } };

            if(storedRefreshToken.Used)
                return new AuthenticationResult { Errors = new[] { "This Refresh Token is already used." } };

            if(storedRefreshToken.JwtId != jti)
                return new AuthenticationResult { Errors = new[] { "This refresh Token doesn't match this JWT" } };

            storedRefreshToken.Used = true;
            _context.Update(storedRefreshToken);
            await _context.SaveChangesAsync();

            var user = await _userManager.FindByIdAsync(validatedToken.Claims.Single(x=>x.Type == "Id").Value);

            return await GenerateAuthenticationResultForUserAsync(user);

        }

        private ClaimsPrincipal GetPrincipalFromToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var principal = tokenHandler.ValidateToken(token, _tokenValidationParams, out var validatedToken);

                var isValidToken = (validatedToken is JwtSecurityToken jwtSecurityToken) &&
                                    jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                                    StringComparison.InvariantCultureIgnoreCase);
                if(!isValidToken)
                {
                    return null;
                }
                return principal;
            }
            catch
            {
                return null;
            }
        }
    }
}

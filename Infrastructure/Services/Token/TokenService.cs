using Application;
using Application.Features.Token;
using AuthLibrary.Constants.Authentication;
using Infrastructure.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ResponseResult.Models.Requests.Identity;
using ResponseResult.Models.Responses.Identity;
using ResponseResult.Wrappers;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Services.Token
{
    public class TokenService : ITokenService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly TokenSettings _tokenSettings;

        public TokenService(UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IOptions<TokenSettings> tokenSettings)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _tokenSettings = tokenSettings.Value;
        }


        public async Task<IResponseWrapper> GetTokenAsync(TokenRequest tokenRequest)
        {
            #region valditions
            var userInDb = await _userManager.FindByNameAsync(tokenRequest.UserName);
            if (userInDb == null || !await _userManager.CheckPasswordAsync(userInDb, tokenRequest.Password))
            {
                return await ResponseWrapper.FailAsync("Invalid username or password");
            }

            //check if user is active
            if (!userInDb.IsActive)
            {
                return await ResponseWrapper.FailAsync("User is inactive. Contact admin");
            }

            //check if email is confirmed
            if (!userInDb.EmailConfirmed)
            {
                return await ResponseWrapper.FailAsync("Email is not confirmed");
            }
            #endregion

            //Generate Refresh Token
            userInDb.RefreshToken = GenerateRefreshToken();
            userInDb.RefreshTokenExpiryDate = DateTime.UtcNow.AddDays(_tokenSettings.RefreshTokenExpiryInDays);

            await _userManager.UpdateAsync(userInDb);

            //Generate auth Token
            var token = await GenerateJwtAsync(userInDb);

            var tokenResponse = new TokenResponse
            {
                Token = token,
                RefreshToken = userInDb.RefreshToken,
                RefreshTokenExpiry = userInDb.RefreshTokenExpiryDate
            };

            return await ResponseWrapper<TokenResponse>.SuccessAsync(data: tokenResponse, "Token generated successfully");
        }

        public async Task<IResponseWrapper> GetRefreshTokenAsync(RefreshTokenRequest refreshTokenRequest)
        {
            var userPrincipal = GetClaimPrincipalFromExpiredToken(refreshTokenRequest.Token);
            var userEmail = userPrincipal.FindFirstValue(ClaimTypes.Email);

            var userInDb = await _userManager.FindByEmailAsync(userEmail);

            if (userInDb is not null)
            {
                if(userInDb.RefreshToken != refreshTokenRequest.RefreshToken ||
                    userInDb.RefreshTokenExpiryDate <= DateTime.UtcNow)
                {
                    return await ResponseWrapper.FailAsync("Invalid token provided");
                }

                var token = GenerateEncryptedToken(await GetClaimsAsync(userInDb), GetSigningCredentials());
                userInDb.RefreshToken = GenerateRefreshToken();
                userInDb.RefreshTokenExpiryDate = DateTime.UtcNow.AddDays(_tokenSettings.RefreshTokenExpiryInDays);
                await _userManager.UpdateAsync(userInDb);

                var tokenResponse = new TokenResponse
                {
                    Token = token,
                    RefreshToken = userInDb.RefreshToken,
                    RefreshTokenExpiry = userInDb.RefreshTokenExpiryDate
                };

                return await ResponseWrapper<TokenResponse>.SuccessAsync(data: tokenResponse, "Token generated successfully");
            }
            return await ResponseWrapper.FailAsync("User does not exist");
        }

        #region Private Helpers for token generation

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        private async Task<IEnumerable<Claim>> GetClaimsAsync(ApplicationUser user)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);
            var roleClaims = new List<Claim>();
            var permissionClaims = new List<Claim>();

            foreach (var role in roles)
            {
                roleClaims.Add(new Claim(ClaimTypes.Role, role));
                var currentRole = await _roleManager.FindByNameAsync(role);
                var allPermissionsForCurrentRole = await _roleManager.GetClaimsAsync(currentRole);
                permissionClaims.AddRange(allPermissionsForCurrentRole);
                //permissionClaims.AddRange(allPermissionsForCurrentRole.Where(c => c.Type == "Permission"));
            }

            var claims = new List<Claim>
            {
                new (ClaimTypes.NameIdentifier, user.Id),
                new (ClaimTypes.Name, user.FirstName),
                new (ClaimTypes.Surname, user.LastName),
                new (ClaimTypes.Email, user.Email),
                new (ClaimTypes.MobilePhone, user.PhoneNumber ?? string.Empty)
            }
            .Union(roleClaims)
            .Union(userClaims)
            .Union(permissionClaims);

            return claims;
        }


        private SigningCredentials GetSigningCredentials()
        {
            var secret = Encoding.UTF8.GetBytes(_tokenSettings.Secret);
            return new SigningCredentials(new SymmetricSecurityKey(secret), SecurityAlgorithms.HmacSha256);
        }

        private string GenerateEncryptedToken(IEnumerable<Claim> claims, SigningCredentials signingCredentials)
        {
            var token = new JwtSecurityToken(claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_tokenSettings.TokenExpiryInMinutes),
                signingCredentials: signingCredentials);

            var tokenHandler = new JwtSecurityTokenHandler();
            var enrcyptedToken = tokenHandler.WriteToken(token);
            return enrcyptedToken;
        }

        private async Task<string> GenerateJwtAsync(ApplicationUser user)
        {
            var signingCredentials = GetSigningCredentials();
            var claims = await GetClaimsAsync(user);
            var token = GenerateEncryptedToken(claims, signingCredentials);
            return token;
        }

        private ClaimsPrincipal GetClaimPrincipalFromExpiredToken(string expiredToken)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenSettings.Secret)),
                ValidateLifetime = false, //we are checking expired tokens here
                ValidIssuer = AppClaim.Issuer,
                ValidAudience = AppClaim.Audience,
                RoleClaimType = ClaimTypes.Role,
                ClockSkew = TimeSpan.Zero
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(expiredToken, tokenValidationParameters, 
                out SecurityToken securityToken);
            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg
                .Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }
            return principal;
        }

        #endregion
    }
}

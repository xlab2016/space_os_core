using Api.AspNetCore.Helpers;
using Api.AspNetCore.Models.Configuration;
using Api.AspNetCore.Models.Secure;
using Api.AspNetCore.Models.Validation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Newtonsoft.Json;

namespace Api.AspNetCore.Services
{
    public class JwtTokenAuthenticationService : IAuthenticateService
    {
        private readonly TokenManagement tokenManagement;
        private readonly IUserManagementService userManagementService;
        private readonly ILogger<JwtTokenAuthenticationService> logger;

        public JwtTokenAuthenticationService(IUserManagementService userManagementService,
            IOptions<TokenManagement> tokenManagement,
             ILogger<JwtTokenAuthenticationService> logger)
        {
            this.logger = logger;
            this.userManagementService = userManagementService;
            this.tokenManagement = tokenManagement?.Value;
        }

        public virtual async Task<JwtToken> IsAuthenticated(JwtTokenRequest request, 
            Func<User, List<Claim>, Task> claimOperation = null)
        {
            var result = new ValidationResult();

            if (string.IsNullOrEmpty(request.Username))
            {
                logger.LogError($"user name is empty");
                return new JwtToken { ErrorCode = IUserManagementService.IsValidResult.UserNameIsEmpty.ToString() };
            }

            if (string.IsNullOrEmpty(request.Password))
            {
                logger.LogError($"for user {request.Username} password is empty");
                return new JwtToken { ErrorCode = IUserManagementService.IsValidResult.PasswordIsEmpty.ToString() };
            }

            //request.Username = request.Username.ToLower();

            //request.Password.ValidateAsPassword(nameof(request.Password), result);
            //if (result.Errors.Count != 0)
            //{
            //    logger.LogError($"for user {request.Username} passwod language is not english");
            //    return new JwtToken { ErrorCode = IUserManagementService.IsValidResult.LanguageIsNotEnglish.ToString() };
            //}

            var user = await userManagementService.GetUserByLogin(request.Username);

            if (user == null)
            {
                // No user in provider
                logger.LogError($"no user {request.Username} in provider/invalid login or password");
                return new JwtToken { ErrorCode = IUserManagementService.IsValidResult.InvalidLoginOrPassword.ToString() };
            }

            var isValidResult = await userManagementService.IsValidUser(request.Username, request.Password);

            var checkAttemptsResult = await userManagementService.CheckAttempts(request.Username, isValidResult.ToString());

            if (isValidResult != IUserManagementService.IsValidResult.Success)
            {
                // No user in system
                logger.LogError($"no user {request.Username} in system");
                return checkAttemptsResult;
            }

            if (!string.IsNullOrEmpty(checkAttemptsResult.ErrorCode))
                return checkAttemptsResult;
            var roles = (await userManagementService.GetRoles(user.Login)).ToList();
            user.Roles = roles;

            // генерим refresh token и сохраняем его у пользователя
            var refreshToken = GenerateRefreshToken();
            await userManagementService.SetRefreshToken(user.Login, refreshToken);

            logger.LogInformation($"user {user.Login} logged");

            // отдаем jwt и refresh токен наружу
            return await GenerateJwtToken(user, refreshToken, claimOperation);
        }

        public async Task<JwtToken> RefreshTokenAsync(RefreshTokenRequest request)
        {
            // базовая валидация входных данных
            if (string.IsNullOrEmpty(request.UserName))
            {
                logger.LogError("user name is empty");
                return new JwtToken
                {
                    ErrorCode = IUserManagementService.IsValidResult.UserNameIsEmpty.ToString()
                };
            }

            if (string.IsNullOrEmpty(request.RefreshToken))
            {
                logger.LogError($"for user {request.UserName} refresh token is empty");
                return new JwtToken
                {
                    ErrorCode = IUserManagementService.IsValidRefreshTokenResult.RefreshTokenIsEmpty.ToString()
                };
            }

            var user = await userManagementService.GetUserByLogin(request.UserName);

            if (user == null)
            {
                // No user in provider
                logger.LogError($"no user {request.UserName} in provider/invalid login or password");
                return new JwtToken
                {
                    ErrorCode = IUserManagementService.IsValidResult.InvalidLoginOrPassword.ToString()
                };
            }

            // проверяем refresh token в сторе (БД, кэше и т.п.)
            var isValid = await userManagementService.IsValidRefreshToken(request.UserName, request.RefreshToken);
            if (isValid != IUserManagementService.IsValidRefreshTokenResult.Success)
            {
                // refresh token невалиден / неактивен
                logger.LogError($"invalid refresh token for user {request.UserName}, result: {isValid}");
                return null;
            }

            // дополнительно проверяем срок жизни refresh токена (внутри сериализованного объекта)
            try
            {
                var refreshTokenObj = JsonConvert.DeserializeObject<RefreshToken>(request.RefreshToken);
                if (DateTime.UtcNow >= refreshTokenObj.Expires)
                {
                    await userManagementService.SetRefreshToken(user.Login, null);
                    return new JwtToken { ErrorCode = "TokenExpired" };
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Refresh token deserialize error");
                return null;
            }

            var roles = (await userManagementService.GetRoles(user.Login)).ToList();
            user.Roles = roles;

            // генерим новый refresh token и перезаписываем старый
            var newRefreshToken = GenerateRefreshToken();
            await userManagementService.SetRefreshToken(user.Login, newRefreshToken);

            // отдаем новый jwt + новый refresh token
            return await GenerateJwtToken(user, newRefreshToken);
        }

        private async Task<JwtToken> GenerateJwtToken(User user, string refreshToken,
            Func<User, List<Claim>, Task> claimOperation = null)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Login),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
            };

            if (user.Roles != null)
                user.Roles.ForEach(_ =>
                {
                    claims.Add(new Claim(ClaimTypes.Role, _?.Name));
                });

            if (claimOperation != null)
                await claimOperation(user, claims);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenManagement.Secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expires = DateTime.Now.AddDays(2);

            var jwtToken = new JwtSecurityToken(
                tokenManagement.Issuer,
                tokenManagement.Audience,
                claims,
                expires: expires,
                signingCredentials: credentials
            );
            // our token
            var token = new JwtSecurityTokenHandler().WriteToken(jwtToken);

            return new JwtToken
            {
                Key = token,
                RefreshToken = refreshToken,
                Expires = expires,
                UserId = user.Id,
                UserName = user.Login,
                Roles = user.Roles != null ? user.Roles.Select(_ => _.Name).ToList() : null
            };
        }

        private string GenerateRefreshToken()
        {
            using (var rngCryptoServiceProvider = new RNGCryptoServiceProvider())
            {
                var randomBytes = new byte[64];
                rngCryptoServiceProvider.GetBytes(randomBytes);
                var result = new RefreshToken
                {
                    Token = Convert.ToBase64String(randomBytes),
                    Expires = DateTime.UtcNow.AddDays(7),
                    Created = DateTime.UtcNow
                };

                return JsonConvert.SerializeObject(result);
            }
        }

        public static ClaimsPrincipal TryGetPrincipalFromTokenWithoutKey(string token,
            Action<TokenValidationParameters> configAction = null)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true //here we are saying that we don't care about the token's expiration date
            };

            configAction?.Invoke(tokenValidationParameters);

            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");
            return principal;
        }
    }
}

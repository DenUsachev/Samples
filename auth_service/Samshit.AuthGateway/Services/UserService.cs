using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.Web.CodeGeneration;
using Samshit.AuthGateway.Extensions;
using Samshit.AuthGateway.Interfaces;
using Samshit.AuthGateway.Models;
using Samshit.DataModels.ErrorDefinitions;
using Samshit.DataModels.UserDomain;
using Samshit.DbAccess.Postgre;
using Samshit.WebUtils;

namespace Samshit.AuthGateway.Services
{
    public class UserService : IUserService
    {
        private readonly SamshitDbContext _ctx;
        private readonly IEmailService _emailService;
        private readonly ILogger<UserService> _logger;
        private bool _checkTokenLifetime = true;

        public UserService(DbContext ctx, IEmailService emailService, ILogger<UserService> logger)
        {
            _ctx = (SamshitDbContext) ctx;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<UserModelSecuredDto> GetUserByRefreshToken(string token)
        {
            var tokenUser = await _ctx.Users.SingleOrDefaultAsync(it => it.RefreshToken == token);
            return tokenUser?.ToDomainSecure();
        }

        public async Task<UserModelSecuredDto> GetUserByBearer(string token)
        {
            var tokenSafe = token.Trim();
            var tokenUser = await _ctx.Users.SingleOrDefaultAsync(it => it.AccessToken == tokenSafe);
            if (tokenUser != null)
            {
#if DEBUG
                _checkTokenLifetime = false;
#endif
                switch (_checkTokenLifetime)
                {
                    case true:
                    {
                        var tokenLifetime = DateTime.Now - tokenUser.TokenIssued;
                        if ((tokenLifetime?.TotalMinutes ?? AuthOptions.LIFETIME_IN_MIN + 1) <
                            AuthOptions.LIFETIME_IN_MIN)
                        {
                            return tokenUser?.ToDomainSecure();
                        }

                        break;
                    }
                    default:
                        return tokenUser?.ToDomainSecure();
                }
            }

            return null;
        }

        public async Task<string> GetToken(string login)
        {
            var existUser = await _ctx.Users.SingleOrDefaultAsync(it => it.Login == login);
            var token = string.Empty;
            if (existUser != null)
            {
                var tokenLifetime = DateTime.Now - existUser.TokenIssued;
                if ((tokenLifetime?.TotalMinutes ?? AuthOptions.LIFETIME_IN_MIN + 1) < AuthOptions.LIFETIME_IN_MIN)
                {
                    token = existUser.LoginToken;
                }
                else
                {
                    token = PasswordHelper.GetMd5Hash($"{login}_{DateTime.Now.ToLongTimeString()}");
                    existUser.LoginToken = token;
                    existUser.Updated = DateTime.Now;
                    existUser.TokenIssued = DateTime.Now;
                    await _ctx.SaveChangesAsync();
                }
            }
            else
            {
                return null;
            }

            return token;
        }

        public async Task<bool> TokenExists(string token)
        {
            return await _ctx.Users.AnyAsync(it => it.AccessToken == token);
        }

        public async Task<UserAuthResult> Login(string token, string secret)
        {
            var existUser = await _ctx.Users.SingleOrDefaultAsync(it => it.LoginToken == token);
            if (existUser == null)
            {
                return null;
            }

            var issuedTimeSpan = DateTime.Now - existUser.TokenIssued;
            if (issuedTimeSpan?.TotalMinutes < AuthOptions.LIFETIME_IN_MIN)
            {
                if (PasswordHelper.ValidatePassword(secret, existUser.Password))
                {
                    var outModel = existUser.ToAuthResult();
                    var refreshToken = GetRefreshToken();
                    var accessToken = GetAccessToken(outModel);

                    existUser.AccessToken = accessToken;
                    existUser.RefreshToken = refreshToken.Token;
                    existUser.Updated = DateTime.Now;
                    await _ctx.SaveChangesAsync();

                    outModel.AccessToken = accessToken;
                    outModel.RefreshToken = refreshToken.Token;
                    return outModel;
                }

                return null;
            }

            return null;
        }

        public async Task<bool> Logout(string email)
        {
            var result = false;
            try
            {
                var existUser = await _ctx.Users.SingleOrDefaultAsync(it => it.Login == email);
                if (existUser != null)
                {
                    existUser.AccessToken = null;
                    existUser.RefreshToken = null;
                    existUser.Updated = DateTime.Now;
                    await _ctx.SaveChangesAsync();
                    result = true;
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"Failed to logout user: {e.Message}", e);
            }

            return result;
        }

        public async Task<UserAuthResult> Create(UserModelDto model)
        {
            if (model.LinkedAccounts == null || model.LinkedAccounts.Length == 0)
            {
                throw new Exception(ErrorCodes.ModelError);
            }

            var createdUser = new User
            {
                Created = DateTime.Now,
                Updated = DateTime.Now,
                Login = model.Login,
                PermissionsMask = 1,
                Password = PasswordHelper.HashPassword(model.Password),
            };
            await _ctx.AddAsync(createdUser);
            var changesQty = await _ctx.SaveChangesAsync();
            if (changesQty > 0 && createdUser.Id > 0 && model.LinkedAccounts != null && model.LinkedAccounts.Length > 0)
            {
                model.UserId = createdUser.Id;
                createdUser.RefreshToken = GetRefreshToken().Token;
                createdUser.AccessToken = GetAccessToken(model);
                foreach (var account in model.LinkedAccounts)
                {
                    var dbEntity = account.ToDbEntity();
                    dbEntity.UserId = createdUser.Id;
                    dbEntity.AccountType = (int) AccountType.Internal;
                    dbEntity.Created = DateTime.Now;
                    dbEntity.Updated = DateTime.Now;
                    await _ctx.AddAsync(dbEntity);
                    await _ctx.SaveChangesAsync();
                }
            }
            else
            {
                throw new Exception(ErrorCodes.ModelAddError);
            }

            var outModel = createdUser.ToAuthResult();
            return outModel;
        }

        public Task<SessionResponseDto> CreateWidgetSessionToken(SessionRequestDto requestDto)
        {
            return Task.Run(() =>
            {
                const string WidgetAudience = "WeyrWidget";
                var sessionId = Guid.NewGuid().ToString();
                var widgetClaims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Jti, sessionId),
                    new Claim(ClaimNames.USER_ID, requestDto.UserId),
                    new Claim(ClaimNames.NAME, requestDto.UserId),
                    new Claim("campaign_id", requestDto.CampaignId.ToString()),
                    new Claim("role", "siteWidget"),
                    new Claim("role", "user")
                };
                var jwtToken = GetAccessToken(null, WidgetAudience, widgetClaims);
                return new SessionResponseDto {Id = sessionId, jwtToken = jwtToken, IsSuccess = true};
            });
        }

        public async Task<RefreshTokenResult?> RefreshToken(string login, string token)
        {
            var currentUser = await Get(login);
            if (currentUser != null && currentUser.RefreshToken == token)
            {
                var model = currentUser.ToAuthResult();
                var accessToken = GetAccessToken(model);
                var refreshToken = GetRefreshToken();
                model.AccessToken = accessToken;
                model.RefreshToken = refreshToken.Token;
                model.Updated = DateTime.Now;

                currentUser.AccessToken = accessToken;
                currentUser.RefreshToken = refreshToken.Token;
                currentUser.Updated = DateTime.Now;
                await _ctx.SaveChangesAsync();
                return new RefreshTokenResult(accessToken, refreshToken.Token);
            }

            return null;
        }

        public async Task<UserAuthResult> ResetPassword(UserModelUpdatePasswordDto model)
        {
            var user = await _ctx.Users.SingleOrDefaultAsync(it => it.RecoveryToken == model.RecoveryToken);
            if (user == null)
            {
                return null;
            }

            var issuedTimeSpan = DateTime.Now - user.RecoveryTokenIssued;
            if (issuedTimeSpan?.TotalHours < AuthOptions.PASSWD_TOKEN_LIFETIME_IN_HOURS)
            {
                user.Password = PasswordHelper.HashPassword(model.Password);
                user.RecoveryToken = null;
                user.RecoveryTokenIssued = null;
                user.Updated = DateTime.Now;

                var updatedModel = user.ToAuthResult();
                var accessToken = GetAccessToken(updatedModel);
                var refreshToken = GetRefreshToken();

                updatedModel.AccessToken = accessToken;
                updatedModel.RefreshToken = refreshToken.Token;

                user.AccessToken = accessToken;
                user.RefreshToken = refreshToken.Token;

                await _ctx.SaveChangesAsync();
                return updatedModel;
            }

            return null;
        }

        public async Task<bool> UpdatePassword(UserModelDto model)
        {
            try
            {
                var dbUser = await _ctx.Users.SingleOrDefaultAsync(it => it.Login == model.Login);
                if (dbUser == null)
                {
                    return false;
                }

                // update old password with new one
                if (!string.IsNullOrEmpty(model.Password) && !string.IsNullOrEmpty(model.OldPassword))
                {
                    if (PasswordHelper.ValidatePassword(model.OldPassword, dbUser.Password))
                    {
                        dbUser.Password = PasswordHelper.HashPassword(model.Password);
                        dbUser.Updated = DateTime.Now;
                        await _ctx.SaveChangesAsync();
                        return true;
                    }
                }

                // set the password for the first time 
                if (!string.IsNullOrEmpty(model.Password) && string.IsNullOrEmpty(dbUser.Login) &&
                    string.IsNullOrEmpty(dbUser.Password))
                {
                    dbUser.Password = PasswordHelper.HashPassword(model.Password);
                    dbUser.Updated = DateTime.Now;
                    await _ctx.SaveChangesAsync();
                    return true;
                }

                return false;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to update user object {@Model}", model);
                return false;
            }
        }

        public async Task<UserModelSecuredDto> Update(UserModelDto model)
        {
            try
            {
                var dbUser = await _ctx.Users.SingleOrDefaultAsync(it => it.Login == model.Login);
                if (dbUser == null)
                {
                    return null;
                }

                var linkedAccounts = model.LinkedAccounts.Select(it => it.ToDbEntity());
                dbUser.UserAccounts = linkedAccounts.ToArray();
                model.Updated = DateTime.Now;
                dbUser.Updated = model.Updated.Value;

                await _ctx.SaveChangesAsync();
                return model;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to update user object {@Model}", model);
                return null;
            }
        }

        public async Task<User> Get(string login)
        {
            return await _ctx.Users.Include(it => it.UserAccounts).SingleOrDefaultAsync(it => it.Login == login);
        }

        public async Task<UserAuthResult> LoginWithSocialId(SocialUserModel model, AccountType accountType)
        {
            UserAuthResult authenticatedUser;
            UserAccount userAccount = await _ctx.UserAccounts.Include(it => it.User)
                .SingleOrDefaultAsync(it => it.SocialId == model.Id);
            if (userAccount == null)
            {
                var timestamp = DateTime.Now;
                userAccount = new UserAccount
                {
                    SocialId = model.Id,
                    AccountType = (int) accountType,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    LastLogin = timestamp,
                    Created = timestamp,
                    Updated = timestamp
                };

                // if we have no user account but have an e-mail, trying to find base user according to it
                if (!string.IsNullOrEmpty(model.Email))
                {
                    var user = await _ctx.Users.Include(it => it.UserAccounts)
                        .SingleOrDefaultAsync(it => it.Login == model.Email);
                    if (user == null)
                    {
                        // if there is user with such email - trying to add social account to it
                        user = new User
                        {
                            Created = timestamp,
                            Updated = timestamp,
                            Login = model.Email,
                            PermissionsMask = 1,
                            Password = null,
                        };
                        await _ctx.AddAsync(user);
                        var changesQty = await _ctx.SaveChangesAsync();
                        if (changesQty > 0 && user.Id > 0)
                        {
                            authenticatedUser = user.ToAuthResult();
                            userAccount.UserId = user.Id;

                            var accessToken = GetAccessToken(authenticatedUser);
                            var refreshToken = GetRefreshToken().Token;

                            authenticatedUser.AccessToken = accessToken;
                            authenticatedUser.RefreshToken = refreshToken;

                            user.AccessToken = accessToken;
                            user.RefreshToken = refreshToken;
                            user.Updated = DateTime.Now;
                            await _ctx.UserAccounts.AddAsync(userAccount);
                            await _ctx.SaveChangesAsync();
                        }
                        else
                        {
                            const string err =
                                "User Account Create Routine failed due to DB issue. Cannot add row to [users].";
                            _logger.LogError(err);
                            throw new Exception(err);
                        }
                    }
                    else
                    {
                        await _ctx.UserAccounts.AddAsync(userAccount);
                        authenticatedUser = user.ToAuthResult();

                        var accessToken = GetAccessToken(authenticatedUser);
                        var refreshToken = GetRefreshToken().Token;

                        authenticatedUser.AccessToken = accessToken;
                        authenticatedUser.RefreshToken = refreshToken;

                        user.AccessToken = accessToken;
                        user.RefreshToken = refreshToken;
                        user.Updated = DateTime.Now;
                        await _ctx.SaveChangesAsync();
                    }
                }
                else
                {
                    var user = new User
                    {
                        Created = timestamp,
                        Updated = timestamp,
                        Login = null,
                        PermissionsMask = 1,
                        Password = null,
                    };
                    await _ctx.AddAsync(user);
                    var changesQty = await _ctx.SaveChangesAsync();
                    if (changesQty > 0 && user.Id > 0)
                    {
                        userAccount = new UserAccount
                        {
                            UserId = user.Id,
                            SocialId = model.Id,
                            AccountType = (int) accountType,
                            Email = null,
                            FirstName = model.FirstName,
                            LastName = model.LastName,
                            LastLogin = timestamp,
                            Created = timestamp,
                            Updated = timestamp
                        };
                        authenticatedUser = userAccount.User.ToAuthResult();
                        var accessToken = GetAccessToken(authenticatedUser);
                        var refreshToken = GetRefreshToken().Token;

                        authenticatedUser.AccessToken = accessToken;
                        authenticatedUser.RefreshToken = refreshToken;

                        user.AccessToken = accessToken;
                        user.RefreshToken = refreshToken;
                        user.Updated = DateTime.Now;
                        await _ctx.SaveChangesAsync();
                    }
                    else
                    {
                        const string err =
                            "User Account Create Routine failed due to DB issue. Cannot add row to [users].";
                        _logger.LogError(err);
                        throw new Exception(err);
                    }
                }
            }
            else
            {
                userAccount.FirstName = model.FirstName;
                userAccount.LastName = model.LastName;
                userAccount.LastLogin = DateTime.Now;

                authenticatedUser = userAccount.User.ToAuthResult();
                var accessToken = GetAccessToken(authenticatedUser);
                var refreshToken = GetRefreshToken().Token;

                authenticatedUser.AccessToken = accessToken;
                authenticatedUser.RefreshToken = refreshToken;

                userAccount.User.AccessToken = accessToken;
                userAccount.User.RefreshToken = refreshToken;
                userAccount.User.Updated = DateTime.Now;
                userAccount.Updated = DateTime.Now;
                await _ctx.SaveChangesAsync();
            }

            return authenticatedUser;
        }

        public async Task<bool> RequestPasswordRestore(UserModelDto model)
        {
            User user = default;
            try
            {
                user = await _ctx.Users.SingleOrDefaultAsync(it => it.Login == model.Login);
                if (user != null)
                {
                    string recoveryToken;
                    if (user.RecoveryTokenIssued.HasValue
                        && (DateTime.Now - user.RecoveryTokenIssued.Value).TotalHours <=
                        AuthOptions.PASSWD_TOKEN_LIFETIME_IN_HOURS)
                    {
                        recoveryToken = user.RecoveryToken;
                    }
                    else
                    {
                        recoveryToken = GetRecoveryToken();
                        user.RecoveryToken = recoveryToken;
                        user.RecoveryTokenIssued = DateTime.Now;
                        await _ctx.SaveChangesAsync();
                    }

                    var message = new EmailMessage
                    {
                        To = model.Login,
                        From = "robot@samshit.club",
                        Subject = "User Account Password Recovery",
                        Body = $"Recovery code for {user.Login}: {recoveryToken}",
                    };
                    return await _emailService.SendMessageAsync(message);
                }

                return false;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to issue recovery token. {@User}", user);
                return false;
            }
        }

        private static string GetRecoveryToken()
        {
            var rnd = new Random();
            var token = string.Join(string.Empty, Enumerable.Range(0, 6).Select(it => rnd.Next(1, 9)));
            return token;
        }

        private static string GetAccessToken(UserModelSecuredDto model, string audience = null, IEnumerable<Claim> extraClaims = null)
        {
            var currentDateTime = DateTime.Now;
            var tokenAudience = audience ?? AuthOptions.AUDIENCE;
            var totalClaims = new List<Claim>();
            if (model != null)
            {
                var authorizedIdentity = AuthorizeUser(model);
                totalClaims.AddRange(authorizedIdentity.Claims);
            }

            if (extraClaims != null)
            {
                totalClaims.AddRange(extraClaims);
            }

            var jwt = new JwtSecurityToken(
                issuer: AuthOptions.ISSUER,
                audience: tokenAudience,
                notBefore: currentDateTime,
                claims: totalClaims,
                expires: currentDateTime.Add(TimeSpan.FromMinutes(AuthOptions.LIFETIME_IN_MIN)),
                signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(),
                    SecurityAlgorithms.HmacSha256));

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
            return encodedJwt;
        }

        private static RefreshToken GetRefreshToken( /*string ipAddress*/)
        {
            using var rngCryptoServiceProvider = new RNGCryptoServiceProvider();
            var randomBytes = new byte[64];
            rngCryptoServiceProvider.GetBytes(randomBytes);
            return new RefreshToken
            {
                Token = Convert.ToBase64String(randomBytes),
                Expires = DateTime.Now.AddDays(7),
                Created = DateTime.Now,
            };
        }

        private static ClaimsIdentity AuthorizeUser(UserModelSecuredDto model)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimNames.USER_ID, model.UserId.ToString()),
                new Claim(ClaimNames.NAME, model.Login),
                new Claim("role", "admin"),
                new Claim("role", "user"),
            };
            var claimsIdentity = new ClaimsIdentity(claims, "Negotiate", ClaimNames.NAME, ClaimNames.ROLE);
            return claimsIdentity;
        }
    }
}
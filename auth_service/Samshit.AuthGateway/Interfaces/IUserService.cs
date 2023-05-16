using System.Threading.Tasks;
using Samshit.AuthGateway.Models;
using Samshit.DataModels.UserDomain;
using Samshit.DbAccess.Postgre;

namespace Samshit.AuthGateway.Interfaces
{
    public interface IUserService
    {
        Task<User> Get(string login);
        Task<UserAuthResult> LoginWithSocialId(SocialUserModel model, AccountType accountType);
        
        
        Task<UserAuthResult> Create(UserModelDto model);
        Task<UserModelSecuredDto> Update(UserModelDto model);
        Task<bool> UpdatePassword(UserModelDto model);
        Task<bool> RequestPasswordRestore(UserModelDto model);
        Task<UserAuthResult> ResetPassword(UserModelUpdatePasswordDto model);


        Task<SessionResponseDto> CreateWidgetSessionToken(SessionRequestDto requestDto);
        Task<RefreshTokenResult> RefreshToken(string login, string token);
        Task<bool> TokenExists(string token);
        Task<UserModelSecuredDto> GetUserByRefreshToken(string token);
        Task<UserModelSecuredDto> GetUserByBearer(string token);
        Task<string> GetToken(string login);
        Task<UserAuthResult> Login(string token, string secret);
        Task<bool> Logout(string email);
    }
}

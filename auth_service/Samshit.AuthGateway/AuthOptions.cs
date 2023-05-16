using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Samshit.AuthGateway
{
    public class AuthOptions
    {
        const string KEY = "LxRbLX2LekvlFuHNxgQZiBa7yzdyoIES";

        public const string ISSUER = "samshitAuth"; 
        public const string AUDIENCE = "samshitUsers"; 
        public const string AUDIENCE_WEB = "samshitEmployee"; 
        public const int LIFETIME_IN_MIN = 10;
        public const int PASSWD_TOKEN_LIFETIME_IN_HOURS = 1;

        public static SymmetricSecurityKey GetSymmetricSecurityKey()
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(KEY));
        }
    }
}

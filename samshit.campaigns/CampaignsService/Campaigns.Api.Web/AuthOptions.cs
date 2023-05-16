using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Campaigns.Api.Web
{
    public class AuthOptions
    {
        const string KEY = "LxRbLX2LekvlFuHNxgQZiBa7yzdyoIES";

        public const string ISSUER = "samshitAuth";
        public const string AUDIENCE = "samshitUsers";
        public const int LIFETIME_IN_MIN = 10;

        public static SymmetricSecurityKey GetSymmetricSecurityKey()
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(KEY));
        }
    }
}
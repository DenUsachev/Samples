namespace Samshit.AuthGateway.Models
{
    public class RefreshTokenResult
    {
        public RefreshTokenResult(string accessToken, string refreshToken)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
        }

        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}

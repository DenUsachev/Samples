using System.Security.Cryptography;
using System.Text;

namespace Samshit.WebUtils
{
    public static class PasswordHelper
    {
        private static MD5 _md5;

        static PasswordHelper()
        {
            _md5 = MD5.Create();
        }

        private static string GetRandomSalt()
        {
            return BCrypt.BCryptHelper.GenerateSalt(12);
        }

        public static string GetMd5Hash(string input)
        {
            byte[] data = _md5.ComputeHash(Encoding.UTF8.GetBytes(input));
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }

        public static string HashPassword(string password)
        {
            return BCrypt.BCryptHelper.HashPassword(password, GetRandomSalt());
        }

        public static bool ValidatePassword(string password, string correctHash)
        {
            return BCrypt.BCryptHelper.CheckPassword(password, correctHash);
        }
    }


}

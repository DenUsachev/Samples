using System;
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

        public static string GetDomainKey(string domainName)
        {
            const int KEY_LENGTH = 32;
            var rng = RandomNumberGenerator.Create();
            var salt = new byte[KEY_LENGTH];
            rng.GetBytes(salt);

            // todo think about encryption!
            // var sourceStringBytes = Encoding.ASCII.GetBytes(domainName);
            // var outputBytes = new byte[salt.Length + sourceStringBytes.Length];
            // Buffer.BlockCopy(sourceStringBytes, 0, outputBytes, 0, sourceStringBytes.Length);
            // buffer.BlockCopy(salt, 0, outputBytes, sourceStringBytes.Length, salt.Length);

            //var intNumbers = Array.ConvertAll(outputBytes, b => (uint)b);
            //var shiftedBytes = new byte[intNumbers.Length * sizeof(uint)];

            //int i, offset;
            //for (i = 0, offset = 0; i < intNumbers.Length; i++, offset += sizeof(uint))
            //{
            //    intNumbers[i] = (intNumbers[i] >> 2);
            //    byte[] bytes = BitConverter.GetBytes(intNumbers[i]);
            //    //if (BitConverter.IsLittleEndian)
            //    //    Array.Reverse(bytes);
            //    Buffer.BlockCopy(bytes, 0, shiftedBytes, offset, sizeof(uint));
            //}
            //Buffer.BlockCopy(numbers, 0, shiftedBytes, 0, shiftedBytes.Length);

            return Convert.ToBase64String(salt);
        }
    }


}

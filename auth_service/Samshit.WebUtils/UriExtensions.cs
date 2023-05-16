using System;

namespace Samshit.WebUtils
{
    public static class UriExtensions
    {
        private const char TOKEN = '?';
        public static string TrimTokenizedString(this Uri uri)
        {
            var tokenIndex = uri.AbsoluteUri.IndexOf(TOKEN);
            var trimmed = uri.AbsoluteUri.Substring(0, tokenIndex);
            return trimmed;
        }
    }
}

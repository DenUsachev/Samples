using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samshit.WebUtils;
using Xunit;

namespace Campaigns.Api.Tests
{
    public class GetDomainKeyTest
    {

        private const int ROUNDS = 1000000;

        [Theory]
        [InlineData("www.yandex.ru", "a1.yandex.ru", "yandex.ru")]
        public void GetDomainKeyUniqTest(string wwwdomain, string subdomain, string sndLevelDomain)
        {
            Dictionary<string, int> check = new Dictionary<string, int>();
            string[] domains = new[] {wwwdomain, subdomain, sndLevelDomain};
            foreach (var domain in domains)
            {
                for (int i = 0; i < ROUNDS; i++)
                {
                    var key = PasswordHelper.GetDomainKey(domain);
                    if (check.TryGetValue(key, out var exist))
                    {
                        check[key] = exist++;
                    }
                    else
                    {
                        check[key] = 1;
                    }
                }
            }
            Assert.True(check.Values.All(x => x == 1));
        }
    }
}

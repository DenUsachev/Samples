using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Samshit.AuthGateway.Interfaces;
using Samshit.AuthGateway.Models;

namespace Samshit.AuthGateway.Services
{
    public class VkAuthProvider : IOAuthProvider
    {
        private const string API_VER = "5.107";
        public string ClientId { get; protected set; }
        public string Secret { get; protected set; }
        public string Token { get; protected set; }
        public string AccessFlags { get; set; }
        public string RedirectUri { get; set; }

        private VkAuthResponse _vkAuthKey;

        private struct VkAuthResponse
        {

            public string access_token { get; set; }
            public int expires_in { get; set; }
            public int user_id { get; set; }
        }

        private struct VkUserTicket
        {
            public int id { get; set; }
            public string first_name { get; set; }
            public string last_name { get; set; }
        }

        public VkAuthProvider(string clientId, string secret)
        {
            ClientId = clientId;
            Secret = secret;
        }

        public string GetServiceUri(Uri callbackEndpoint)
        {
            var authUrl = new StringBuilder();
            RedirectUri = callbackEndpoint.AbsoluteUri;
            authUrl.Append("https://oauth.vk.com/authorize?");
            authUrl.AppendFormat("client_id={0}&", ClientId);
            if (!string.IsNullOrEmpty(AccessFlags))
                authUrl.AppendFormat("scope={0}&", AccessFlags);
            authUrl.AppendFormat("redirect_uri={0}&", RedirectUri);
            authUrl.Append("response_type=code");
            return authUrl.ToString();
        }

        public string AcquireToken(string code)
        {
            var tokenUrl = string.Format("https://oauth.vk.com/access_token?client_id={0}&client_secret={1}&code={2}&redirect_uri={3}", ClientId, Secret, code, RedirectUri);
            using (var httpClient = new WebClient())
            {
                var response = httpClient.DownloadData(tokenUrl);
                if (response.Length > 0)
                {
                    var jsonResponse = Encoding.UTF8.GetString(response);
                    var vkAuthObject = JsonConvert.DeserializeObject<VkAuthResponse>(jsonResponse);
                    _vkAuthKey = vkAuthObject;
                    return vkAuthObject.access_token;
                }
            }
            return null;
        }

        public dynamic ExecuteQuery(Uri queryUrl)
        {
            using (var httpClient = new WebClient())
            {
                var response = httpClient.DownloadData(queryUrl.AbsoluteUri + string.Format("?uid={0}&access_token={1}&v={2}", _vkAuthKey.user_id, _vkAuthKey.access_token, API_VER));
                if (response.Length > 0)
                {
                    var jsonResponse = Encoding.UTF8.GetString(response);
                    var vkResponse = JsonConvert.DeserializeObject<Dictionary<string, VkUserTicket[]>>(jsonResponse);
                    if (vkResponse != null)
                    {
                        var vkObject = vkResponse["response"][0];
                        return new VkUserModel
                        {
                            Id = vkObject.id,
                            FirstName = vkObject.first_name,
                            LastName = vkObject.last_name
                        };
                    }
                }
            }
            return null;
        }
    }
}
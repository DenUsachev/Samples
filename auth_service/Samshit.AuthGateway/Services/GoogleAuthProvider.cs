using Newtonsoft.Json;
using Samshit.AuthGateway.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Text;

namespace Samshit.AuthGateway.Services
{
    internal static class GoogleScope
    {
        public const string Email = "https://www.googleapis.com/auth/userinfo.email";
        public const string Profile = "https://www.googleapis.com/auth/userinfo.profile";
    }

    internal class GoogleAuthProvider : IOAuthProvider
    {
        public string ClientId { get; set; }
        public string Secret { get; set; }
        public string Token { get; protected set; }
        public string AccessFlags { get; set; }
        public string CallbackUri { get; set; }

        public string GetServiceUri(Uri callbackEndpoint)
        {
            var authUrl = new StringBuilder();
            authUrl.Append("https://accounts.google.com/o/oauth2/auth?");
            authUrl.AppendFormat("client_id={0}&", ClientId);
            if (!string.IsNullOrEmpty(AccessFlags))
                authUrl.AppendFormat("scope={0}&", AccessFlags);
            authUrl.AppendFormat("redirect_uri={0}&", callbackEndpoint.AbsoluteUri);
            authUrl.Append("response_type=code");
            CallbackUri = callbackEndpoint.AbsoluteUri;
            return authUrl.ToString();
        }

        public string AcquireToken(string code)
        {
            using (var httpClient = new WebClient())
            {
                var token = new NameValueCollection();
                token["client_id"] = ClientId;
                token["client_secret"] = Secret;
                token["redirect_uri"] = CallbackUri;
                token["code"] = code;
                token["grant_type"] = "authorization_code";
                var response = httpClient.UploadValues("https://accounts.google.com/o/oauth2/token", "POST", token);
                if (response.Length > 0)
                {
                    var jsonResponse = httpClient.Encoding.GetString(response);
                    var jsonData = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonResponse);
                    if (!string.IsNullOrEmpty(jsonData["access_token"]))
                    {
                        Token = jsonData["access_token"];
                        return Token;
                    }
                }
                return null;
            }
        }

        public dynamic ExecuteQuery(Uri queryUrl)
        {
            using (var httpClient = new WebClient())
            {
                var response = httpClient.DownloadData(queryUrl.AbsoluteUri + "?access_token=" + Token);
                if (response.Length > 0)
                {
                    var jsonResponse = Encoding.UTF8.GetString(response);
                    return JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonResponse);
                }
            }
            return null;
        }
    }
}

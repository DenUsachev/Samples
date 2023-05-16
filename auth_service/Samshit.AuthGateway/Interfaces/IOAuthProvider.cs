using System;

namespace Samshit.AuthGateway.Interfaces
{
    internal interface IOAuthProvider
    {
        string ClientId { get; }
        string Secret { get; }
        string Token { get; }
        string AccessFlags { get; }

        string GetServiceUri(Uri callbackEndpoint);
        string AcquireToken(string code);
        dynamic ExecuteQuery(Uri queryUrl);
    }
}

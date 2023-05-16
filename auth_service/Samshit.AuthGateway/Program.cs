using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Net;
using Microsoft.AspNetCore.Server.Kestrel.Core;


namespace Samshit.AuthGateway
{
    public class Program
    {
        private const int DEFAULT_PORT = 8890;
        private const int DEFAULT_PORT_GRPC = 10000;
        private const string DEFAULT_ENVIRONMENT = "Development";
        private static string _environment;
        private static string _sentryDsn;
        public static void Main(string[] args)
        {
            _environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? DEFAULT_ENVIRONMENT;
            _sentryDsn = Environment.GetEnvironmentVariable("SENTRY_DSN") ?? string.Empty;
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>()
                    .UseKestrel(opts =>
                    {
                        opts.AddServerHeader = false;
                        opts.Listen(IPAddress.Any, DEFAULT_PORT);
                        opts.Listen(IPAddress.Any, DEFAULT_PORT_GRPC, o =>
                        {
                            o.Protocols = HttpProtocols.Http2;
                        });
                    });

#if !DEBUG
                    if (!string.IsNullOrEmpty(_sentryDsn))
                    {
                        webBuilder.UseSentry(sentry =>
                        {
                            sentry.Environment = _environment;
                            sentry.Debug = _environment == DEFAULT_ENVIRONMENT;
                            sentry.Dsn = _sentryDsn;
                        });
                    }
#endif
                });
    }
}

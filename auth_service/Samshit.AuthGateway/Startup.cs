using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Samshit.AuthGateway.Interfaces;
using Samshit.AuthGateway.Services;
using Samshit.DbAccess.Postgre;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Samshit.AuthGateway.Extensions;
using Samshit.AuthGateway.FIlters;
using Serilog.Events;

namespace Samshit.AuthGateway
{
    public class Startup
    {
        private const string DEFAULT_ENVIRONMENT = "Development";
        private const string CORS_POLICY = "Localhost-tolerant";
        private static string[] _allowedOrigins =
        {
            "http://localhost:3000",
            "https://localhost:3000",
            "http://localhost:3001",
            "https://localhost:3001",
            "http://localhost:3002",
            "https://localhost:3002",
            "http://localhost:3003",
            "https://localhost:3003",
            "http://localhost:3004",
            "https://localhost:3004",
            "http://localhost:3005",
            "https://localhost:3005",
            "http://weyr.io",
            "https://weyr.io",
            "http://*.weyr.io",
            "https://*.weyr.io"
        };

        private static void RemoveLocalhostOrigins()
        {
            var nonLocalhostOrigins = _allowedOrigins.Where(it => !it.Contains("localhost")).ToArray();
            _allowedOrigins = nonLocalhostOrigins;
        }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            var sentryDsn = Environment.GetEnvironmentVariable("SENTRY_DSN") ?? string.Empty;
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? DEFAULT_ENVIRONMENT;

            Log.Logger = new LoggerConfiguration()
           .Enrich.FromLogContext()
           .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level}] {Message:lj}{NewLine}{Exception}")
#if  !DEBUG
           .WriteTo.Sentry(sentryDsn, environment: environment, restrictedToMinimumLevel: LogEventLevel.Error)
#endif
           .CreateLogger();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));

            Log.Logger.Warning("*********** Registering Email Service ***********");
            services.AddInternalEmailService();

            services.AddCors(options =>
            {
                options.AddPolicy(CORS_POLICY,
                    builder =>
                    {
                        builder.SetIsOriginAllowedToAllowWildcardSubdomains()
                        .WithOrigins(_allowedOrigins)
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                    });
            });
            services.AddGrpc();
            services.AddControllers().AddNewtonsoftJson();
            services.AddHttpContextAccessor();
            services.AddScoped<DbContext, SamshitDbContext>();
            services.AddScoped<IUserService, UserService>();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(opts =>
                {
                    opts.RequireHttpsMetadata = false;
                    opts.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidIssuers = new List<string> { AuthOptions.ISSUER },
                        ValidateIssuer = false,
                        ValidAudiences = new List<string> { AuthOptions.AUDIENCE },
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
                        ValidateIssuerSigningKey = true
                    };
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            Log.Logger.Warning($"*********** Starting Web host ***********");
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors(CORS_POLICY);
            if (env.IsProduction())
            {
                RemoveLocalhostOrigins();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "auth/v1/{controller}/{action}/{id?}");
                endpoints.MapControllers();
                endpoints.MapGrpcService<AuthGrpcBridge>(); 
            });
        }
    }
}

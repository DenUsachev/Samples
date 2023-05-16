using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Campaigns.Api.Web.Filters;
using Campaigns.Api.Web.Interfaces;
using Campaigns.Api.Web.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Samshit.DbAccess.Postgre.Campaigns;
using Serilog;
using Serilog.Events;

namespace Campaigns.Api.Web
{
    public class Startup
    {
        private const string SWAGGER_DOCUMENT_NAME = "v1";
        private const string SERVICE_NAME = "Campaigns Service";
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
            "https://samshit.club",
            "http://samshit.club",
            "https://*.samshit.club",
            "http://*.samshit.club"
        };

        private static void RemoveLocalhostOrigins()
        {
            var nonLocalhostOrigins = _allowedOrigins.Where(it => !it.Contains("localhost")).ToArray();
            _allowedOrigins = nonLocalhostOrigins;
        }

        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? DEFAULT_ENVIRONMENT;
            var sentryDsn = Environment.GetEnvironmentVariable("SENTRY_DNS") ?? DEFAULT_ENVIRONMENT;
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Env", environment)
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .WriteTo.Console(
                    outputTemplate: "[{Env} {Timestamp:yyyy-MM-dd HH:mm:ss} {Level}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.Sentry(sentryDsn, environment: environment, restrictedToMinimumLevel: LogEventLevel.Error)
                .CreateLogger();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));
            Log.Logger.Warning($"*********** Starting {SERVICE_NAME} ***********");
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
            services.AddControllers().AddNewtonsoftJson();
            services.AddGrpc();
            services.AddHttpContextAccessor();
            services.AddDbContext<DbContext, CampaignsContext>();
            services.AddScoped<ICampaignsService, CampaignsService>();
            services.AddScoped<IChannelService, ChannelService>();

            Log.Logger.Warning("*********** Registering the Swagger generator ***********");
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            Log.Logger.Warning($"Swagger file path: {xmlPath}.");
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(SWAGGER_DOCUMENT_NAME,
                    new Microsoft.OpenApi.Models.OpenApiInfo {Title = "Samshit Campaigns API", Version = "v1"});
                c.IncludeXmlComments(xmlPath);
            });
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(opts =>
                {
                    opts.RequireHttpsMetadata = false;
                    opts.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidIssuers = new List<string> {AuthOptions.ISSUER},
                        ValidateIssuer = false,
                        ValidAudiences = new List<string> {AuthOptions.AUDIENCE},
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

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapGrpcService<CampaignsGrpcBridge>();
            });
            app.UseSwagger(s => s.RouteTemplate = "campaigns/doc/{documentName}/swagger.json");
            app.UseSwaggerUI(ui =>
            {
                ui.SwaggerEndpoint($"{SWAGGER_DOCUMENT_NAME}/swagger.json", "Samshit Campaigns API v1 doc");
                ui.RoutePrefix = "campaigns/doc";
            });
        }
    }
}
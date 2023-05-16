using Microsoft.Extensions.DependencyInjection;
using System;
using Samshit.WebUtils;

namespace Samshit.AuthGateway.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInternalEmailService(this IServiceCollection services)
        {
            services.AddTransient<IEmailService, EmailService>(sp =>
            {
                var provider = new EnvironmentVariablesEmailConfigurationProvider();
                var configuration = provider.GetConfiguration();
                if (configuration == null)
                {
                    throw new ArgumentNullException(nameof(configuration), "Failed to get Email service configuration.");
                }
                return new EmailService(configuration);
            });
            return services;
        }
    }
}
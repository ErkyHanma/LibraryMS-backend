using LibraryMS_API.Core.Application.Interfaces;
using LibraryMS_API.Core.Domain.Settings;
using LinkUp.Infrastructure.Shared.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryMS_API.Infrastructure.Shared.IOC
{
    public static class ServiceRegistration
    {

        public static void AddSharedLayerIOC(this IServiceCollection services, IConfiguration config)
        {
            #region Configurations
            services.Configure<MailSettings>(config.GetSection("MailSettings"));
            #endregion

            #region Services IOC
            services.AddScoped<IEmailService, EmailService>();
            #endregion
        }

    }
}

using LibraryMS.Core.Application.Interfaces;
using LibraryMS.Core.Domain.Settings;
using LibraryMS.Infrastructure.Shared.Services;
using LinkUp.Infrastructure.Shared.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryMS.Infrastructure.Shared.IOC
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
            services.AddScoped<IEmailTemplateService, EmailTemplateService>();
            services.AddScoped<ICloudinaryService, CloudinaryService>();
            #endregion
        }

    }
}

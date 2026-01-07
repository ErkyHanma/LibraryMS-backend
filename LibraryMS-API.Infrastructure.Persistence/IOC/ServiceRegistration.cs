using LibraryMS_API.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryMS_API.Infrastructure.Persistence.IOC
{
    public static class ServiceRegistration
    {
        public static void AddPersistenceLayerIoc(this IServiceCollection services, IConfiguration config)
        {
            #region Contexts
            if (config.GetValue<bool>("UseInMemoryDatabase"))
            {
                services.AddDbContext<LibraryMSContext>(options =>
                {
                    options.UseInMemoryDatabase("LibraryMSInMemoryDb");
                });
            }
            else
            {
                var connectionString = config.GetValue<string>("ConnectionStrings:DefaultConnection");

                services.AddDbContext<LibraryMSContext>(
                    (serviceProvider, options) =>
                    {
                        options.EnableSensitiveDataLogging();
                        options.UseNpgsql(
                             connectionString,
                             options => options.MigrationsAssembly(typeof(LibraryMSContext).Assembly.FullName)
                        );
                    },
                    contextLifetime: ServiceLifetime.Scoped,
                    optionsLifetime: ServiceLifetime.Scoped
                );
            }
            #endregion


        }


    }
}

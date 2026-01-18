using LibraryMS.Core.Domain.Interfaces.Repositories;
using LibraryMS.Infrastructure.Persistence.Contexts;
using LibraryMS.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryMS.Infrastructure.Persistence.IOC
{
    public static class ServiceRegistration
    {
        public static void AddPersistenceLayerIOC(this IServiceCollection services, IConfiguration config)
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


            #region Repositories
            services.AddScoped<IBookRepository, BookRepository>();
            services.AddScoped<IBorrowRecordRepository, BorrowRecordRepository>();
            services.AddScoped<IAccountRequestRepository, AccountRequestRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            #endregion


        }

        #region Seeds

        private static readonly string[] Categories =
        [
            "Science Fiction",
            "Fantasy",
            "Programming",
            "Mathematics",
            "Software Architecture",
            "Education",
            "Design",
            "Psychology",
            "Horror",
            "Non-Fiction",
            "Historical Fiction",
            "Thriller",
            "Biography",
            "Self-Help"
        ];

        public static async Task RunLibrarySeedAsync(this IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<LibraryMSContext>();

            await DefaultCategory.SeedAsync(context, Categories);
        }

        #endregion
    }
}

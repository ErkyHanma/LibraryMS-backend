using FluentValidation;
using LibraryMS.Core.Application.Interfaces;
using LibraryMS.Core.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace LibraryMS.Core.Application.IOC
{
    public static class ServiceRegistration
    {
        public static void AddApplicationLayerIOC(this IServiceCollection services)
        {

            #region Configurations
            services.AddAutoMapper(Assembly.GetExecutingAssembly());
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            #endregion

            #region Services
            services.AddScoped<IBookService, BookService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IBorrowRecordService, BorrowRecordService>();
            services.AddScoped<IAccountRequestService, AccountRequestService>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<IValidationService, ValidationService>();
            #endregion
        }
    }
}

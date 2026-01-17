namespace LibraryMS.WebApi.Extensions
{
    public static class AppExtension
    {
        public static void UseSwaggerExtensions(this IApplicationBuilder app, IEndpointRouteBuilder routeBuilder)
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                var versionDescriptions = routeBuilder.DescribeApiVersions();
                if (versionDescriptions != null && versionDescriptions.Any())
                {
                    foreach (var description in versionDescriptions)
                    {
                        var url = $"/swagger/{description.GroupName}/swagger.json";
                        var name = $"LibraryMS API - {description.GroupName.ToUpperInvariant()}";

                        options.SwaggerEndpoint(url, name);
                    }
                }
                else
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "LibraryMS API v1.0");
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "LibraryMS API v1.0");
                    options.RoutePrefix = string.Empty;
                }
            });

        }
    }
}

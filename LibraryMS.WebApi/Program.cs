using LibraryMS.Core.Application.IOC;
using LibraryMS.Infrastructure.Identity.Contexts;
using LibraryMS.Infrastructure.Identity.IOC;
using LibraryMS.Infrastructure.Persistence.Contexts;
using LibraryMS.Infrastructure.Persistence.IOC;
using LibraryMS.Infrastructure.Shared.IOC;
using LibraryMS.WebApi.Extensions;
using LibraryMS.WebApi.Handlers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Controllers & JSON
builder.Services
    .AddControllers(options =>
    {
        options.Filters.Add(new ProducesAttribute("application/json"));
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        options.SuppressInferBindingSourcesForParameters = true;
        options.SuppressMapClientErrors = true;
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Layers 
builder.Services.AddPersistenceLayerIOC(builder.Configuration);
builder.Services.AddIdentityLayerIocForWebApi(builder.Configuration);
builder.Services.AddApplicationLayerIOC();
builder.Services.AddSharedLayerIOC(builder.Configuration);


builder.Services.AddHealthChecks();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();

// Swagger & Versioning
builder.Services.AddSwaggerExtension();
builder.Services.AddApiVersioningExtension();
builder.Services.AddApiVersioning();
builder.Services.AddOpenApi();

// CORS
var frontendUrl = builder.Configuration.GetValue<string>("Frontend:Url");

if (string.IsNullOrWhiteSpace(frontendUrl))
    throw new ArgumentException("Frontend:Url is missing.");

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .WithOrigins(frontendUrl)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});



var app = builder.Build();

if (!builder.Configuration.GetValue<bool>("UseInMemoryDatabase"))
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;

    var identityContext =
        services.GetRequiredService<IdentityContext>();

    var libraryContext =
        services.GetRequiredService<LibraryMSContext>();

    await identityContext.Database.MigrateAsync();
    await libraryContext.Database.MigrateAsync();
}

await app.Services.RunIdentitySeedAsync(builder.Configuration);
await app.Services.RunLibrarySeedAsync();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerExtensions(app);
    app.MapOpenApi();
}

app.UseCors();

app.UseHttpsRedirection();

app.UseExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

app.UseHealthChecks("/health");

app.MapControllers();

app.Run();

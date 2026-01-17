using LibraryMS.Core.Application.IOC;
using LibraryMS.Infrastructure.Identity.IOC;
using LibraryMS.Infrastructure.Persistence.IOC;
using LibraryMS.Infrastructure.Shared.IOC;
using LibraryMS.WebApi.Extensions;
using LibraryMS.WebApi.Handlers;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers(opt =>
{
    opt.Filters.Add(new ProducesAttribute("application/json"));
}).ConfigureApiBehaviorOptions(opt =>
{
    opt.SuppressInferBindingSourcesForParameters = true;
    opt.SuppressMapClientErrors = true;
}).AddJsonOptions(opt =>
{
    opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});



// Add services to the container.
builder.Services.AddPersistenceLayerIOC(builder.Configuration);
builder.Services.AddIdentityLayerIocForWebApi(builder.Configuration);
builder.Services.AddApplicationLayerIOC();
builder.Services.AddSharedLayerIOC(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();


// Documentation
builder.Services.AddSwaggerExtension();
builder.Services.AddApiVersioningExtension();
builder.Services.AddApiVersioning();

var app = builder.Build();

// Run seeds
await app.Services.RunIdentitySeedAsync();
await app.Services.RunLibrarySeedAsync();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerExtensions(app);
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();
app.UseHealthChecks("/health");

app.MapControllers();

app.Run();

using ECommerce.Api.Middleware;
using ECommerce.Application.Extensions;
using ECommerce.Infrastructure.Extensions;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHealthChecks();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "E-Commerce Payment Integration API", 
        Version = "v1",
        Description = "A comprehensive e-commerce backend with payment integration for the Balance Management service",
        Contact = new OpenApiContact
        {
            Name = "E-Commerce Team",
            Email = "yasincoskunn35@gmail.com"
        }
    });
});

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddLogging();

var app = builder.Build();
//Health check added
app.MapHealthChecks("/health");
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "E-Commerce API V1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();
app.UseMiddleware<GlobalExceptionHandler>();
app.UseAuthorization();
app.MapControllers();

app.Run();

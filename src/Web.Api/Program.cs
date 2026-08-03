using System.Reflection;
using Application;
using HealthChecks.UI.Client;
using Infrastructure;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Onspay.AspNetCore;
using Onspay.AspNetCore.Extensions;
using Onspay.Infrastructure.Logging.Elasticsearch;
using Onspay.Infrastructure.Logging.Extensions;
using Scalar.AspNetCore;
using Serilog;
using Web.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteToElasticsearch(context));

builder.Services.AddOpenApi();

builder.Services.UseLoggingService(cfg => builder.Configuration.GetSection("Onspay:Logging").Bind(cfg));
builder.Services.AddElasticSearchLogService();

builder.Services
    .AddApplication(builder.Configuration)
    .AddInfrastructure(builder.Configuration)
    .AddPresentation(builder.Configuration);

builder.Services.AddEndpoints(Assembly.GetExecutingAssembly());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.ApplyMigrations();
    //await app.SeedData();
    //Data seed gerekirse bunu kullanabilirsiniz Extensions içindeki SeedData metodunu doldurabilirsiniz.
}

app.UseRequestContextLogging();
app.UseRequestResponseLogging();
app.UseExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.UseHttpsRedirection();
app.MapEndpoints();

app.Run();
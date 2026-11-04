using System.Reflection;
using Application;
using HealthChecks.UI.Client;
using Infrastructure;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Onspay.AspNetCore;
using Onspay.AspNetCore.Extensions;
using Onspay.Infrastructure.Correlation;
using Onspay.Infrastructure.Logging.Elasticsearch;
using Onspay.Infrastructure.Logging.Extensions;
using Scalar.AspNetCore;
using Serilog;
using Web.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Framework/ILogger<T> logs → console. CorrelationIdEnricher adds "correlation.id" to every event, so HTTP
// requests, outbox publishes and inbox handling of the same flow share one id.
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
    .Enrich.With(services.GetRequiredService<CorrelationIdEnricher>())
    .WriteTo.Console(outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] [{correlation.id}] {SourceContext}: {Message:lj}{NewLine}{Exception}"));

builder.Services.AddOpenApi();

// Onspay ILogService channel (business, request/response and error logs) → Elasticsearch.
// Everything is driven by the "Onspay:Logging" section; with Enabled=false nothing is shipped.
builder.Services.UseLoggingService(cfg => builder.Configuration.GetSection("Onspay:Logging").Bind(cfg));
builder.Services.AddElasticSearchLogService();

builder.Services
    .AddPresentation()
    .AddApplication(builder.Configuration)
    .AddInfrastructure(builder.Configuration);

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

// Order matters: correlation and request/response logging must wrap the exception handler.
app.UseRequestContextLogging();
app.UseRequestResponseLogging();
app.UseExceptionHandler();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapEndpoints();

app.Run();

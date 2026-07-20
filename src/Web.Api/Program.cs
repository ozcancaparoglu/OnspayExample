using System.Reflection;
using Application;
using HealthChecks.UI.Client;
using Infrastructure;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Onspay.AspNetCore;
using Onspay.AspNetCore.Extensions;
using Onspay.Infrastructure.Logging;
using Onspay.Infrastructure.Logging.Elasticsearch;
using Scalar.AspNetCore;
using Web.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseOnspayLogging((cfg, ctx) => cfg.WriteToElasticsearch(ctx));builder.Services.AddOpenApi();

builder.Services
    .AddApplication(builder.Configuration)
    .AddInfrastructure(builder.Configuration)
    .AddPresentation();

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
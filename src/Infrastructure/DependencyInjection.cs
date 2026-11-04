using Application.Abstractions.Data;
using Dapper;
using Infrastructure.Database;
using Infrastructure.Database.Handlers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Onspay.Infrastructure.Caching.Redis;
using Onspay.Infrastructure.DomainEvents;
using Onspay.Infrastructure.Inbox;
using Onspay.Infrastructure.Messaging.RabbitMq;
using Onspay.Infrastructure.Outbox;
using Onspay.SharedKernel;

namespace Infrastructure;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddInfrastructure(IConfiguration configuration) =>
            services
                .AddServices()
                .AddDatabase(configuration)
                .AddMessaging(configuration)
                .AddCaching(configuration)
                .AddHealthChecks(configuration);

        private IServiceCollection AddServices()
        {
            services.AddDomainEvents();
            return services;
        }

        private IServiceCollection AddDatabase(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("Database")
                                   ?? throw new InvalidOperationException("Connection string 'Database' is missing.");

            services.AddDbContext<ApplicationDbContext>(options => options
                .UseNpgsql(connectionString, npgsqlOptions =>
                    npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Default))
                .UseSnakeCaseNamingConvention());

            services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
            services.AddScoped<IInboxDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
            services.AddScoped<IOutboxDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

            // Used by the Dapper query handlers and by the Inbox/Outbox processing jobs.
            services.AddSingleton<ISqlConnectionFactory>(_ => new SqlConnectionFactory(connectionString));

            SqlMapper.AddTypeHandler(new NameTypeHandler());
            SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());

            return services;
        }

        private IServiceCollection AddMessaging(IConfiguration configuration)
        {
            // Outbox: Quartz job polls outbox_messages and publishes through IMessagePublisher ("Outbox" section).
            services.AddOutboxProcessing(configuration);

            // Inbox: registers IInboxWriter (used by HasInbox queues) and the job that dispatches
            // inbox_messages rows to IInboxMessageHandler implementations ("Inbox" section).
            services.AddInboxProcessing(configuration);

            // Publisher + one hosted consumer per bus configured under "RabbitMq".
            services.AddRabbitMqMessaging(configuration);

            return services;
        }

        private IServiceCollection AddCaching(IConfiguration configuration)
        {
            services.AddRedisCache(configuration);

            // Decorates every IQueryHandler; only ICachedQuery<T> queries are actually cached.
            // Must run after AddCqrs (AddApplication), which registers the handlers.
            services.AddQueryCaching();

            return services;
        }

        private IServiceCollection AddHealthChecks(IConfiguration configuration)
        {
            services
                .AddHealthChecks()
                .AddNpgSql(configuration.GetConnectionString("Database")!)
                .AddRedis(configuration.GetSection(CacheOptions.SectionName)[nameof(CacheOptions.ConnectionString)]!);

            return services;
        }
    }
}

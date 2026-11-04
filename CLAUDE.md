# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

OnsPayExample is a .NET 10 example/reference API for an order-management domain (create/list/get orders), built as a
showcase of the `Onspay.*` framework packages (v2.0.0): CQRS + Result pattern, domain events, transactional
Outbox → RabbitMQ → Inbox messaging, Redis query caching, correlation-id propagation, and structured logging to
Elasticsearch. Treat it as a template project: the interesting architecture is the plumbing, not the business logic.

## Commands

```bash
cp .env.example .env                           # credentials for compose (git-ignored)
docker compose up -d                           # infra only: postgres 5432, rabbitmq 5672/15672, redis 6379,
                                               #   elasticsearch 9200, kibana 5601
docker compose --profile api up -d --build     # infra + the API container (http://localhost:8080/scalar)
```

On this machine the runtime is podman; `docker-compose` (`/usr/local/bin/docker-compose`) works against it.

Build / run locally against the compose infra:

```bash
dotnet build                              # build whole solution
dotnet run --project src/Web.Api          # http://localhost:5199 (Scalar at /scalar)
```

Local dev credentials live in `src/Web.Api/appsettings.Development.json` and match `.env.example` defaults; the
container gets them from `.env` via environment variables in `compose.yaml`. Override with user-secrets
(`UserSecretsId` is set on Web.Api) rather than editing `appsettings.json`.

On startup in `Development`, `Program.cs` auto-applies pending EF Core migrations (`app.ApplyMigrations()`) and
exposes Scalar OpenAPI UI. Data seeding (`app.SeedData()`) is wired up but commented out and currently a no-op.

EF Core migrations (run from repo root, targeting the `Infrastructure` project which owns `ApplicationDbContext`):

```bash
dotnet ef migrations add <Name> --project src/Infrastructure --startup-project src/Web.Api --output-dir Database/Migrations
dotnet ef database update --project src/Infrastructure --startup-project src/Web.Api
```

There is no test project yet — the `test` solution folder in `OnsPayExample.sln` is an empty placeholder.

**Packages**: `Onspay.*` packages are published to nuget.org (source: `~/Core/Onspay.Packages`, README there is
the API spec; breaking changes in its `docs/MIGRATION-2.0.md`). The repo `nuget.config` clears sources and uses
nuget.org only. Versions are centrally managed in `Directory.Packages.props` — csprojs carry no `Version`; all
`Onspay.*` packages share `$(OnspayVersion)` and must be bumped together. Shared build props (TFM, nullable,
implicit usings) are in `Directory.Build.props`.

## Architecture

Classic layered/Clean Architecture, one project per layer, dependencies point inward:

```
Web.Api  →  Infrastructure  →  Application  →  Domain
Web.Api  →  Application
Web.Api  →  Domain
```

- **Domain** — no dependencies except `Onspay.SharedKernel`. Aggregates (`Order`) and entities (`OrderItem`) derive
  from `AuditableEntity<Guid>` (from the shared kernel) with private constructors and `Create(...)` static
  factories. `OrderItem`s are created only through `Order.AddItem(...)` (EF maps the `_orderItems` backing field). Value objects (`Money`, `Name`, `Description`, `Currency`) are records with private constructors,
  a validating `Create`/`ResolveDescription` factory returning `Result<T>`, and a separate `From(...)` factory
  used only by EF Core value converters for reconstitution from the database (skips validation intentionally —
  don't add validation there). Domain errors are static `Error` instances defined per aggregate in
  `Domain/<Aggregate>/Helpers/<Aggregate>Errors.cs` with a numeric code, message, and `ErrorType`. Domain events
  (e.g. `OrderCreatedDomainEvent`) are raised inside aggregate factory methods via `Raise(...)` and dispatched by
  the infrastructure's `DomainEventDbContextBase` around `SaveChanges`.

- **Application** — CQRS via `Onspay.Cqrs` (`services.AddCqrs(configuration, assembly)` registers all
  `ICommandHandler`/`IQueryHandler`/domain event handlers/FluentValidation validators from the assembly by
  convention). Organized as vertical slices under `Features/<Aggregate>/<UseCase>/`
  (`Create`, `GetAll`, `GetById`, `Events`), each folder holding its command/query record, handler, and (for
  commands) a `FluentValidation` validator — not by technical layer. Handlers return `Result<T>` /
  `Result<IReadOnlyList<T>>`, never throw for expected failures. `Responses/` holds the DTOs returned to clients.

- **CQRS read/write split**: command handlers (writes) go through EF Core via `IApplicationDbContext`
  (`Application/Abstractions/Data`), so domain events fire and validation/conversions run. Query handlers (reads)
  bypass EF Core entirely and use Dapper directly via `ISqlConnectionFactory`, with hand-written SQL against the
  Postgres tables (snake_case columns, e.g. `total_amount_amount`, `product_name`) — EF Core's naming convention
  (`UseSnakeCaseNamingConvention`) governs the actual schema, so query SQL must track configuration changes made
  in `Infrastructure/Database/Configurations/*Configuration.cs`.

- **Infrastructure** — `ApplicationDbContext` extends `DomainEventDbContextBase` (from
  `Onspay.Infrastructure.DomainEvents`) so `SaveChangesAsync` dispatches raised domain events after persisting.
  Entity configurations live in `Database/Configurations/`, one per aggregate, using `HasConversion` to map value
  objects to columns and `OwnsOne` for embedded value objects like `Money` (stored as `<prop>_amount` /
  `<prop>_currency`). Custom Dapper type handlers (`Handlers/`) are registered once in `AddInfrastructure` via
  `SqlMapper.AddTypeHandler`. `AddInfrastructure` uses C# extension-member syntax (`extension(IServiceCollection
  services) { ... }`) to group related registration methods — follow that pattern when adding new registration
  helpers here rather than introducing separate static classes.

- **Web.Api** — minimal API endpoints, one class per endpoint implementing `Onspay.AspNetCore`'s `IEndpoint`
  (`MapEndpoint(IEndpointRouteBuilder)`), auto-discovered and mapped via `AddEndpoints(assembly)` /
  `app.MapEndpoints()`. Endpoints live under `Endpoints/<Aggregate>/<Verb>.cs`, take the CQRS handler as a
  parameter (endpoint-level DI, no MediatR-style single entry point), map the HTTP request DTO to a
  command/query, and convert the `Result<T>` to an HTTP response via `Onspay.AspNetCore`'s
  `ToApiResponse()` / `ToCreatedApiResponse(...)` extensions. Route group tags come from the `Tags` static class —
  add new tag constants there rather than inlining string literals.

- **Messaging (Outbox → RabbitMQ → Inbox)**: `CreateOrderCommandHandler` adds an `OutboxMessage` (an
  `OrderCreatedIntegrationEvent`, JSON) to `IApplicationDbContext.OutboxMessages` in the *same* `SaveChanges` as the
  order. `ProcessOutboxMessagesJob` (Quartz, `Outbox` section) publishes it to the exchange in
  `MessagingTopology`; the `RabbitMq:Onspay:Queues:OrderEvents` queue has `HasInbox: true`, so the consumer writes
  it to `inbox_messages`, and `ProcessInboxMessagesJob` (`Inbox` section) dispatches it to the
  `InboxMessageHandlerBase<T>` whose `MessageType` matches (`CompleteOrderOnOrderCreatedHandler`, which completes the
  order and evicts its cache entry). Inbox handlers are found by `AddCqrs`'s assembly scan and must be idempotent.
  Exchange names in `MessagingTopology` must match `appsettings.json`. Inbox/Outbox options are validated at startup
  (all of `IntervalInSeconds`/`BatchSize`/`MaxRetryCount` > 0). Integration events are a public contract —
  keep them separate from domain events.

- **Caching**: `GetOrderByIdQuery` implements `ICachedQuery<T>`; `AddQueryCaching()` (must come after `AddCqrs`)
  decorates query handlers and caches those in Redis (`Cache` section). Writers evict with
  `ICacheService.RemoveAsync(GetOrderByIdQuery.CacheKeyFor(id))`.

- **Cross-cutting**: Serilog writes framework/`ILogger<T>` logs to console, enriched with `correlation.id` via
  `CorrelationIdEnricher`. The Onspay `ILogService` channel (request/response, errors) goes to Elasticsearch via
  `ElasticSearchLogService`, configured entirely by `Onspay:Logging` (`Enabled`, `RequestResponseEnabled`, `Address`,
  `ExcludedPaths`…). The correlation id (`X-Correlation-Id`) flows HTTP → outbox row → AMQP header → inbox row, so
  one id traces the whole flow. Middleware order in `Program.cs` (`UseRequestContextLogging` →
  `UseRequestResponseLogging` → `UseExceptionHandler` → `MapEndpoints`) is required by `Onspay.AspNetCore`.

## Adding a new feature slice

Follow the existing `Orders` slice as the template: domain entity/value objects in `Domain`, a
`Features/<Aggregate>/<UseCase>` folder in `Application` with command-or-query + handler (+ validator for
commands), an `EntityTypeConfiguration` in `Infrastructure/Database/Configurations` if it's a new aggregate, and
an `IEndpoint` class in `Web.Api/Endpoints/<Aggregate>`. No manual DI registration is needed for handlers,
validators, inbox handlers, or endpoints — all are assembly-scanned. Dapper query handlers should map into flat row
types and assemble the response record afterwards (Dapper can't construct a record whose constructor also takes a
child collection).
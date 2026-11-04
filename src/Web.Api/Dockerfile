# Build context: repository root (see compose.yaml).
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Restore first, using only the files that affect it, so the layer is cached until a dependency changes.
COPY ["nuget.config", "Directory.Build.props", "Directory.Packages.props", "./"]
COPY ["src/Domain/Domain.csproj", "src/Domain/"]
COPY ["src/Application/Application.csproj", "src/Application/"]
COPY ["src/Infrastructure/Infrastructure.csproj", "src/Infrastructure/"]
COPY ["src/Web.Api/Web.Api.csproj", "src/Web.Api/"]
RUN dotnet restore "src/Web.Api/Web.Api.csproj"

COPY src/ src/
RUN dotnet publish "src/Web.Api/Web.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish --no-restore /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Web.Api.dll"]

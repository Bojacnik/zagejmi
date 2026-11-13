# Build context is the repository root (see docker-compose.yaml).
FROM mcr.microsoft.com/dotnet/aspnet:11.0 AS base
USER $APP_UID
# Writable location for ASP.NET Core Data Protection keys (mount a volume here to keep them across restarts).
RUN mkdir -p /home/app/.aspnet/DataProtection-Keys
ENV DataProtection__KeysPath=/home/app/.aspnet/DataProtection-Keys
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:11.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /repo

# Restore first (cached layer): repo-wide build props + every project file.
COPY ["global.json", "Directory.Build.props", "Directory.Build.targets", "Directory.Packages.props", "stylecop.json", "./"]
COPY ["src/Zagejmi.Shared/Zagejmi.Shared.csproj", "src/Zagejmi.Shared/"]
COPY ["src/Zagejmi.Domain/Zagejmi.Domain.csproj", "src/Zagejmi.Domain/"]
COPY ["src/Zagejmi.Application/Zagejmi.Application.csproj", "src/Zagejmi.Application/"]
COPY ["src/Zagejmi.Infrastructure/Zagejmi.Infrastructure.csproj", "src/Zagejmi.Infrastructure/"]
COPY ["src/Zagejmi.Api/Zagejmi.Api.csproj", "src/Zagejmi.Api/"]
RUN dotnet restore "src/Zagejmi.Api/Zagejmi.Api.csproj"

COPY src/ src/
RUN dotnet build "src/Zagejmi.Api/Zagejmi.Api.csproj" -c $BUILD_CONFIGURATION --no-restore

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "src/Zagejmi.Api/Zagejmi.Api.csproj" -c $BUILD_CONFIGURATION --no-restore -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Zagejmi.Api.dll"]

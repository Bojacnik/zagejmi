# Use the .NET SDK 9.0 as the build environment
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# Set build argument for configuration (default: Release)
ARG BUILD_CONFIGURATION=Release

# Set the working directory inside the container
WORKDIR /src

# Copy only the project file to restore dependencies
COPY Zagejmi.Client/Zagejmi.Client.csproj Zagejmi.Client/

# Restore dependencies (ensures layer caching)
WORKDIR /src/Zagejmi.Client
RUN dotnet restore "Zagejmi.Client.csproj"

# Copy the remaining source code
COPY . .

# Build the application
RUN dotnet build "Zagejmi.Client.csproj" -c $BUILD_CONFIGURATION -o /app/build --no-restore

# Publish the application for deployment
RUN dotnet publish "Zagejmi.Client.csproj" -c $BUILD_CONFIGURATION -o /app/publish --no-build --self-contained false

# Use the ASP.NET runtime as the final runtime environment
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime

# Set the working directory inside the runtime container
WORKDIR /app

# Copy the published application from the build container
COPY --from=build /app/publish .

# Expose necessary ports
EXPOSE 80
EXPOSE 443

# Health check to ensure the container is running
HEALTHCHECK --interval=30s --timeout=10s --retries=3 \
  CMD curl --fail http://localhost:80 || exit 1

# Set the entry point to start the application
ENTRYPOINT ["dotnet", "Zagejmi.Client.dll"]

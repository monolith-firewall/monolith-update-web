# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

# Copy csproj and restore dependencies
COPY ["src/MonolithUpdateSite.csproj", "src/"]
RUN dotnet restore "src/MonolithUpdateSite.csproj"

# Copy everything else and build
COPY src/ src/
WORKDIR /source/src
RUN dotnet build "MonolithUpdateSite.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "MonolithUpdateSite.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published app
COPY --from=publish /app/publish .

# Create directories for data persistence
RUN mkdir -p /app/data /app/wwwroot/updates

# Declare volumes for persistence
VOLUME ["/app/data", "/app/wwwroot/updates"]

# Set environment variables
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

# Expose port
EXPOSE 5000

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:5000/Account/Login || exit 1

# Set the entrypoint
ENTRYPOINT ["dotnet", "MonolithUpdateSite.dll"]

# Use the official .NET 9.0 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# Set working directory
WORKDIR /src

# Copy solution file
COPY ContentGenerator.sln ./

# Copy project files
COPY src/ContentGenerator.API/ContentGenerator.API.csproj src/ContentGenerator.API/
COPY src/ContentGenerator.Core/ContentGenerator.Core.csproj src/ContentGenerator.Core/
COPY src/ContentGenerator.Infrastructure/ContentGenerator.Infrastructure.csproj src/ContentGenerator.Infrastructure/
COPY src/ContentGenerator.Tests/ContentGenerator.Tests.csproj src/ContentGenerator.Tests/

# Restore dependencies
RUN dotnet restore

# Copy all source code
COPY . .

# Build the application
RUN dotnet build -c Release --no-restore

# Publish the application
RUN dotnet publish src/ContentGenerator.API/ContentGenerator.API.csproj -c Release -o /app/publish --no-restore

# Use the official .NET 9.0 runtime image for running
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime

# Set working directory
WORKDIR /app

# Install necessary packages for file operations
RUN apt-get update && apt-get install -y \
    curl \
    && rm -rf /var/lib/apt/lists/*

# Create non-root user for security
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Create directories for file uploads and logs
RUN mkdir -p /app/uploads /app/logs && \
    chown -R appuser:appuser /app

# Copy published application
COPY --from=build /app/publish .

# Set ownership of the application directory
RUN chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Expose ports
EXPOSE 80
EXPOSE 443

# Set environment variables
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost/health || exit 1

# Start the application
ENTRYPOINT ["dotnet", "ContentGenerator.API.dll"]

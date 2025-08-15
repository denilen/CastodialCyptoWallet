# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY ["src/API/CryptoWallet.API.csproj", "src/API/"]
COPY ["src/Application/CryptoWallet.Application.csproj", "src/Application/"]
COPY ["src/Domain/CryptoWallet.Domain.csproj", "src/Domain/"]
COPY ["src/Infrastructure/CryptoWallet.Infrastructure.csproj", "src/Infrastructure/"]

# Restore dependencies
RUN dotnet restore "src/API/CryptoWallet.API.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/src/API"
RUN dotnet build "CryptoWallet.API.csproj" -c Release -o /app/build

# Publish the app
FROM build AS publish
RUN dotnet publish "CryptoWallet.API.csproj" -c Release -o /app/publish \
    --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install cultures (same as default .NET image)
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        netcat \
        procps \
    && rm -rf /var/lib/apt/lists/*

# Set the environment to production
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1
ENV DOTNET_NOLOGO=true

# Expose the port the app runs on
EXPOSE 80
EXPOSE 443

# Copy the published app
COPY --from=publish /app/publish .

# Set the entry point
ENTRYPOINT ["dotnet", "CryptoWallet.API.dll"]

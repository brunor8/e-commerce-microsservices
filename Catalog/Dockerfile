# build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/Catalog.API/Catalog.API.csproj", "src/Catalog.API/"]
COPY ["src/Catalog.Application/Catalog.Application.csproj", "src/Catalog.Application/"]
COPY ["src/Catalog.Domain/Catalog.Domain.csproj", "src/Catalog.Domain/"]
COPY ["src/Catalog.Infrastructure/Catalog.Infrastructure.csproj", "src/Catalog.Infrastructure/"]

RUN dotnet restore "src/Catalog.API/Catalog.API.csproj"

COPY . .

WORKDIR /src/src/Catalog.API
RUN dotnet build "Catalog.API.csproj" -c Release -o /app/build

# publish
FROM build AS publish
RUN dotnet publish "Catalog.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

COPY --from=publish /app/publish .

HEALTHCHECK --interval=30s --timeout=5s --start-period=15s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "Catalog.API.dll"]
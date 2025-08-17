
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["ECommercePaymentChallenge.sln", "./"]
COPY ["src/ECommerce.Api/ECommerce.Api.csproj", "src/ECommerce.Api/"]
COPY ["src/ECommerce.Application/ECommerce.Application.csproj", "src/ECommerce.Application/"]
COPY ["src/ECommerce.Domain/ECommerce.Domain.csproj", "src/ECommerce.Domain/"]
COPY ["src/ECommerce.Infrastructure/ECommerce.Infrastructure.csproj", "src/ECommerce.Infrastructure/"]
COPY ["src/ECommerce.Tests/ECommerce.Tests.csproj", "src/ECommerce.Tests/"]

RUN dotnet restore

COPY . .
RUN dotnet build -c Release

RUN dotnet test -c Release --no-build --verbosity normal

RUN dotnet publish "src/ECommerce.Api/ECommerce.Api.csproj" -c Release -o /app/publish --no-build

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

RUN adduser --disabled-password --gecos "" appuser

COPY --from=build /app/publish .
RUN chown -R appuser:appuser /app

USER appuser

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "ECommerce.Api.dll"] 
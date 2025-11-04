FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Films.Start/Films.Start.csproj", "Films.Start/"]
COPY ["Common.DI.WebApi/Common.DI.WebApi.csproj", "Common.DI.WebApi/"]
COPY ["Common.DI/Common.DI.csproj", "Common.DI/"]
COPY ["Common.Application/Common.Application.csproj", "Common.Application/"]
COPY ["Common.Domain/Common.Domain.csproj", "Common.Domain/"]
COPY ["Common.Infrastructure/Common.Infrastructure.csproj", "Common.Infrastructure/"]
COPY ["Films.Application.Services/Films.Application.Services.csproj", "Films.Application.Services/"]
COPY ["Films.Application.Abstractions/Films.Application.Abstractions.csproj", "Films.Application.Abstractions/"]
COPY ["Common.IntegrationEvents/Common.IntegrationEvents.csproj", "Common.IntegrationEvents/"]
COPY ["Films.Domain/Films.Domain.csproj", "Films.Domain/"]
COPY ["Films.Infrastructure.Storage/Films.Infrastructure.Storage.csproj", "Films.Infrastructure.Storage/"]
COPY ["Films.Infrastructure.Bus/Films.Infrastructure.Bus.csproj", "Films.Infrastructure.Bus/"]
COPY ["Films.Infrastructure.Web/Films.Infrastructure.Web.csproj", "Films.Infrastructure.Web/"]
RUN dotnet restore "Films.Start/Films.Start.csproj"
COPY ["Films.Start/.", "Films.Start/"]
COPY ["Common.DI.WebApi/.", "Common.DI.WebApi/"]
COPY ["Common.DI/.", "Common.DI/"]
COPY ["Common.Application/.", "Common.Application/"]
COPY ["Common.Domain/.", "Common.Domain/"]
COPY ["Common.Infrastructure/.", "Common.Infrastructure/"]
COPY ["Films.Application.Services/.", "Films.Application.Services/"]
COPY ["Films.Application.Abstractions/.", "Films.Application.Abstractions/"]
COPY ["Common.IntegrationEvents/.", "Common.IntegrationEvents/"]
COPY ["Films.Domain/.", "Films.Domain/"]
COPY ["Films.Infrastructure.Storage/.", "Films.Infrastructure.Storage/"]
COPY ["Films.Infrastructure.Bus/.", "Films.Infrastructure.Bus/"]
COPY ["Films.Infrastructure.Web/.", "Films.Infrastructure.Web/"]
WORKDIR "/src/Films.Start"
RUN dotnet build "./Films.Start.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Films.Start.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Films.Start.dll"]

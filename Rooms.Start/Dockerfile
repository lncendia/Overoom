FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Rooms.Start/Rooms.Start.csproj", "Rooms.Start/"]
COPY ["Rooms.Application.Services/Rooms.Application.Services.csproj", "Rooms.Application.Services/"]
COPY ["Rooms.Application.Abstractions/Rooms.Application.Abstractions.csproj", "Rooms.Application.Abstractions/"]
COPY ["Overoom.IntegrationEvents/Overoom.IntegrationEvents.csproj", "Overoom.IntegrationEvents/"]
COPY ["Rooms.Domain.Abstractions/Rooms.Domain.Abstractions.csproj", "Rooms.Domain.Abstractions/"]
COPY ["Rooms.Domain/Rooms.Domain.csproj", "Rooms.Domain/"]
COPY ["Rooms.Infrastructure.Storage/Rooms.Infrastructure.Storage.csproj", "Rooms.Infrastructure.Storage/"]
COPY ["Rooms.Infrastructure.Bus/Rooms.Infrastructure.Bus.csproj", "Rooms.Infrastructure.Bus/"]
COPY ["Rooms.Infrastructure.Web/Rooms.Infrastructure.Web.csproj", "Rooms.Infrastructure.Web/"]
RUN dotnet restore "Rooms.Start/Rooms.Start.csproj"
COPY . .
WORKDIR "/src/Rooms.Start"
RUN dotnet build "./Rooms.Start.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Rooms.Start.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Rooms.Start.dll"]

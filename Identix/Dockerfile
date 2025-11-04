FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Identix/Identix.csproj", "Identix/"]
COPY ["Common.DI/Common.DI.csproj", "Common.DI/"]
COPY ["Common.Application/Common.Application.csproj", "Common.Application/"]
COPY ["Common.Domain/Common.Domain.csproj", "Common.Domain/"]
COPY ["Common.Infrastructure/Common.Infrastructure.csproj", "Common.Infrastructure/"]
COPY ["Identix.Infrastructure.Common/Identix.Infrastructure.Common.csproj", "Identix.Infrastructure.Common/"]
COPY ["Identix.Application.Abstractions/Identix.Application.Abstractions.csproj", "Identix.Application.Abstractions/"]
COPY ["Common.IntegrationEvents/Common.IntegrationEvents.csproj", "Common.IntegrationEvents/"]
COPY ["Identix.Application.Services/Identix.Application.Services.csproj", "Identix.Application.Services/"]
COPY ["Identix.Infrastructure.Web/Identix.Infrastructure.Web.csproj", "Identix.Infrastructure.Web/"]
RUN dotnet restore "Identix/Identix.csproj"

COPY ["Identix/.", "Identix/"]
COPY ["Common.DI/.", "Common.DI/"]
COPY ["Common.Application/.", "Common.Application/"]
COPY ["Common.Domain/.", "Common.Domain/"]
COPY ["Common.Infrastructure/.", "Common.Infrastructure/"]
COPY ["Identix.Infrastructure.Common/.", "Identix.Infrastructure.Common/"]
COPY ["Identix.Application.Abstractions/.", "Identix.Application.Abstractions/"]
COPY ["Common.IntegrationEvents/.", "Common.IntegrationEvents/"]
COPY ["Identix.Application.Services/.", "Identix.Application.Services/"]
COPY ["Identix.Infrastructure.Web/.", "Identix.Infrastructure.Web/"]

FROM node:23-alpine AS npmbuild
WORKDIR /src/Identix/client
COPY --from=build /src/Identix/client/package*.json ./
RUN npm install
COPY --from=build /src/Identix/client/. ./
RUN npm run build

FROM build AS publish
WORKDIR "/src/Identix"
COPY --from=npmBuild /src/Identix/wwwroot ./wwwroot
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Identix.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Identix.dll"]

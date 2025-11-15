using System.Text.Json;
using System.Text.Json.Serialization;
using Common.Application.ScopedDictionary;
using Common.DI.Extensions;
using Common.DI.Middlewares;
using Common.DI.WebApi.Extensions;
using Common.Infrastructure.JsonConverters;
using Common.Infrastructure.ScopedDictionary;
using Microsoft.AspNetCore.SignalR;
using Rooms.Application.Abstractions;
using Rooms.Application.Abstractions.RoomEvents;
using Rooms.Application.Services.CommandHandlers;
using Rooms.Infrastructure.Storage.DatabaseInitialization;
using Rooms.Infrastructure.Web.HubFilters;
using Rooms.Infrastructure.Web.Metrics;
using Rooms.Infrastructure.Web.Rooms.Hubs;
using Rooms.Start.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Инициализируем подключение к MongoDB
builder.InitializeMongoDb();

// Регистрируем сервисы логгирования
builder.AddLoggingServices();

// Добавление служб авторизации
builder.AddJwtAuthentication();

// Добавляем сервисы CORS
builder.AddCorsServices();

// Добавляем словарь области
builder.Services.AddScoped<IScopedContext, ScopedContext>();

// Добавляем в приложение сервисы для работы с хранилищами
builder.AddStorageServices();

// Добавляем в приложение сервисы для работы с сообщениями MassTransit
builder.AddMassTransitServices();

// Добавляем в приложение сервисы для работы с медиатором
builder.Services.AddMediatorServices(typeof(CreateRoomCommandHandler));

// Регистрация SignalR
builder.Services.AddSignalR(options =>
{
    options.AddFilter<HubMetricsFilter>();
    options.AddFilter<HubExceptionFilter>();
    options.AddFilter<HubConnectionIdFilter>();
}).AddJsonProtocol(options =>
{
    options.PayloadSerializerOptions.Converters.Add(new TypeNameJsonConverter<RoomBaseEvent>());
    options.PayloadSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.PayloadSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
});

// Настраиваем OpenTelemetry
builder.Services.AddOpenTelemetryServices(Constants.OpenTelemetry.ServiceName, RoomsConnectionMetrics.MeterName);

// Создаем экземпляр приложения ASP.NET Core
var app = builder.Build();

// Создаем область для инициализации баз данных
using (var scope = app.Services.CreateScope())
{
    // Инициализация начальных данных в базу данных
    await DatabaseInitializer.InitAsync(scope.ServiceProvider);
}

// Включение CORS
app.UseCors(CorsServices.CorsPolicy);

// Добавляем в приложение middleware для аутентификации
app.UseAuthentication();

// Добавляем в приложение middleware для авторизации
app.UseAuthorization();

// Добавляем в приложение хаб SignalR
app.MapHub<RoomHub>("/room");

// Эндпоинт для Prometheus
app.MapPrometheusScrapingEndpointWithBasicAuth();

// Запускаем приложение ASP.NET Core
await app.RunAsync();
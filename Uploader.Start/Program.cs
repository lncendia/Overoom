using Uploader.Infrastructure.Web.Queue.Controllers;
using Uploader.Infrastructure.Web.Queue.Validators;
using Uploader.Start.Exceptions;
using Uploader.Start.Extensions;
using Common.DI.Extensions;
using Common.DI.Middlewares;
using Common.DI.WebApi.Extensions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Uploader.Application.Abstractions;

BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

var builder = WebApplication.CreateBuilder(args);

// Инициализируем подключение к MongoDB
builder.InitializeMongoDb();

// Регистрируем сервисы логгирования
builder.AddLoggingServices();

// Добавление служб авторизации
builder.AddJwtAuthentication();

// Добавление сервисов swagger
builder.AddSwaggerServices(typeof(QueueController));

// Добавление сервисов хранения файлов
builder.AddFileStorage();

// Добавляем в приложение сервисы для работы с сообщениями MassTransit
builder.AddMassTransitServices();

// Добавляем сервисы CORS
builder.AddCorsServices();

// Добавляет сервисы загрузки фильмов
builder.AddFilmDownloadServices();

// Добавляет HTTP Client для сервиса хранения файлов
builder.Services.AddHlsServices();

// Добавляем политики авторизации
builder.Services.AddAuthorizationPolicies();

// Добавляем в приложение сервисы для валидации моделей
builder.Services.AddValidationServices(typeof(QueueValidator));

// Добавляет сервисы для использования формата сведений о проблеме
builder.Services.AddProblemDetails();

// Добавляем обработчик исключений
builder.Services.AddExceptionHandler<ExceptionHandler>();

// Регистрация контроллеров с поддержкой сериализации JSON
builder.Services.AddControllers();

// Настраиваем OpenTelemetry
builder.Services.AddOpenTelemetryServices(Constants.OpenTelemetry.ServiceName);

// Создаем экземпляр приложения ASP.NET Core
var app = builder.Build();

// Преобразует необработанные исключения в ответы с подробной информацией о проблеме
app.UseExceptionHandler();

// Включение CORS
app.UseCors(CorsServices.CorsPolicy);

// Добавляем в приложение middleware для аутентификации
app.UseAuthentication();

// Добавляем в приложение middleware для авторизации
app.UseAuthorization();

// Использование Swagger для обслуживания документации по API
app.UseSwagger();

// Добавляем в приложение middleware для отображения документации API в формате Swagger UI
app.UseAuthorizedSwaggerUI();

// Добавляем в приложение маршрутизацию запросов на контроллеры MVC
app.MapControllers();

// Эндпоинт для Prometheus
app.MapPrometheusScrapingEndpointWithBasicAuth();

// Запускаем приложение ASP.NET Core
await app.RunAsync();
using System.Text.Json.Serialization;
using Common.DI.Extensions;
using Common.DI.WebApi.Extensions;
using Films.Application.Abstractions.DTOs.Films;
using Films.Application.Services.QueryHandlers.Films;
using Films.Infrastructure.Storage.DatabaseInitialization;
using Films.Infrastructure.Web.Films.Controllers;
using Films.Infrastructure.Web.Films.Mappers;
using Films.Infrastructure.Web.Films.Validators;
using Films.Start.Exceptions;
using Films.Start.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Инициализируем подключение к MongoDB
builder.InitializeMongoDb();

// Регистрируем сервисы логгирования
builder.AddLoggingServices();

// Добавление служб авторизации
builder.AddJwtAuthentication();

// Добавление сервисов swagger
builder.AddSwaggerServices(typeof(FilmsController), typeof(FilmDto));

// Добавление сервисов хранения файлов
builder.AddFileStorage();

// Добавляем в приложение сервисы для работы с хранилищами
builder.AddStorageServices();

// Добавляем в приложение сервисы для работы с сообщениями MassTransit
builder.AddMassTransitServices();

// Добавляем сервисы CORS
builder.AddCorsServices();

// Добавляет HTTP Client для сервиса хранения файлов.
builder.Services.AddFileStorageHttpClient();

// Добавляем политики авторизации
builder.Services.AddAuthorizationPolicies();

// Добавляем в приложение сервисы для валидации моделей
builder.Services.AddValidationServices(typeof(SearchFilmsValidator));

// Добавляем в приложение сервисы для работы с медиатором
builder.Services.AddMediatorServices(typeof(GetFilmByIdQueryHandler));

// Добавляем в приложение сервисы для работы с AutoMapper
builder.Services.AddMappingServices(typeof(FilmsMapperProfile));

// Добавляет сервисы для использования формата сведений о проблеме
builder.Services.AddProblemDetails();

// Добавляем обработчик исключений
builder.Services.AddExceptionHandler<ExceptionHandler>();

// Регистрация контроллеров с поддержкой сериализации JSON
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
});

// Создаем экземпляр приложения ASP.NET Core
var app = builder.Build();

// Создаем область для инициализации баз данных
using (var scope = app.Services.CreateScope())
{
    // Инициализация начальных данных в базу данных
    await DatabaseInitializer.InitAsync(scope.ServiceProvider);
}

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

app.Use(async (context, next) =>
{
    // имитация задержки запроса
    // await Task.Delay(TimeSpan.FromSeconds(40));
    await next();
});

// Добавляем в приложение маршрутизацию запросов на контроллеры MVC
app.MapControllers();

// Запускаем приложение ASP.NET Core
await app.RunAsync();
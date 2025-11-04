using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Identix.Infrastructure.Common.DatabaseInitialization;
using Identix.Extensions;
using Common.DI.Extensions;
using Common.DI.Middlewares;
using Identix.Application.Abstractions;
using Identix.Application.Services.Commands.Create;

// ╔══╗╔═══╗╔═══╗╔═╗ ╔╗╔════╗╔══╗╔═╗╔═╗
// ╚╣╠╝╚╗╔╗║║╔══╝║║╚╗║║║╔╗╔╗║╚╣╠╝╚╗╚╝╔╝
//  ║║  ║║║║║╚══╗║╔╗╚╝║╚╝║║╚╝ ║║  ╚╗╔╝ 
//  ║║  ║║║║║╔══╝║║╚╗║║  ║║   ║║  ╔╝╚╗ 
// ╔╣╠╗╔╝╚╝║║╚══╗║║ ║║║ ╔╝╚╗ ╔╣╠╗╔╝╔╗╚╗
// ╚══╝╚═══╝╚═══╝╚╝ ╚═╝ ╚══╝ ╚══╝╚═╝╚═╝

// Регистрирует сериализатор для типа Guid с использованием стандартного представления
BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

// Создаем билдер приложения
var builder = WebApplication.CreateBuilder(args);

// Инициализируем подключение к MongoDB
builder.InitializeMongoDb();

// Добавляем хранение ключей DataProtection
builder.AddSecureDataProtection(builder.Environment.ApplicationName);

// Регистрируем сервисы логгирования
builder.AddLoggingServices();

// Добавляем сервисы Hangfire
builder.AddHangfireServices(Constants.Hangfire.Queue);

// Добавление сервисов хранения файлов
builder.AddFileStorage();

// Добавление кэша в Mongo (используется для сессий)
builder.AddMongoCache();

// Добавляет службы электронной почты с использованием конфигурации
builder.AddEmailService();

// Добавляет службы совместного использования ресурсов из разных источников.
builder.AddCorsServices();

// Добавляет службы для контроллеров в указанную коллекцию IServiceCollection.
builder.Services.AddControllersWithViews()

    // Добавляет в приложение локализацию аннотаций данных MVC.
    .AddDataAnnotationsLocalization()

    // компиляцию View при изменениях
    .AddRazorRuntimeCompilation()

    // Добавляет службы локализации представлений MVC в приложение.
    .AddViewLocalization();

// Добавляет HTTP Client для сервиса хранения файлов.
builder.Services.AddFileStorageHttpClient();

// Добавляет службы ASP.NET Identity.
builder.AddAspIdentity();

// Добавляет службы OpenIdDict.
builder.AddOpenId();

// Добавляет службы локализации для поддержки многоязычности в приложении
builder.Services.AddLocalizationServices();

// Добавляет медиатор и регистрирует обработчики
builder.Services.AddMediatorServices(typeof(CreateUserCommandHandler));

// Добавляет службы MassTransit.
builder.AddMassTransitServices();

// Добавляет службы электронной почты с использованием конфигурации
builder.AddEmailTemplates();

// Настраиваем OpenTelemetry
builder.Services.AddOpenTelemetryServices(Constants.OpenTelemetry.ServiceName);

// Создаем объект приложения
await using var app = builder.Build();

// Создаем область для инициализации баз данных
using (var scope = app.Services.CreateScope())
{
    // Инициализация начальных данных в базу данных
    await DatabaseInitializer.InitAsync(scope.ServiceProvider, builder.Configuration);
}

// Добавляет RequestLocalizationMiddleware для автоматической установки сведений о культуре
app.UseRequestLocalization();

// Добавляем хендлер исключений
app.UseExceptionHandler("/Home/Error");

// Настраиваем обработку HTTP ошибок
app.UseStatusCodePagesWithReExecute("/Home/Error", "?code={0}");

// Включает статическое обслуживание файлов для текущего пути запроса
app.UseStaticFiles();

// Добавляет ПО промежуточного слоя EndpointRoutingMiddleware.
app.UseRouting();

// Добавляет ПО промежуточного слоя CORS в конвейер веб-приложений, чтобы разрешить междоменные запросы
app.UseCors(CorsServices.CorsPolicy);

// Добавляет AuthenticationMiddleware в указанный IApplicationBuilder
app.UseAuthentication();

// Добавляет AuthorizationMiddleware в указанный IApplicationBuilder
app.UseAuthorization();

// Добавляет поддержку сессий
app.UseSession();

// Мапим маршруты к контроллерам с использованием маршрута по умолчанию
app.MapDefaultControllerRoute();

// Эндпоинт для Prometheus
app.MapPrometheusScrapingEndpointWithBasicAuth();

// Запускаем приложение
await app.RunAsync();
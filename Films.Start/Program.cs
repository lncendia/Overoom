using Films.Infrastructure.Storage.DatabaseInitialization;
using Films.Start.Extensions;
using Films.Start.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "Configuration"))
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("movieApi.json", optional: false, reloadOnChange: true)
    .AddJsonFile("jwt.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables()
    .AddCommandLine(args);

builder.Services.AddMemoryCache();

//builder.Services.AddHostedService<FilmLoadHostedService>();

builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddCorsServices();

builder.Services.AddMassTransitServices(builder.Configuration);

builder.Services.AddJwtAuthorization(builder.Configuration);

// Регистрация Swagger генератора
builder.Services.AddSwaggerServices();

// Добавление служб Mediator
builder.Services.AddMediatorServices();

// Добавление служб для хранилища
builder.Services.AddPersistenceServices(builder.Configuration, builder.Environment.WebRootPath);

// Регистрация контроллеров с поддержкой сериализации JSON
builder.Services.AddControllers();

// Добавление служб для работы с CORS
builder.Services.AddCorsServices();



// Создание приложения на основе настроек builder
await using var app = builder.Build();

// Создаем область для инициализации баз данных
using (var scope = app.Services.CreateScope())
{
    // Инициализация начальных данных в базу данных
    await DatabaseInitializer.InitAsync(scope.ServiceProvider);
}

// Добавляем мидлварь обработки ошибок
app.UseMiddleware<ExceptionMiddleware>();

// Использование Swagger для обслуживания документации по API
app.UseSwagger();

// Использование SwaggerServices UI для предоставления интерактивной документации по API
app.UseSwaggerUI(c=>
{
    // Настройте Swagger UI для использования OAuth2
    c.OAuthClientId("swagger");
    c.OAuthUsePkce(); // Используйте PKCE (Proof Key for Code Exchange) с авторизационным кодом
});

// Использование политик Cors
app.UseCors("DefaultPolicy");

app.UseStaticFiles();

app.UseAuthentication();

app.UseAuthorization();

// Маппим контроллеры
app.MapControllers();

// Запуск приложения
await app.RunAsync();
using Common.DI.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace Common.DI.WebApi.Extensions;

/// <summary>
/// Статический класс, предоставляющий метод расширения для добавления авторизации по JWT в коллекцию сервисов.
/// </summary>
public static class JwtAuthentication
{
    /// <summary>
    /// Добавляет авторизацию по JWT в коллекцию сервисов.
    /// </summary>
    /// <param name="builder">Построитель веб-приложений и сервисов.</param>
    public static void AddJwtAuthentication(this IHostApplicationBuilder builder)
    {
        // Получаем секцию из конфигурации
        var section = builder.Configuration.GetSection("Authentication");

        // Получение значения "Authorization:Authority" из конфигурации.
        var authority = section.GetRequiredValue<string>("Authority");
        
        // Получение значения "Authorization:Issuer" из конфигурации.
        var issuer = section.GetRequiredValue<string>("Issuer");

        // Получение значения "Authorization:Audience" из конфигурации.
        var audience = section.GetRequiredValue<string>("Audience");

        // Получение значения "Authorization:InsecureConnection" из конфигурации.
        var insecureConnection = section.GetValue<bool>("InsecureConnection");
        
        // Получение путей хабов SignalR из "Authorization:SignalR" конфигурации.
        var signalRPaths = section.GetSection("SignalR").Get<string[]>() ?? [];

        // Регистрация аутентификации с настройками по умолчанию
        builder.Services.AddAuthentication(options =>
            {
                // Установка схемы аутентификации по умолчанию
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;

                // Установка схемы для изменения по умолчанию
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })

            // Конфигурация параметров JwtBearer
            .AddJwtBearer(options =>
            {
                // Если разрешено небезопасное подключение, то доверяем сертификату сервера OIDC в любом случае
                if (insecureConnection)
                {
                    // Если среда является контейнером разработки
                    options.BackchannelHttpHandler = new HttpClientHandler
                    {
                        // Всегда доверять сертификату сервиса аутентификации
                        ServerCertificateCustomValidationCallback =
                            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                    };
                }

                // Адрес IdentityServer
                options.Authority = authority;

                // Идентификатор ресурса API, который вы задали в IdentityServer
                options.Audience = audience;

                // Устанавливаем настройки валидации JWT-токенов
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    // Устанавливаем допустимого издателя токена
                    ValidIssuer = issuer,

                    // Включаем проверку подписи токена
                    ValidateIssuerSigningKey = true,

                    // Включаем проверку издателя токена
                    ValidateIssuer = true,

                    // Включаем проверку аудитории токена
                    ValidateAudience = true,

                    // Включаем проверку срока действия токена
                    ValidateLifetime = true
                };
                
                // Устанавливаем обработчики событий для JWT-токенов
                options.Events = new JwtBearerEvents
                {
                    // Обработчик события получения сообщения
                    OnMessageReceived = context =>
                    {
                        // Получаем токен доступа из строки запроса
                        var accessToken = context.Request.Query["access_token"];

                        // Получаем путь запроса
                        var path = context.HttpContext.Request.Path;

                        // Если токен доступа не пустой и путь запроса начинается с одного из допустимых путей
                        if (!string.IsNullOrEmpty(accessToken) && signalRPaths.Any(p => path.StartsWithSegments(p)))
                        {
                            // Устанавливаем токен из строки запроса
                            context.Token = accessToken;
                        }

                        // Возвращаем завершенную задачу
                        return Task.CompletedTask;
                    }
                };
            });
    }
}
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
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                // Если разрешено небезопасное подключение, то доверяем сертификату сервера OIDC в любом случае
                if (insecureConnection)
                {
                    options.BackchannelHttpHandler = new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                    };
                }
                options.RequireHttpsMetadata = !insecureConnection;
                options.Authority = authority;
                options.Audience = audience;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = issuer,
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && signalRPaths.Any(p => path.StartsWithSegments(p)))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });
    }
}
using Common.DI.Exceptions;
using Common.DI.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace Common.DI.WebApi.Extensions;

/// <summary>
/// Класс для настройки Swagger в приложении.
/// </summary>
public static class SwaggerServices
{
    /// <summary>
    /// Добавляет настройки Swagger в коллекцию сервисов.
    /// </summary>
    /// <param name="builder">Построитель веб-приложений и сервисов.</param>
    /// <param name="types">Типы, из сборок которых будут загружены файлы с документацией.</param>
    public static void AddSwaggerServices(this IHostApplicationBuilder builder, params Type[] types)
    {
        // Получение значения "Authorization:Issuer" из конфигурации.
        var issuer = builder.Configuration.GetRequiredValue<string>("Swagger:Issuer");

        // Получение областей OAuth из конфигурации.
        var scopes = builder.Configuration.GetSection("Swagger:Scopes").Get<Scope[]>();

        // Получение значения "Swagger:Title" из конфигурации.
        var title = builder.Configuration.GetRequiredValue<string>("Swagger:Title");

        // Получение значения "Swagger:Version" из конфигурации.
        var version = builder.Configuration.GetRequiredValue<string>("Swagger:Version");

        // Если области не указаны - вызывается исключение.
        if (scopes == null || scopes.Length == 0) throw new ConfigurationException("Swagger:Scopes");

        builder.Services.AddSwaggerGen(options =>
        {
            // Включение human-friendly описаний для операций, параметров и схем на
            // основе файлов комментариев XML
            foreach (var type in types)
            {
                // Получение имени файла
                var filename = $"{type.Assembly.GetName().Name}.xml";

                // Добавление комментариев в файлы
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, filename));
            }

            options.SwaggerDoc(version, new OpenApiInfo { Title = title, Version = version });

            // Определите схему OAuth2 для Swagger
            options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri($"{issuer}/connect/authorize"),
                        TokenUrl = new Uri($"{issuer}/connect/token"),
                        Scopes = scopes.ToDictionary(k => k.Name, v => v.Description)
                    }
                }
            });

            // Используйте схему OAuth2 для всех операций в Swagger
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "oauth2"
                        },
                        Scheme = "oauth2",
                        Name = "oauth2",
                        In = ParameterLocation.Header
                    },
                    scopes.Select(s => s.Name).ToArray()
                }
            });
        });
    }

    /// <summary>
    /// Настраивает Swagger UI middleware с поддержкой OAuth2/PKCE
    /// </summary>
    /// <param name="app">Экземпляр IApplicationBuilder</param>
    /// <param name="setupAction">Делегат конфигурации</param>
    /// <returns>IApplicationBuilder для цепочки вызовов</returns>
    // ReSharper disable once InconsistentNaming
    public static void UseAuthorizedSwaggerUI(this IApplicationBuilder app,
        Action<SwaggerUIOptions>? setupAction = null)
    {
        var configuration = app.ApplicationServices.GetRequiredService<IConfiguration>();
        var clientId = configuration.GetRequiredValue<string>("Swagger:ClientId");
        var clientName = configuration.GetRequiredValue<string>("Swagger:ClientName");

        app.UseSwaggerUI(c =>
        {
            c.OAuthClientId(clientId);
            c.OAuthAppName(clientName);
            c.OAuthUsePkce();
            setupAction?.Invoke(c);
        });
    }

    /// <summary>
    /// Область OAuth
    /// </summary>
    /// <param name="Name">Имя области</param>
    /// <param name="Description">Описание области</param>
    private record Scope(string Name, string Description);
}
using System;
using System.Security.Cryptography.X509Certificates;
using Common.DI.Extensions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenIddict.Abstractions;
using Identix.Application.Abstractions.Services;
using Identix.Application.Services.Services;
using Identix.Infrastructure.Web.Account.Services;
using Quartz;

namespace Identix.Extensions;

/// <summary>
/// Методы расширения для настройки OpenID Connect и инфраструктуры аутентификации
/// </summary>
public static class OpenId
{
    /// <summary>
    /// Добавляет и настраивает OpenID Connect сервер и связанные сервисы
    /// </summary>
    /// <param name="builder">Построитель веб-приложения</param>
    public static void AddOpenId(this IHostApplicationBuilder builder)
    {
        // Получаем путь к сертификату из конфигурации
        var certificatePath = builder.Configuration.GetRequiredValue<string>("Identity:Certificate:Path");
        var certificatePassword = builder.Configuration.GetRequiredValue<string>("Identity:Certificate:Password");

        // Извлекаем имя базы данных из конфигурации
        var openIdDatabaseName = builder.Configuration.GetRequiredValue<string>("MongoDB:OpenIdDB");

        // Загружаем сертификат для подписи и шифрования токенов
        var certificate = X509CertificateLoader.LoadPkcs12FromFile(
            certificatePath,
            certificatePassword,
            X509KeyStorageFlags.MachineKeySet |
            X509KeyStorageFlags.Exportable |
            X509KeyStorageFlags.PersistKeySet
        );
        
        // Регистрируем фабрику для создания claims identity в DI контейнере
        builder.Services.AddScoped<IOpenIdClaimsIdentityFactory, OpenIdClaimsIdentityFactory>();

        // Настраиваем управление сессиями с использованием MongoDB в качестве хранилища
        // Сессии будут сохраняться между перезапусками приложения и доступны во всех экземплярах
        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });

        // Настраиваем Quartz для выполнения фоновых задач и периодических работ
        builder.Services.AddQuartz(options =>
        {
            options.UseSimpleTypeLoader();
            options.UseInMemoryStore();
        });

        // Регистрируем хостинг-сервис для Quartz, который управляет выполнением заданий
        builder.Services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

        // Конфигурируем OpenIddict сервер для обработки OAuth 2.0 и OpenID Connect запросов
        builder.Services.AddOpenIddict()

            // Настраиваем ядро OpenIddict с использованием MongoDB как хранилища
            .AddCore(options =>
            {
                // Используем MongoDB для хранения всех сущностей OpenIddict
                options.UseMongoDb().UseDatabase(MongoDbProvider.Client.GetDatabase(openIdDatabaseName));

                // Интегрируем Quartz для выполнения фоновых задач OpenIddict
                options.UseQuartz();
            })

            // Настраиваем клиентскую часть OpenIddict для обработки входящих запросов
            .AddClient(options =>
            {
                // Регистрируем внешние OAuth-провайдеры
                options.AddExternalProviders(builder);
            })

            // Настраиваем серверную часть
            .AddServer(options =>
            {
                // Устанавливаем endpoints OIDC сервера
                options
                    .SetAuthorizationEndpointUris("/connect/authorize")
                    .SetTokenEndpointUris("/connect/token")
                    .SetUserInfoEndpointUris("/connect/userinfo")
                    .SetEndSessionEndpointUris("connect/logout")
                    .SetRevocationEndpointUris("/connect/revoke");

                // Разрешаем стандартные OAuth2/OIDC потоки
                options
                    .AllowPasswordFlow()
                    .AllowClientCredentialsFlow()
                    .AllowAuthorizationCodeFlow()
                    .AllowRefreshTokenFlow();

                // Настраиваем безопасность токенов
                options
                    .AddSigningCertificate(certificate)
                    .AddEncryptionCertificate(certificate)
                    .DisableAccessTokenEncryption();

                // Интеграция с ASP.NET Core
                options.UseAspNetCore()
                    .EnableAuthorizationEndpointPassthrough()
                    .EnableTokenEndpointPassthrough()
                    .EnableEndSessionEndpointPassthrough()
                    .EnableUserInfoEndpointPassthrough()
                    .EnableStatusCodePagesIntegration();
            })
            // Добавляем валидацию токенов для API защиты
            .AddValidation(options =>
            {
                options.UseLocalServer();
                options.UseAspNetCore();
            });

        // Настройка аутентификационных cookie для Identity
        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.ExpireTimeSpan = TimeSpan.FromDays(30);
            options.Cookie.IsEssential = true;

            // Убираем ограничение SameSite, чтобы куки работали внутри iframe (например, для authorize endpoint)
            options.Cookie.SameSite = SameSiteMode.None;
        });

        // Настройка внешних куки (используются при логине через внешних провайдеров)
        builder.Services.ConfigureExternalCookie(options => options.Cookie.IsEssential = true);

        // Куки для запоминания устройства при двухфакторной аутентификации (Remember this machine)
        builder.Services.Configure<CookieAuthenticationOptions>(IdentityConstants.TwoFactorRememberMeScheme, options =>
        {
            options.ExpireTimeSpan = TimeSpan.FromDays(30);
            options.Cookie.IsEssential = true;
        });

        // Куки, используемые для хранения идентификатора пользователя при двухфакторной аутентификации
        builder.Services.Configure<CookieAuthenticationOptions>(IdentityConstants.TwoFactorUserIdScheme,
            options => options.Cookie.IsEssential = true);

        // Настройка валидации security stamp — защита от параллельных/украденных сессий.
        builder.Services.Configure<SecurityStampValidatorOptions>(options =>
        {
            options.ValidationInterval = TimeSpan.FromMinutes(15);
            options.OnRefreshingPrincipal = SecurityStampValidatorCallback.UpdatePrincipal;
        });

        // Настройки Identity: указываем, какие claim использовать для UserId, UserName и Role.
        builder.Services.Configure<IdentityOptions>(options =>
        {
            options.ClaimsIdentity.UserIdClaimType = OpenIddictConstants.Claims.Subject;
            options.ClaimsIdentity.UserNameClaimType = OpenIddictConstants.Claims.Name;
            options.ClaimsIdentity.RoleClaimType = OpenIddictConstants.Claims.Role;
            options.ClaimsIdentity.EmailClaimType = OpenIddictConstants.Claims.Email;
        });
    }
}
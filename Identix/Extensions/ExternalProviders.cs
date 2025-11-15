using System.Security.Cryptography.X509Certificates;
using Common.DI.Extensions;
using Identix.Infrastructure.Web.External.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Identix.Extensions;

/// <summary>
/// Методы расширения для настройки внешних провайдеров аутентификации через OpenIddict
/// </summary>
public static class ExternalProviders
{
    /// <summary>
    /// Добавляет и настраивает внешние провайдеры аутентификации
    /// </summary>
    /// <param name="options">Builder для настройки OpenIddict клиента</param>
    /// <param name="builder">Builder приложения для доступа к конфигурации и сервисам</param>
    /// <param name="certificate">Сертификат шифрования и подписи токенов</param>
    public static void AddExternalProviders(this OpenIddictClientBuilder options, IHostApplicationBuilder builder, X509Certificate2 certificate)
    {
        // Получение clientId и секретов для всех провайдеров из конфигурации
        var githubClientId = builder.Configuration.GetRequiredValue<string>("OAuth:GitHub:Client");
        var githubSecret = builder.Configuration.GetRequiredValue<string>("OAuth:GitHub:Secret");

        var googleClientId = builder.Configuration.GetRequiredValue<string>("OAuth:Google:Client");
        var googleSecret = builder.Configuration.GetRequiredValue<string>("OAuth:Google:Secret");

        var yandexClientId = builder.Configuration.GetRequiredValue<string>("OAuth:Yandex:Client");
        var yandexSecret = builder.Configuration.GetRequiredValue<string>("OAuth:Yandex:Secret");

        var microsoftClientId = builder.Configuration.GetRequiredValue<string>("OAuth:Microsoft:Client");
        var microsoftSecret = builder.Configuration.GetRequiredValue<string>("OAuth:Microsoft:Secret");

        var vkClientId = builder.Configuration.GetRequiredValue<string>("OAuth:VkId:Client");
        var vkSecret = builder.Configuration.GetRequiredValue<string>("OAuth:VkId:Secret");

        var xClientId = builder.Configuration.GetRequiredValue<string>("OAuth:X:Client");
        var xSecret = builder.Configuration.GetRequiredValue<string>("OAuth:X:Secret");
        
        var discordClientId = builder.Configuration.GetRequiredValue<string>("OAuth:Discord:Client");
        var discordSecret = builder.Configuration.GetRequiredValue<string>("OAuth:Discord:Secret");

        // Разрешаем flow авторизации через authorization code
        options.AllowAuthorizationCodeFlow();

        // Регистрируем сертификаты для шифрования и подписи токенов (для development)
        options.AddEncryptionCertificate(certificate)
            .AddSigningCertificate(certificate);

        // Настраиваем интеграцию с ASP.NET Core
        options.UseAspNetCore()
            .EnableRedirectionEndpointPassthrough();

        // Настраиваем HTTP-клиент и устанавливаем информацию о продукте
        options.UseSystemNetHttp()
            .SetProductInformation(typeof(Program).Assembly);

        // Регистрируем и настраиваем веб-провайдеры
        options.UseWebProviders()
            // Настройка GitHub провайдера
            .AddGitHub(opts =>
            {
                opts.SetClientId(githubClientId)
                    .SetClientSecret(githubSecret)
                    .SetRedirectUri("signin-github")
                    .AddScopes("user:email");
            })
            // Настройка Google провайдера
            .AddGoogle(opts =>
            {
                opts.SetClientId(googleClientId)
                    .SetClientSecret(googleSecret)
                    .SetRedirectUri("signin-google")
                    .AddScopes("email", "profile");
            })
            // Настройка Yandex провайдера
            .AddYandex(opts =>
            {
                opts.SetClientId(yandexClientId)
                    .SetClientSecret(yandexSecret)
                    .SetRedirectUri("signin-yandex")
                    .AddScopes("login:email", "login:info", "login:avatar");
            })
            // Настройка Microsoft провайдера
            .AddMicrosoft(opts =>
            {
                opts.SetClientId(microsoftClientId)
                    .SetClientSecret(microsoftSecret)
                    .SetRedirectUri("signin-microsoft")
                    .AddScopes("email", "profile", "User.Read");
            })
            // Настройка VK ID провайдера
            .AddVkId(opts =>
            {
                opts.SetClientId(vkClientId)
                    .SetClientSecret(vkSecret)
                    .SetRedirectUri("signin-vkid")
                    .AddScopes("email");
            })
            .AddTwitter(opts =>
            {
                opts.SetClientId(xClientId)
                    .SetClientSecret(xSecret)
                    .SetRedirectUri("signin-twitter")
                    .AddScopes("users.email")
                    .AddUserFields("confirmed_email", "profile_image_url");
            })
            .AddDiscord(opts =>
            {
                opts.SetClientId(discordClientId)
                    .SetClientSecret(discordSecret)
                    .SetRedirectUri("signin-discord")
                    .AddScopes("email");
            });
        
        // Регистрируем мапперы claims для каждого провайдера
        builder.Services.AddTransient<IExternalClaimsMapper, GitHubClaimsMapper>();
        builder.Services.AddTransient<IExternalClaimsMapper, GoogleClaimsMapper>();
        builder.Services.AddTransient<IExternalClaimsMapper, YandexClaimsMapper>();
        builder.Services.AddTransient<IExternalClaimsMapper, MicrosoftClaimsMapper>();
        builder.Services.AddTransient<IExternalClaimsMapper, VkIdClaimsMapper>();
        builder.Services.AddTransient<IExternalClaimsMapper, TwitterClaimsMapper>();
        builder.Services.AddTransient<IExternalClaimsMapper, DiscordClaimsMapper>();
    }
}
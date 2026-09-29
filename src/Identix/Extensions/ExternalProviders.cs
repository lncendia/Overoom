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
  public static void AddExternalProviders(this OpenIddictClientBuilder options, IHostApplicationBuilder builder,
    X509Certificate2 certificate)
  {
    string githubClientId = builder.Configuration.GetRequiredValue<string>("OAuth:GitHub:Client");
    string githubSecret = builder.Configuration.GetRequiredValue<string>("OAuth:GitHub:Secret");

    string googleClientId = builder.Configuration.GetRequiredValue<string>("OAuth:Google:Client");
    string googleSecret = builder.Configuration.GetRequiredValue<string>("OAuth:Google:Secret");

    string yandexClientId = builder.Configuration.GetRequiredValue<string>("OAuth:Yandex:Client");
    string yandexSecret = builder.Configuration.GetRequiredValue<string>("OAuth:Yandex:Secret");

    string microsoftClientId = builder.Configuration.GetRequiredValue<string>("OAuth:Microsoft:Client");
    string microsoftSecret = builder.Configuration.GetRequiredValue<string>("OAuth:Microsoft:Secret");

    string vkClientId = builder.Configuration.GetRequiredValue<string>("OAuth:VkId:Client");
    string vkSecret = builder.Configuration.GetRequiredValue<string>("OAuth:VkId:Secret");

    string xClientId = builder.Configuration.GetRequiredValue<string>("OAuth:X:Client");
    string xSecret = builder.Configuration.GetRequiredValue<string>("OAuth:X:Secret");

    string discordClientId = builder.Configuration.GetRequiredValue<string>("OAuth:Discord:Client");
    string discordSecret = builder.Configuration.GetRequiredValue<string>("OAuth:Discord:Secret");
    options.AllowAuthorizationCodeFlow();

    options.AddEncryptionCertificate(certificate)
      .AddSigningCertificate(certificate);

    options.UseAspNetCore()
      .EnableRedirectionEndpointPassthrough();

    options.UseSystemNetHttp()
      .SetProductInformation(typeof(Program).Assembly);

    options.UseWebProviders()
      .AddGitHub(opts =>
      {
        opts.SetClientId(githubClientId)
          .SetClientSecret(githubSecret)
          .SetRedirectUri("signin-github")
          .AddScopes("user:email");
      })
      .AddGoogle(opts =>
      {
        opts.SetClientId(googleClientId)
          .SetClientSecret(googleSecret)
          .SetRedirectUri("signin-google")
          .AddScopes("email", "profile");
      })
      .AddYandex(opts =>
      {
        opts.SetClientId(yandexClientId)
          .SetClientSecret(yandexSecret)
          .SetRedirectUri("signin-yandex")
          .AddScopes("login:email", "login:info", "login:avatar");
      })
      .AddMicrosoft(opts =>
      {
        opts.SetClientId(microsoftClientId)
          .SetClientSecret(microsoftSecret)
          .SetRedirectUri("signin-microsoft")
          .AddScopes("email", "profile", "User.Read");
      })
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

    builder.Services.AddTransient<IExternalClaimsMapper, GitHubClaimsMapper>();
    builder.Services.AddTransient<IExternalClaimsMapper, GoogleClaimsMapper>();
    builder.Services.AddTransient<IExternalClaimsMapper, YandexClaimsMapper>();
    builder.Services.AddTransient<IExternalClaimsMapper, MicrosoftClaimsMapper>();
    builder.Services.AddTransient<IExternalClaimsMapper, VkIdClaimsMapper>();
    builder.Services.AddTransient<IExternalClaimsMapper, TwitterClaimsMapper>();
    builder.Services.AddTransient<IExternalClaimsMapper, DiscordClaimsMapper>();
  }
}
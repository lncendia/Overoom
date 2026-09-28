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
using Microsoft.Extensions.Configuration;
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
    string certificatePath = builder.Configuration.GetRequiredValue<string>("Identity:Certificate:Path");
    string certificatePassword = builder.Configuration.GetRequiredValue<string>("Identity:Certificate:Password");
    string openIdDatabaseName = builder.Configuration.GetRequiredValue<string>("MongoDB:OpenIdDB");
    bool allowInsecureConnection = builder.Configuration.GetValue<bool>("Identity:InsecureConnection");

    X509Certificate2 certificate = X509CertificateLoader.LoadPkcs12FromFile(
      certificatePath,
      certificatePassword,
      X509KeyStorageFlags.MachineKeySet |
      X509KeyStorageFlags.Exportable |
      X509KeyStorageFlags.PersistKeySet
    );

    builder.Services.AddScoped<IOpenIdClaimsIdentityFactory, OpenIdClaimsIdentityFactory>();

    builder.Services.AddSession(options =>
    {
      options.IdleTimeout = TimeSpan.FromMinutes(30);
      options.Cookie.HttpOnly = true;
      options.Cookie.IsEssential = true;
    });

    builder.Services.AddQuartz(options =>
    {
      options.UseSimpleTypeLoader();
      options.UseInMemoryStore();
    });

    builder.Services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

    builder.Services.AddOpenIddict()

      .AddCore(options =>
      {
        options.UseMongoDb().UseDatabase(MongoDbProvider.Client.GetDatabase(openIdDatabaseName));
        options.UseQuartz();
      })

      .AddClient(options =>
      {
        options.AddExternalProviders(builder, certificate);
      })

      .AddServer(options =>
      {
        options
          .SetAuthorizationEndpointUris("/connect/authorize")
          .SetTokenEndpointUris("/connect/token")
          .SetUserInfoEndpointUris("/connect/userinfo")
          .SetEndSessionEndpointUris("connect/logout")
          .SetRevocationEndpointUris("/connect/revoke");

        options
          .AllowPasswordFlow()
          .AllowClientCredentialsFlow()
          .AllowAuthorizationCodeFlow()
          .AllowRefreshTokenFlow();

        options
          .AddSigningCertificate(certificate)
          .AddEncryptionCertificate(certificate)
          .DisableAccessTokenEncryption();

        OpenIddictServerAspNetCoreBuilder aspBuilder = options.UseAspNetCore()
          .DisableTransportSecurityRequirement()
          .EnableAuthorizationEndpointPassthrough()
          .EnableTokenEndpointPassthrough()
          .EnableEndSessionEndpointPassthrough()
          .EnableUserInfoEndpointPassthrough()
          .EnableStatusCodePagesIntegration()
          .DisableTransportSecurityRequirement();

        if (allowInsecureConnection)
          aspBuilder.DisableTransportSecurityRequirement();
      })
      .AddValidation(options =>
      {
        options.UseLocalServer();
        options.UseAspNetCore();
      });

    builder.Services.ConfigureApplicationCookie(options =>
    {
      options.ExpireTimeSpan = TimeSpan.FromDays(30);
      options.Cookie.IsEssential = true;
      options.Cookie.SameSite = SameSiteMode.None;
    });

    builder.Services.ConfigureExternalCookie(options => options.Cookie.IsEssential = true);

    builder.Services.Configure<CookieAuthenticationOptions>(IdentityConstants.TwoFactorRememberMeScheme, options =>
    {
      options.ExpireTimeSpan = TimeSpan.FromDays(30);
      options.Cookie.IsEssential = true;
    });

    builder.Services.Configure<CookieAuthenticationOptions>(IdentityConstants.TwoFactorUserIdScheme,
      options => options.Cookie.IsEssential = true);

    builder.Services.Configure<SecurityStampValidatorOptions>(options =>
    {
      options.ValidationInterval = TimeSpan.FromMinutes(15);
      options.OnRefreshingPrincipal = SecurityStampValidatorCallback.UpdatePrincipal;
    });

    builder.Services.Configure<IdentityOptions>(options =>
    {
      options.ClaimsIdentity.UserIdClaimType = OpenIddictConstants.Claims.Subject;
      options.ClaimsIdentity.UserNameClaimType = OpenIddictConstants.Claims.Name;
      options.ClaimsIdentity.RoleClaimType = OpenIddictConstants.Claims.Role;
      options.ClaimsIdentity.EmailClaimType = OpenIddictConstants.Claims.Email;
    });
  }
}

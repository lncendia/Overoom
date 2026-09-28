using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Identix.Application.Abstractions.Extensions;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;
using Identix.Infrastructure.Common.DatabaseInitialization.Converters;
using Identix.Infrastructure.Common.DatabaseInitialization.Models;
using Microsoft.Extensions.Logging;

namespace Identix.Infrastructure.Common.DatabaseInitialization;

/// <summary>
/// Класс для настройки начальных данных OpenIddict (приложения и области видимости).
/// </summary>
internal static class OpenIdConfiguration
{
  /// <summary>
  /// Настройки десериализации JSON.
  /// </summary>
  private static readonly JsonSerializerOptions _options = new()
  {
    Converters = { new CultureInfoDictionaryConverter() }
  };

  /// <summary>
  /// Настраивает начальные данные OpenIddict в базе данных.
  /// </summary>
  /// <param name="provider">Провайдер служб для получения менеджеров OpenIddict и сервиса логирования.</param>
  /// <returns>Задача, представляющая асинхронную операцию настройки.</returns>
  public static async Task ConfigureAsync(IServiceProvider provider)
  {
    ILoggerFactory loggerFactory = provider.GetRequiredService<ILoggerFactory>();
    ILogger logger = loggerFactory.CreateLogger("OpenIdConfiguration");

    if (!File.Exists("openid.json"))
    {
      logger.LogWarning(
        "OpenID configuration file 'openid.json' was not found. Falling back to default configuration");
      return;
    }

    string json = await File.ReadAllTextAsync("openid.json");
    Models.OpenIdConfiguration? config = JsonSerializer.Deserialize<Models.OpenIdConfiguration>(json, _options);

    if (config is null)
    {
      logger.LogWarning(
        "Failed to deserialize OpenID configuration from 'openid.json'. File is empty or contains malformed JSON. Using default configuration");
      return;
    }

    IOpenIddictApplicationManager applicationManager = provider.GetRequiredService<IOpenIddictApplicationManager>();
    IOpenIddictScopeManager scopeManager = provider.GetRequiredService<IOpenIddictScopeManager>();
    await SeedApplicationsAsync(applicationManager, config.Applications, logger);
    await SeedScopesAsync(scopeManager, config.Scopes, logger);
  }

  /// <summary>
  /// Создает клиентские приложения, если они отсутствуют в базе данных.
  /// </summary>
  /// <param name="manager">Менеджер приложений OpenIddict.</param>
  /// <param name="applications">Массив дескрипторов приложений для создания.</param>
  /// <param name="logger">Логгер для записи информации о процессе создания.</param>
  private static async Task SeedApplicationsAsync(IOpenIddictApplicationManager manager,
    OpenIdApplication[] applications, ILogger logger)
  {
    foreach (OpenIdApplication application in applications)
      await CreateApplicationAsync(manager, application, logger);
  }

  /// <summary>
  /// Создает клиентское приложение на основе дескриптора.
  /// </summary>
  /// <param name="manager">Менеджер приложений OpenIddict.</param>
  /// <param name="application">Дескриптор приложения для создания.</param>
  /// <param name="logger">Логгер для записи информации о процессе создания.</param>
  private static async Task CreateApplicationAsync(IOpenIddictApplicationManager manager,
    OpenIdApplication application, ILogger logger)
  {
    if (await manager.FindByClientIdAsync(application.ClientId) is not null)
    {
      logger.LogInformation("OpenID client application '{ClientId}' already exists", application.ClientId);
      return;
    }

    var descriptor = new OpenIddictApplicationDescriptor
    {
      ApplicationType = application.ApplicationType,
      ClientId = application.ClientId,
      ClientSecret = application.ClientSecret,
      ClientType = application.ClientType,
      ConsentType = application.ConsentType,
      DisplayName = application.DisplayName,
    };

    foreach (KeyValuePair<CultureInfo, string> displayName in application.DisplayNames)
      descriptor.DisplayNames.Add(displayName.Key, displayName.Value);

    foreach (string permission in application.Permissions)
      descriptor.Permissions.Add(permission);

    foreach (Uri redirectUri in application.PostLogoutRedirectUris)
      descriptor.PostLogoutRedirectUris.Add(redirectUri);

    foreach (Uri redirectUri in application.RedirectUris)
      descriptor.RedirectUris.Add(redirectUri);

    foreach (string requirement in application.Requirements)
      descriptor.Requirements.Add(requirement);

    foreach (KeyValuePair<string, string> setting in application.Settings)
      descriptor.Settings.Add(setting.Key, setting.Value);

    if (application.LogoKey != null)
      descriptor.SetLogoKey(application.LogoKey);

    await manager.CreateAsync(descriptor);
    logger.LogInformation("OpenID client application '{ClientId}' created successfully", application.ClientId);
  }

  /// <summary>
  /// Создает области видимости (scopes) для приложения.
  /// </summary>
  /// <param name="manager">Менеджер областей видимости OpenIddict.</param>
  /// <param name="scopes">Массив дескрипторов областей видимости для создания.</param>
  /// <param name="logger">Логгер для записи информации о процессе создания.</param>
  private static async Task SeedScopesAsync(IOpenIddictScopeManager manager, OpenIdScope[] scopes, ILogger logger)
  {
    foreach (OpenIdScope scope in scopes)
      await CreateScopeAsync(manager, scope, logger);
  }

  /// <summary>
  /// Создает область видимости на основе определения.
  /// </summary>
  /// <param name="manager">Менеджер областей видимости OpenIddict.</param>
  /// <param name="scope">Определение области видимости для создания.</param>
  /// <param name="logger">Логгер для записи информации о процессе создания.</param>
  private static async Task CreateScopeAsync(IOpenIddictScopeManager manager, OpenIdScope scope, ILogger logger)
  {
    if (await manager.FindByNameAsync(scope.Name) is not null)
    {
      logger.LogInformation("OpenID scope '{Scope}' already exists", scope.Name);
      return;
    }

    var descriptor = new OpenIddictScopeDescriptor
    {
      Description = scope.Description,
      DisplayName = scope.DisplayName,
      Name = scope.Name,
    };

    foreach (KeyValuePair<CultureInfo, string> description in scope.Descriptions)
      descriptor.Descriptions.Add(description.Key, description.Value);

    foreach (KeyValuePair<CultureInfo, string> displayName in scope.DisplayNames)
      descriptor.DisplayNames.Add(displayName.Key, displayName.Value);

    foreach (string resource in scope.Resources)
      descriptor.Resources.Add(resource);

    await manager.CreateAsync(descriptor);
    logger.LogInformation("OpenID scope '{Scope}' created successfully", scope.Name);
  }
}

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Identix.Infrastructure.Common.DatabaseInitialization;

/// <summary>
/// Класс для инициализации начальных данных в базу данных сервиса аутентификации.
/// Содержит методы для настройки индексов и конфигурации MongoDB.
/// </summary>
public static class DatabaseInitializer
{
  /// <summary>
  /// Инициализирует начальные данные в базу данных.
  /// Выполняет настройку индексов и конфигурации для Identity и OpenId модулей.
  /// </summary>
  /// <param name="serviceProvider">Провайдер сервисов для создания области видимости.</param>
  /// <param name="configuration">Конфигурация приложения</param>
  /// <returns>Задача, представляющая асинхронную операцию инициализации.</returns>
  public static async Task InitAsync(IServiceProvider serviceProvider, IConfiguration configuration)
  {
    await IdentityConfiguration.ConfigureAsync(serviceProvider, configuration);
    await OpenIdMongoIndexCreator.ConfigureAsync(serviceProvider);
    await OpenIdConfiguration.ConfigureAsync(serviceProvider);
  }
}

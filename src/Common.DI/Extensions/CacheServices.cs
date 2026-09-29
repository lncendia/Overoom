using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Common.DI.Extensions;

/// <summary>
/// Методы расширения для настройки сервисов кэширования в MongoDB
/// </summary>
public static class CacheServices
{
  /// <summary>
  /// Добавляет и настраивает распределенный кэш в MongoDB для хранения сессий и временных данных
  /// </summary>
  /// <param name="builder">Построитель приложения для доступа к конфигурации и сервисам</param>
  public static void AddMongoCache(this IHostApplicationBuilder builder)
  {
    string cacheDatabaseName = builder.Configuration.GetRequiredValue<string>("MongoDB:CacheDB");

    builder.Services.AddMongoCache(o =>
    {
      o.MongoClient = MongoDbProvider.Client;
      o.DatabaseName = cacheDatabaseName;
    });
  }
}
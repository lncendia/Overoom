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
        // Получаем имя базы данных для кэша из конфигурации
        var cacheDatabaseName = builder.Configuration.GetRequiredValue<string>("MongoDB:CacheDB");

        // Регистрируем MongoDB в качестве провайдера распределенного кэша
        builder.Services.AddMongoCache(o =>
        {
            // Используем глобальный клиент MongoDB для подключения к базе
            o.MongoClient = MongoDbProvider.Client;
            
            // Указываем конкретную базу данных для хранения кэша
            o.DatabaseName = cacheDatabaseName;
        });
    }
}
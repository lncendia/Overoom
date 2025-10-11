using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;

namespace Common.DI.Extensions;

public static class MongoDbProvider
{
    /// <summary>
    /// Глобальный экземпляр <see cref="MongoClient"/> для работы с MongoDB.
    /// Инициализируется один раз при старте приложения.
    /// </summary>
    private static MongoClient? _client;

    /// <summary>
    /// Получает экземпляр <see cref="MongoClient"/>.
    /// Бросает <see cref="InvalidOperationException"/>, если клиент не был инициализирован.
    /// </summary>
    /// <exception cref="InvalidOperationException">Клиент не инициализирован.</exception>
    public static MongoClient Client => _client ?? throw new InvalidOperationException(
        "MongoDbProvider is not initialized. Call MongoDbProvider.InitializeMongoDb(...) at application startup.");

    /// <summary>
    /// Инициализирует глобальный <see cref="MongoClient"/> на основе строки подключения из конфигурации.
    /// Должен вызываться один раз при старте приложения.
    /// </summary>
    /// <param name="builder">Экземпляр <see cref="WebApplicationBuilder"/> для доступа к конфигурации приложения.</param>
    /// <exception cref="InvalidOperationException">Клиент уже инициализирован или строка подключения отсутствует.</exception>
    public static void InitializeMongoDb(this IHostApplicationBuilder builder)
    {
        if (_client != null)
            throw new InvalidOperationException("MongoDbProvider is already initialized.");

        // Получаем строку подключения из конфигурации
        var connectionString = builder.Configuration.GetRequiredValue<string>("MongoDB:ConnectionString");

        // Создаем и сохраняем экземпляр MongoClient
        _client = new MongoClient(connectionString);
    
        // Регистрируем клиент как singleton в DI-контейнере
        builder.Services.AddSingleton<IMongoClient>(_client);
    }
}
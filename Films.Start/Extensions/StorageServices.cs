using Common.DI.Extensions;
using Common.Infrastructure.Repositories;
using Common.Infrastructure.Repositories.SessionHandlers;
using Films.Domain.Repositories;
using Films.Infrastructure.Storage;
using Films.Infrastructure.Storage.Context;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace Films.Start.Extensions;

/// <summary>
/// Статический класс сервисов хранилища.
/// </summary>
public static class StorageServices
{
    /// <summary>
    /// Расширяющий метод для регистрации сервисов хранилища в коллекции служб.
    /// Метод настраивает зависимости для работы с базами данных, файловым хранилищем и другими компонентами системы.
    /// </summary>
    /// <param name="builder">Построитель веб-приложения.</param>
    public static void AddStorageServices(this IHostApplicationBuilder builder)
    {
        // Получаем секцию конфигурации, содержащую параметры подключения к базе данных.
        var database = builder.Configuration.GetSection("MongoDB");

        // Извлекаем имя базы данных для приложения из конфигурации.
        var applicationDatabaseName = database.GetRequiredValue<string>("ApplicationDB");

        // Регистрируем MongoDbContext как синглтон.
        builder.Services.AddSingleton<MongoDbContext>(sp =>
        {
            // Получаем IMongoClient из контейнера зависимостей.
            var client = sp.GetRequiredService<IMongoClient>();
            
            // Создаем и возвращаем контекст MongoDB.
            return new MongoDbContext(client, applicationDatabaseName);
        });

        // Регистрируем фабрику обработчиков сессий как Singleton
        builder.Services.AddScoped<ISessionHandlerFactory, SessionHandlerFactory>();

        // Регистрируем UnitOfWork как реализацию интерфейса IUnitOfWork с областью видимости Scoped.
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        // Регистрирует сериализатор для типа Guid с использованием стандартного представления
        BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
    }
}
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using MongoDB.Driver.Core.Configuration;
using Identix.Application.Abstractions.Entities;

namespace Identix.Infrastructure.Common.DatabaseInitialization;

/// <summary>
/// Класс для создания индексов в коллекциях ASP.NET Identity
/// </summary>
internal static class IdentityMongoIndexCreator
{
    /// <summary>
    /// Создает индексы для всех коллекций, используемых в приложении.
    /// </summary>
    /// <param name="provider">Провайдер служб для извлечения необходимых сервисов.</param>
    public static async Task ConfigureAsync(IServiceProvider provider)
    {
        // Получаем конфигурацию приложения
        var configuration = provider.GetRequiredService<IConfiguration>();

        // Получаем строку подключения к MongoDB
        var connectionString = configuration.GetValue<string>("MongoDB:IdentityDB");
        
        // Создаем объект для парсинга строки подключения
        var cs = new ConnectionString(connectionString);

        // Создаем клиент MongoDB
        using var mongoClient = new MongoClient(connectionString);

        // Получаем базу данных
        var database = mongoClient.GetDatabase(cs.DatabaseName);

        // Создание индексов для коллекции Users
        await CreateUserIndexesAsync(database);

        // Создание индексов для коллекции Roles
        await CreateRoleIndexesAsync(database);
    }

    /// <summary>
    /// Создает индексы для коллекции Users.
    /// </summary>
    /// <param name="database">База данных MongoDB.</param>
    private static Task CreateUserIndexesAsync(IMongoDatabase database)
    {
        // Получаем коллекцию Users
        var usersCollection = database.GetCollection<AppUser>("Users");

        // Определяем ключи индекса для поля NormalizedEmail
        var normalizedEmailIndexKeys = Builders<AppUser>.IndexKeys.Ascending(u => u.NormalizedEmail);

        // Настраиваем опции индекса: уникальность
        var normalizedEmailIndexOptions = new CreateIndexOptions { Unique = true };

        // Создаем модель индекса
        var normalizedEmailIndexModel = new CreateIndexModel<AppUser>(normalizedEmailIndexKeys, normalizedEmailIndexOptions);

        // Создаем индекс в коллекции Users
       return usersCollection.Indexes.CreateOneAsync(normalizedEmailIndexModel);
    }

    /// <summary>
    /// Создает индексы для коллекции Roles.
    /// </summary>
    /// <param name="database">База данных MongoDB.</param>
    private static Task CreateRoleIndexesAsync(IMongoDatabase database)
    {
        // Получаем коллекцию Roles
        var rolesCollection = database.GetCollection<AppRole>("Roles");

        // Определяем ключи индекса для поля NormalizedName
        var normalizedNameIndexKeys = Builders<AppRole>.IndexKeys.Ascending(r => r.NormalizedName);

        // Настраиваем опции индекса: уникальность
        var normalizedNameIndexOptions = new CreateIndexOptions { Unique = true };

        // Создаем модель индекса
        var normalizedNameIndexModel = new CreateIndexModel<AppRole>(normalizedNameIndexKeys, normalizedNameIndexOptions);

        // Создаем индекс в коллекции Roles
        return rolesCollection.Indexes.CreateOneAsync(normalizedNameIndexModel);
    }
}
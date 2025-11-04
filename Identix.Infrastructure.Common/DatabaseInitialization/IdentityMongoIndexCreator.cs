using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
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
        // Получаем коллекцию пользователей.
        var usersCollection = provider.GetRequiredService<IMongoCollection<AppUser>>();

        // Получаем коллекцию ролей.
        var rolesCollection = provider.GetRequiredService<IMongoCollection<AppRole>>();
        
        // Создание индексов для коллекции Users
        await CreateUserIndexesAsync(usersCollection);

        // Создание индексов для коллекции Roles
        await CreateRoleIndexesAsync(rolesCollection);
    }

    /// <summary>
    /// Создает индексы для коллекции Users.
    /// </summary>
    /// <param name="usersCollection">Коллекция пользователей.</param>
    private static Task CreateUserIndexesAsync(IMongoCollection<AppUser> usersCollection)
    {
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
    /// <param name="rolesCollection">Коллекция ролей.</param>
    private static Task CreateRoleIndexesAsync(IMongoCollection<AppRole> rolesCollection)
    {
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
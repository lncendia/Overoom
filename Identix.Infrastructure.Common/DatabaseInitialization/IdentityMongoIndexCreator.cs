using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Identix.Application.Abstractions.Entities;

using Incendia.Identity.Mongo.Model;

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
    IMongoCollection<MongoUser<Guid, AppUser>> usersCollection = provider.GetRequiredService<IMongoCollection<MongoUser<Guid, AppUser>>>();

    // Получаем коллекцию ролей.
    IMongoCollection<MongoRole<Guid, AppRole>> rolesCollection = provider.GetRequiredService<IMongoCollection<MongoRole<Guid, AppRole>>>();

    // Создание индексов для коллекции Users
    await CreateUserIndexesAsync(usersCollection);

    // Создание индексов для коллекции Roles
    await CreateRoleIndexesAsync(rolesCollection);
  }

  /// <summary>
  /// Создает индексы для коллекции Users.
  /// </summary>
  /// <param name="usersCollection">Коллекция пользователей.</param>
  private static Task CreateUserIndexesAsync(IMongoCollection<MongoUser<Guid, AppUser>> usersCollection)
  {
    // Определяем ключи индекса для поля NormalizedEmail
    IndexKeysDefinition<MongoUser<Guid, AppUser>>? normalizedEmailIndexKeys = Builders<MongoUser<Guid, AppUser>>.IndexKeys
      .Ascending(u => u.User.NormalizedEmail);

    // Настраиваем опции индекса: уникальность
    var normalizedEmailIndexOptions = new CreateIndexOptions { Unique = true };

    // Создаем модель индекса
    var normalizedEmailIndexModel =
      new CreateIndexModel<MongoUser<Guid, AppUser>>(normalizedEmailIndexKeys, normalizedEmailIndexOptions);

    // Создаем индекс в коллекции Users
    return usersCollection.Indexes.CreateOneAsync(normalizedEmailIndexModel);
  }

  /// <summary>
  /// Создает индексы для коллекции Roles.
  /// </summary>
  /// <param name="rolesCollection">Коллекция ролей.</param>
  private static Task CreateRoleIndexesAsync(IMongoCollection<MongoRole<Guid, AppRole>> rolesCollection)
  {
    // Определяем ключи индекса для поля NormalizedName
    IndexKeysDefinition<MongoRole<Guid, AppRole>>? normalizedNameIndexKeys = Builders<MongoRole<Guid, AppRole>>.IndexKeys
      .Ascending(r => r.Role.NormalizedName);

    // Настраиваем опции индекса: уникальность
    var normalizedNameIndexOptions = new CreateIndexOptions { Unique = true };

    // Создаем модель индекса
    var normalizedNameIndexModel = new CreateIndexModel<MongoRole<Guid, AppRole>>(normalizedNameIndexKeys, normalizedNameIndexOptions);

    // Создаем индекс в коллекции Roles
    return rolesCollection.Indexes.CreateOneAsync(normalizedNameIndexModel);
  }
}

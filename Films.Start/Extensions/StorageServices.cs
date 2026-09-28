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
    IConfigurationSection database = builder.Configuration.GetSection("MongoDB");
    string applicationDatabaseName = database.GetRequiredValue<string>("ApplicationDB");

    builder.Services.AddSingleton<MongoDbContext>(sp =>
    {
      IMongoClient client = sp.GetRequiredService<IMongoClient>();

      return new MongoDbContext(client, applicationDatabaseName);
    });

    builder.Services.AddScoped<ISessionHandlerFactory, SessionHandlerFactory>();
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
    BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
  }
}
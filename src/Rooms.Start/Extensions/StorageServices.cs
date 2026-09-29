using Common.DI.Extensions;
using Common.Infrastructure.Repositories;
using Common.Application.Transactions;
using Common.Infrastructure.Transactions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Rooms.Application.Abstractions.Services;
using Rooms.Domain.Repositories;
using Rooms.Infrastructure.Storage;
using Rooms.Infrastructure.Storage.Context;
using Rooms.Infrastructure.Storage.Services;

namespace Rooms.Start.Extensions;

///<summary>
/// Статический класс сервисов хранилища.
///</summary>
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

    builder.Services.AddSingleton(TrackerConfiguration.ConfigureModelBuilder());
    builder.Services.AddScoped<MongoTransactionContext>();
    builder.Services.AddScoped<ITransactionContext>(sp => sp.GetRequiredService<MongoTransactionContext>());
    builder.Services.AddScoped<ITransactionManager>(sp => sp.GetRequiredService<MongoTransactionContext>());
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
    builder.Services.AddScoped<IMessagesCleaner, MessagesCleaner>();
    builder.Services.AddScoped<IViewerStatistics, ViewerStatistics>();
    BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
  }
}

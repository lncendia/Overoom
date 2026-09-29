using Rooms.Infrastructure.Storage.Models.Statistics;

using MongoDB.Driver;
using Rooms.Infrastructure.Storage.Models.Messages;
using Rooms.Infrastructure.Storage.Models.Rooms;

namespace Rooms.Infrastructure.Storage.Context;

/// <summary>
/// Контекст базы данных MongoDB для кинотеки.
/// Предоставляет доступ к коллекциям фильмов, плейлистов, комнат и других сущностей.
/// </summary>
public class MongoDbContext
{
  /// <summary>
  /// Клиент MongoDB для подключения к серверу
  /// </summary>
  public IMongoClient Client { get; }

  /// <summary>
  /// Коллекция комнат для совместного просмотра
  /// </summary>
  public IMongoCollection<RoomModel> Rooms { get; }

  /// <summary>
  /// Коллекция сообщений
  /// </summary>
  public IMongoCollection<MessageModel> Messages { get; }

  /// <summary>
  /// Коллекция счётчиков действий зрителей
  /// </summary>
  public IMongoCollection<ViewerStatisticModel> ViewerStatistics { get; }

  /// <summary>
  /// 
  /// </summary>
  private readonly IMongoDatabase _database;

  /// <summary>
  /// Инициализирует новый экземпляр контекста базы данных
  /// </summary>
  /// <param name="mongoClient">Клиент MongoDB</param>
  /// <param name="databaseName">Название базы данных</param>
  public MongoDbContext(IMongoClient mongoClient, string databaseName)
  {
    Client = mongoClient;
    _database = mongoClient.GetDatabase(databaseName);

    Rooms = _database.GetCollection<RoomModel>("Rooms");
    Messages = _database.GetCollection<MessageModel>("Messages");
    ViewerStatistics = _database.GetCollection<ViewerStatisticModel>("ViewerStatistics");
  }

  /// <summary>
  /// Асинхронно создает все необходимые коллекции и индексы
  /// </summary>
  /// <param name="cancellationToken">Токен отмены операции</param>
  public async Task EnsureCreatedAsync(CancellationToken cancellationToken = default)
  {
    await CreateCollectionsAsync(cancellationToken);
    await CreateViewerStatisticIndexesAsync(cancellationToken);
    await RemoveLegacyViewerStatisticAsync(cancellationToken);
  }

  /// <summary>
  /// Создаёт индексы коллекции счётчиков зрителей.
  /// </summary>
  /// <param name="cancellationToken">Токен отмены операции</param>
  private async Task CreateViewerStatisticIndexesAsync(CancellationToken cancellationToken)
  {
    IndexKeysDefinition<ViewerStatisticModel> roomViewerIndex = Builders<ViewerStatisticModel>.IndexKeys
      .Ascending(s => s.RoomId)
      .Ascending(s => s.ViewerId);

    await ViewerStatistics.Indexes.CreateOneAsync(
      new CreateIndexModel<ViewerStatisticModel>(roomViewerIndex, new CreateIndexOptions { Unique = true }),
      cancellationToken: cancellationToken);
  }

  /// <summary>
  /// Удаляет статистику, которая раньше хранилась внутри документов комнат.
  /// </summary>
  /// <param name="cancellationToken">Токен отмены операции</param>
  private async Task RemoveLegacyViewerStatisticAsync(CancellationToken cancellationToken)
  {
    await Rooms.UpdateManyAsync(
      Builders<RoomModel>.Filter.Exists("Viewers.Statistic"),
      Builders<RoomModel>.Update.Unset("Viewers.$[].Statistic"),
      cancellationToken: cancellationToken);
  }

  /// <summary>
  /// Асинхронно создает стандартные коллекции в базе данных, если они ещё не существуют.
  /// </summary>
  /// <param name="cancellationToken">Токен отмены для прерывания операции.</param>
  private async Task CreateCollectionsAsync(CancellationToken cancellationToken)
  {
    string[] collections =
    [
      "Rooms",
      "Messages",
      "ViewerStatistics"
    ];

    foreach (string collectionName in collections)
    {
      await _database.CreateCollectionAsync(collectionName, cancellationToken: cancellationToken);
    }
  }
}
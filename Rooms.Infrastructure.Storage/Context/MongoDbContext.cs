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
    }

    /// <summary>
    /// Асинхронно создает все необходимые коллекции и индексы
    /// </summary>
    /// <param name="cancellationToken">Токен отмены операции</param>
    public async Task EnsureCreatedAsync(CancellationToken cancellationToken = default)
    {
        await CreateCollectionsAsync(cancellationToken);
    }

    /// <summary>
    /// Асинхронно создает стандартные коллекции в базе данных, если они ещё не существуют.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены для прерывания операции.</param>
    private async Task CreateCollectionsAsync(CancellationToken cancellationToken)
    {
        // Перечень коллекций, необходимых для инициализации БД
        var collections = new[]
        {
            "Rooms",
            "Messages"
        };

        foreach (var collectionName in collections)
        {
            // Попытка создать коллекцию; если уже существует — будет выброшено исключение (возможно, стоит обрабатывать)
            await _database.CreateCollectionAsync(collectionName, cancellationToken: cancellationToken);
        }
    }

}
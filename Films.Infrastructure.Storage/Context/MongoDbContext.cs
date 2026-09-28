using Films.Infrastructure.Storage.Models.Comments;
using Films.Infrastructure.Storage.Models.Films;
using Films.Infrastructure.Storage.Models.Playlists;
using Films.Infrastructure.Storage.Models.Ratings;
using Films.Infrastructure.Storage.Models.Rooms;
using Films.Infrastructure.Storage.Models.Users;
using MongoDB.Driver;

namespace Films.Infrastructure.Storage.Context;

/// <summary>
/// Контекст базы данных MongoDB для кинотеки.
/// Предоставляет доступ к коллекциям фильмов, плейлистов, комнат и других сущностей.
/// </summary>
public class MongoDbContext
{
  /// <summary>
  /// Коллекция фильмов
  /// </summary>
  public IMongoCollection<FilmModel> Films { get; }

  /// <summary>
  /// Коллекция плейлистов
  /// </summary>
  public IMongoCollection<PlaylistModel> Playlists { get; }

  /// <summary>
  /// Коллекция комнат для совместного просмотра
  /// </summary>
  public IMongoCollection<RoomModel> Rooms { get; }

  /// <summary>
  /// Коллекция пользователей
  /// </summary>
  public IMongoCollection<UserModel> Users { get; }

  /// <summary>
  /// Коллекция комментариев
  /// </summary>
  public IMongoCollection<CommentModel> Comments { get; }

  /// <summary>
  /// Коллекция рейтингов
  /// </summary>
  public IMongoCollection<RatingModel> Ratings { get; }

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
    _database = mongoClient.GetDatabase(databaseName);

    Films = _database.GetCollection<FilmModel>("Films");
    Playlists = _database.GetCollection<PlaylistModel>("Playlists");
    Rooms = _database.GetCollection<RoomModel>("Rooms");
    Users = _database.GetCollection<UserModel>("Users");
    Comments = _database.GetCollection<CommentModel>("Comments");
    Ratings = _database.GetCollection<RatingModel>("Ratings");
  }

  /// <summary>
  /// Асинхронно создает все необходимые коллекции и индексы
  /// </summary>
  /// <param name="cancellationToken">Токен отмены операции</param>
  public async Task EnsureCreatedAsync(CancellationToken cancellationToken = default)
  {
    await CreateCollectionsAsync(cancellationToken);
    await CreateFilmIndexesAsync(cancellationToken);
    await CreateRoomIndexesAsync(cancellationToken);
    await CreateCommentIndexesAsync(cancellationToken);
    await CreateRatingIndexesAsync(cancellationToken);
    await BackfillFilmRatingsAsync(cancellationToken);
  }

  /// <summary>
  /// Асинхронно создает стандартные коллекции в базе данных, если они ещё не существуют.
  /// </summary>
  /// <param name="cancellationToken">Токен отмены для прерывания операции.</param>
  private async Task CreateCollectionsAsync(CancellationToken cancellationToken)
  {
    string[] collections =
    [
      "Films",
      "Playlists",
      "Rooms",
      "Users",
      "Comments",
      "Ratings"
    ];

    foreach (string collectionName in collections)
    {
      await _database.CreateCollectionAsync(collectionName, cancellationToken: cancellationToken);
    }
  }

  /// <summary>
  /// Создает индексы для коллекции фильмов
  /// </summary>
  private async Task CreateFilmIndexesAsync(CancellationToken cancellationToken)
  {
    IndexKeysDefinition<FilmModel>? titleYearIndex = Builders<FilmModel>.IndexKeys
      .Ascending(f => f.Title)
      .Ascending(f => f.Date);

    await Films.Indexes.CreateOneAsync(
      new CreateIndexModel<FilmModel>(titleYearIndex, new CreateIndexOptions
      {
        Unique = true
      }),
      cancellationToken: cancellationToken);

    IndexKeysDefinition<FilmModel>? textSearchIndex = Builders<FilmModel>.IndexKeys
      .Text(f => f.Title);

    await Films.Indexes.CreateOneAsync(
      new CreateIndexModel<FilmModel>(textSearchIndex),
      cancellationToken: cancellationToken);

    IndexKeysDefinition<FilmModel>? genreIndex = Builders<FilmModel>.IndexKeys
      .Ascending(f => f.Genres);

    await Films.Indexes.CreateOneAsync(
      new CreateIndexModel<FilmModel>(genreIndex),
      cancellationToken: cancellationToken);

    IndexKeysDefinition<FilmModel>? countryIndex = Builders<FilmModel>.IndexKeys
      .Ascending(f => f.Countries);

    await Films.Indexes.CreateOneAsync(
      new CreateIndexModel<FilmModel>(countryIndex),
      cancellationToken: cancellationToken);

    IndexKeysDefinition<FilmModel>? dateSortIndex = Builders<FilmModel>.IndexKeys
      .Descending(c => c.Date);

    await Films.Indexes.CreateOneAsync(
      new CreateIndexModel<FilmModel>(dateSortIndex),
      cancellationToken: cancellationToken);

    IndexKeysDefinition<FilmModel>? popularityIndex = Builders<FilmModel>.IndexKeys
      .Descending(f => f.UserRatingsCount);

    await Films.Indexes.CreateOneAsync(
      new CreateIndexModel<FilmModel>(popularityIndex),
      cancellationToken: cancellationToken);
  }

  /// <summary>
  /// Заполняет денормализованные счётчики оценок у фильмов, созданных до их появления.
  /// </summary>
  /// <param name="cancellationToken">Токен отмены операции</param>
  private async Task BackfillFilmRatingsAsync(CancellationToken cancellationToken)
  {
    FilterDefinition<FilmModel> notFilled = Builders<FilmModel>.Filter.Exists(f => f.UserRatingsCount, false);
    if (!await Films.Find(notFilled).AnyAsync(cancellationToken)) return;

    var stats = await Ratings.Aggregate()
      .Group(r => r.FilmId, g => new { FilmId = g.Key, Count = g.Count(), Sum = g.Sum(r => r.Score) })
      .ToListAsync(cancellationToken);

    UpdateOneModel<FilmModel>[] updates = stats
      .Select(s => new UpdateOneModel<FilmModel>(
        Builders<FilmModel>.Filter.And(Builders<FilmModel>.Filter.Eq(f => f.Id, s.FilmId), notFilled),
        Builders<FilmModel>.Update
          .Set(f => f.UserRatingsCount, s.Count)
          .Set(f => f.UserRatingsSum, s.Sum)))
      .ToArray();

    if (updates.Length > 0)
      await Films.BulkWriteAsync(updates, new BulkWriteOptions { IsOrdered = false }, cancellationToken);

    await Films.UpdateManyAsync(notFilled,
      Builders<FilmModel>.Update.Set(f => f.UserRatingsCount, 0).Set(f => f.UserRatingsSum, 0),
      cancellationToken: cancellationToken);
  }


  /// <summary>
  /// Создает индексы для коллекции комнат
  /// </summary>
  private async Task CreateRoomIndexesAsync(CancellationToken cancellationToken)
  {
    IndexKeysDefinition<RoomModel>? filmIndex = Builders<RoomModel>.IndexKeys
      .Ascending(r => r.FilmId);

    await Rooms.Indexes.CreateOneAsync(
      new CreateIndexModel<RoomModel>(filmIndex),
      cancellationToken: cancellationToken);

    IndexKeysDefinition<RoomModel>? dateSortIndex = Builders<RoomModel>.IndexKeys
      .Descending(c => c.CreatedAt);

    await Rooms.Indexes.CreateOneAsync(
      new CreateIndexModel<RoomModel>(dateSortIndex),
      cancellationToken: cancellationToken);

    IndexKeysDefinition<RoomModel>? viewersIndex = Builders<RoomModel>.IndexKeys
      .Ascending(f => f.Viewers);

    await Rooms.Indexes.CreateOneAsync(
      new CreateIndexModel<RoomModel>(viewersIndex),
      cancellationToken: cancellationToken);
  }

  /// <summary>
  /// Создает индексы для коллекции рейтингов.
  /// Оптимизирует запросы по:
  /// - Поиску рейтингов по фильму
  /// - Поиску рейтингов по пользователю
  /// - Проверке уникальности пары (фильм + пользователь)
  /// </summary>
  /// <param name="cancellationToken">Токен отмены операции</param>
  private async Task CreateRatingIndexesAsync(CancellationToken cancellationToken)
  {
    IndexKeysDefinition<RatingModel>? filmIndex = Builders<RatingModel>.IndexKeys
      .Ascending(r => r.FilmId);

    await Ratings.Indexes.CreateOneAsync(
      new CreateIndexModel<RatingModel>(filmIndex),
      cancellationToken: cancellationToken);

    IndexKeysDefinition<RatingModel>? userIndex = Builders<RatingModel>.IndexKeys
      .Ascending(r => r.UserId);

    await Ratings.Indexes.CreateOneAsync(
      new CreateIndexModel<RatingModel>(userIndex),
      cancellationToken: cancellationToken);

    IndexKeysDefinition<RatingModel>? uniqueUserFilmIndex = Builders<RatingModel>.IndexKeys
      .Ascending(r => r.UserId)
      .Ascending(r => r.FilmId);

    await Ratings.Indexes.CreateOneAsync(
      new CreateIndexModel<RatingModel>(uniqueUserFilmIndex, new CreateIndexOptions
      {
        Unique = true
      }),
      cancellationToken: cancellationToken);

    IndexKeysDefinition<RatingModel>? dateSortIndex = Builders<RatingModel>.IndexKeys
      .Descending(c => c.CreatedAt);

    await Ratings.Indexes.CreateOneAsync(
      new CreateIndexModel<RatingModel>(dateSortIndex),
      cancellationToken: cancellationToken);
  }

  /// <summary>
  /// Создает индексы для коллекции комментариев.
  /// Оптимизирует запросы по:
  /// - Поиску комментариев к фильму (FilmId)
  /// - Сортировке комментариев по дате создания (новые сначала)
  /// </summary>
  /// <param name="cancellationToken">Токен отмены операции</param>
  private async Task CreateCommentIndexesAsync(CancellationToken cancellationToken)
  {
    IndexKeysDefinition<CommentModel>? filmIndex = Builders<CommentModel>.IndexKeys
      .Ascending(c => c.FilmId);

    await Comments.Indexes.CreateOneAsync(
      new CreateIndexModel<CommentModel>(filmIndex),
      cancellationToken: cancellationToken);

    IndexKeysDefinition<CommentModel>? dateSortIndex = Builders<CommentModel>.IndexKeys
      .Descending(c => c.CreatedAt);

    await Comments.Indexes.CreateOneAsync(
      new CreateIndexModel<CommentModel>(dateSortIndex),
      cancellationToken: cancellationToken);
  }
}
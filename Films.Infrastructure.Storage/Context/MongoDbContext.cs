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
    /// Клиент MongoDB для подключения к серверу
    /// </summary>
    public IMongoClient Client { get; }

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
        Client = mongoClient;
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
            "Films",
            "Playlists",
            "Rooms",
            "Users",
            "Comments",
            "Ratings"
        };

        foreach (var collectionName in collections)
        {
            // Попытка создать коллекцию; если уже существует — будет выброшено исключение (возможно, стоит обрабатывать)
            await _database.CreateCollectionAsync(collectionName, cancellationToken: cancellationToken);
        }
    }

    /// <summary>
    /// Создает индексы для коллекции фильмов
    /// </summary>
    private async Task CreateFilmIndexesAsync(CancellationToken cancellationToken)
    {
        // 1. Уникальный составной индекс на название и год выпуска
        var titleYearIndex = Builders<FilmModel>.IndexKeys
            .Ascending(f => f.Title)
            .Ascending(f => f.Date);

        await Films.Indexes.CreateOneAsync(
            new CreateIndexModel<FilmModel>(titleYearIndex, new CreateIndexOptions
            {
                Unique = true
            }),
            cancellationToken: cancellationToken);

        // 2. Текстовый индекс для поиска по названию (case insensitive)
        var textSearchIndex = Builders<FilmModel>.IndexKeys
            .Text(f => f.Title);

        await Films.Indexes.CreateOneAsync(
            new CreateIndexModel<FilmModel>(textSearchIndex),
            cancellationToken: cancellationToken);

        // 3. Индекс для поиска по жанрам
        var genreIndex = Builders<FilmModel>.IndexKeys
            .Ascending(f => f.Genres);

        await Films.Indexes.CreateOneAsync(
            new CreateIndexModel<FilmModel>(genreIndex),
            cancellationToken: cancellationToken);

        // 4. Индекс для поиска по странам
        var countryIndex = Builders<FilmModel>.IndexKeys
            .Ascending(f => f.Countries);

        await Films.Indexes.CreateOneAsync(
            new CreateIndexModel<FilmModel>(countryIndex),
            cancellationToken: cancellationToken);
        
        // 5. Составной индекс для сортировки по дате съемки (новые сначала)
        // Оптимизирует:
        // - Сортировку фильмов от новых к старым
        // - Пагинацию с сортировкой
        var dateSortIndex = Builders<FilmModel>.IndexKeys
            .Descending(c => c.Date);

        await Films.Indexes.CreateOneAsync(
            new CreateIndexModel<FilmModel>(dateSortIndex),
            cancellationToken: cancellationToken);
    }


    /// <summary>
    /// Создает индексы для коллекции комнат
    /// </summary>
    private async Task CreateRoomIndexesAsync(CancellationToken cancellationToken)
    {
        // Индекс по идентификатору фильма
        var filmIndex = Builders<RoomModel>.IndexKeys
            .Ascending(r => r.FilmId);

        await Rooms.Indexes.CreateOneAsync(
            new CreateIndexModel<RoomModel>(filmIndex),
            cancellationToken: cancellationToken);
        
        // 2. Составной индекс для сортировки по дате съемки (новые сначала)
        // Оптимизирует:
        // - Сортировку фильмов от новых к старым
        // - Пагинацию с сортировкой
        var dateSortIndex = Builders<RoomModel>.IndexKeys
            .Descending(c => c.CreatedAt);

        await Rooms.Indexes.CreateOneAsync(
            new CreateIndexModel<RoomModel>(dateSortIndex),
            cancellationToken: cancellationToken);
        
        // 3. Индекс для поиска по зрителям
        var viewersIndex = Builders<RoomModel>.IndexKeys
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
        // 1. Индекс по FilmId для быстрого поиска всех оценок фильма.
        // Используется при:
        // - Показе средней оценки фильма
        // - Отображении всех оценок конкретного фильма
        var filmIndex = Builders<RatingModel>.IndexKeys
            .Ascending(r => r.FilmId);

        await Ratings.Indexes.CreateOneAsync(
            new CreateIndexModel<RatingModel>(filmIndex),
            cancellationToken: cancellationToken);

        // 2. Индекс по UserId для быстрого поиска оценок пользователя.
        // Используется при:
        // - Показе истории оценок пользователя
        // - Проверке активности пользователя
        var userIndex = Builders<RatingModel>.IndexKeys
            .Ascending(r => r.UserId);

        await Ratings.Indexes.CreateOneAsync(
            new CreateIndexModel<RatingModel>(userIndex),
            cancellationToken: cancellationToken);

        // 3. Уникальный составной индекс для обеспечения правила 
        // "Один пользователь - одна оценка на фильм"
        // Важно: Этот индекс НЕ заменяет отдельные индексы, а дополняет их
        var uniqueUserFilmIndex = Builders<RatingModel>.IndexKeys
            .Ascending(r => r.UserId)
            .Ascending(r => r.FilmId);

        await Ratings.Indexes.CreateOneAsync(
            new CreateIndexModel<RatingModel>(uniqueUserFilmIndex, new CreateIndexOptions
            {
                Unique = true
            }),
            cancellationToken: cancellationToken);
        
        // 4. Составной индекс для сортировки по дате оценки (новые сначала)
        // Оптимизирует:
        // - Сортировку оценок от новых к старым
        // - Пагинацию с сортировкой
        var dateSortIndex = Builders<RatingModel>.IndexKeys
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
        // 1. Базовый индекс по FilmId для быстрого поиска комментариев фильма.
        // Используется при:
        // - Показе всех комментариев к фильму
        // - Пагинации комментариев
        var filmIndex = Builders<CommentModel>.IndexKeys
            .Ascending(c => c.FilmId);

        await Comments.Indexes.CreateOneAsync(
            new CreateIndexModel<CommentModel>(filmIndex),
            cancellationToken: cancellationToken);

        // 2. Составной индекс для сортировки по дате создания (новые сначала)
        // Оптимизирует:
        // - Сортировку комментариев от новых к старым
        // - Пагинацию с сортировкой
        var dateSortIndex = Builders<CommentModel>.IndexKeys
            .Descending(c => c.CreatedAt);

        await Comments.Indexes.CreateOneAsync(
            new CreateIndexModel<CommentModel>(dateSortIndex),
            cancellationToken: cancellationToken);
    }
}
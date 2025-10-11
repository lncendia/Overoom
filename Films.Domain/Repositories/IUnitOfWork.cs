using Common.Infrastructure.Repositories;

namespace Films.Domain.Repositories;

/// <summary>
/// Интерфейс Unit of Work для работы с моделями данных кинотеки.
/// Предоставляет ленивую инициализацию репозиториев и управление транзакциями.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Лениво инициализируемый репозиторий для работы с комментариями
    /// </summary>
    Lazy<ICommentRepository> CommentRepository { get; }

    /// <summary>
    /// Лениво инициализируемый репозиторий для работы с фильмами
    /// </summary>
    Lazy<IFilmRepository> FilmRepository { get; }
    
    /// <summary>
    /// Лениво инициализируемый репозиторий для работы с комнатами просмотра
    /// </summary>
    Lazy<IRoomRepository> RoomRepository { get; }
    
    /// <summary>
    /// Лениво инициализируемый репозиторий для работы с плейлистами
    /// </summary>
    Lazy<IPlaylistRepository> PlaylistRepository { get; }
    
    /// <summary>
    /// Лениво инициализируемый репозиторий для работы с рейтингами
    /// </summary>
    Lazy<IRatingRepository> RatingRepository { get; }

    /// <summary>
    /// Лениво инициализируемый репозиторий для работы с пользователями
    /// </summary>
    Lazy<IUserRepository> UserRepository { get; }

    /// <summary>
    /// Асинхронно сохраняет все изменения, внесенные в репозитории, в рамках единой транзакции
    /// </summary>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <param name="handler">Обработчик сессии для выполнения операций</param>
    /// <returns>Задача, представляющая асинхронную операцию сохранения</returns>
    Task SaveChangesAsync(ISessionHandler? handler = null, CancellationToken cancellationToken = default);
}
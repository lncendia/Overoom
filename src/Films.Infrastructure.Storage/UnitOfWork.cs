using Common.Infrastructure.Repositories;
using Common.Infrastructure.Transactions;
using Films.Domain.Repositories;
using Films.Infrastructure.Storage.Context;
using Films.Infrastructure.Storage.Repositories;

using Incendia.MongoTracker.Builders;

using MediatR;
using Microsoft.Extensions.Logging;

using MongoDB.Driver;

namespace Films.Infrastructure.Storage;

/// <summary>
/// Класс, реализующий интерфейс IUnitOfWork.
/// Представляет собой единицу работы, которая отслеживает все изменения, внесенные в репозитории,
/// и предоставляет метод для сохранения этих изменений в базе данных.
/// </summary>
public class UnitOfWork : UnitOfWorkBase, IUnitOfWork
{
  /// <summary>
  /// Инициализирует новый экземпляр класса UnitOfWork.
  /// </summary>
  /// <param name="client">Клиент MongoDB</param>
  /// <param name="transaction">Текущая транзакция области</param>
  /// <param name="context">Контекст базы данных.</param>
  /// <param name="config">Конфигурация трекера</param>
  /// <param name="publisher">Публикатор событий.</param>
  /// <param name="logger">Логгер.</param>
  public UnitOfWork(IMongoClient client, ITransactionContext transaction, MongoDbContext context,
    ModelBuilder config, IPublisher publisher, ILogger<UnitOfWork> logger)
    : base(client, transaction, publisher, logger)
  {
    CommentRepository = new Lazy<ICommentRepository>(() => new CommentRepository(context, config, transaction));
    CommentReactionRepository = new Lazy<ICommentReactionRepository>(() =>
      new CommentReactionRepository(context, config, transaction));
    FilmRepository = new Lazy<IFilmRepository>(() => new FilmRepository(context, config, transaction));
    RoomRepository = new Lazy<IRoomRepository>(() => new RoomRepository(context, config, transaction));
    PlaylistRepository = new Lazy<IPlaylistRepository>(() => new PlaylistRepository(context, config, transaction));
    RatingRepository = new Lazy<IRatingRepository>(() => new RatingRepository(context, config, transaction));
    UserRepository = new Lazy<IUserRepository>(() => new UserRepository(context, config, transaction));
  }

  /// <summary>
  /// Лениво инициализируемый репозиторий для работы с комментариями
  /// </summary>
  public Lazy<ICommentRepository> CommentRepository { get; }

  /// <summary>
  /// Лениво инициализируемый репозиторий для работы с реакциями на комментарии
  /// </summary>
  public Lazy<ICommentReactionRepository> CommentReactionRepository { get; }

  /// <summary>
  /// Лениво инициализируемый репозиторий для работы с фильмами
  /// </summary>
  public Lazy<IFilmRepository> FilmRepository { get; }

  /// <summary>
  /// Лениво инициализируемый репозиторий для работы с комнатами просмотра
  /// </summary>
  public Lazy<IRoomRepository> RoomRepository { get; }

  /// <summary>
  /// Лениво инициализируемый репозиторий для работы с плейлистами
  /// </summary>
  public Lazy<IPlaylistRepository> PlaylistRepository { get; }

  /// <summary>
  /// Лениво инициализируемый репозиторий для работы с рейтингами
  /// </summary>
  public Lazy<IRatingRepository> RatingRepository { get; }

  /// <summary>
  /// Лениво инициализируемый репозиторий для работы с пользователями
  /// </summary>
  public Lazy<IUserRepository> UserRepository { get; }

  /// <inheritdoc/>
  /// <summary>
  /// Получает коллекцию репозиториев, в которых были изменения
  /// </summary>
  protected override IEnumerable<IRepository> GetRepositories()
  {
    if (CommentRepository.IsValueCreated)
      yield return (CommentRepository)CommentRepository.Value;

    if (CommentReactionRepository.IsValueCreated)
      yield return (CommentReactionRepository)CommentReactionRepository.Value;

    if (FilmRepository.IsValueCreated)
      yield return (FilmRepository)FilmRepository.Value;

    if (RoomRepository.IsValueCreated)
      yield return (RoomRepository)RoomRepository.Value;

    if (PlaylistRepository.IsValueCreated)
      yield return (PlaylistRepository)PlaylistRepository.Value;

    if (RatingRepository.IsValueCreated)
      yield return (RatingRepository)RatingRepository.Value;

    if (UserRepository.IsValueCreated)
      yield return (UserRepository)UserRepository.Value;
  }
}

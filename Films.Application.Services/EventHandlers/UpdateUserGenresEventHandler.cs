using Common.Application.Events;
using Common.Domain.Events;
using Films.Application.Abstractions.Exceptions;
using Films.Domain.Ratings;
using Films.Domain.Repositories;
using Films.Domain.Users;
using Films.Infrastructure.Storage.Context;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Films.Application.Services.EventHandlers;

/// <summary>
/// Обработчик доменного события создания рейтинга.
/// Обновляет жанровые предпочтения пользователя при добавлении новой оценки
/// </summary>
/// <param name="unitOfWork">Единица работы для взаимодействия с базой данных</param>
/// <param name="context">Контекст MongoDB для работы с рейтингами и фильмами</param>
public class UpdateUserGenresEventHandler(IUnitOfWork unitOfWork, MongoDbContext context)
  : BeforeSaveNotificationHandler<CreateEvent<Rating>>
{
  /// <summary>
  /// Обрабатывает событие создания рейтинга и обновляет жанровые предпочтения пользователя
  /// </summary>
  /// <param name="notification">Доменное событие создания рейтинга</param>
  /// <param name="cancellationToken">Токен отмены операции</param>
  /// <exception cref="UserNotFoundException">Если пользователь с указанным ID не найден</exception>
  protected override async Task Execute(CreateEvent<Rating> notification, CancellationToken cancellationToken)
  {
    User? user = await unitOfWork.UserRepository.Value.GetAsync(notification.Aggregate.UserId, cancellationToken);
    if (user == null) throw new UserNotFoundException(notification.Aggregate.UserId);

    List<User.FilmToUpdate>? genres = await context.Ratings.AsQueryable()
      .Where(x => x.UserId == notification.Aggregate.UserId)

      .OrderByDescending(x => x.CreatedAt)

      .Take(100)

      .GroupJoin(
        context.Films.AsQueryable(),

        r => r.FilmId,

        f => f.Id,

        (r, films) => new User.FilmToUpdate(films.First().Genres.ToArray())
      )
      .ToListAsync(cancellationToken: cancellationToken);

    user.UpdateGenres(genres);
    await unitOfWork.UserRepository.Value.UpdateAsync(user, cancellationToken);
  }
}
using Common.Application.Events;
using Common.Domain.Events;
using Films.Application.Abstractions.Exceptions;
using Films.Domain.Films;
using Films.Domain.Films.Specifications;
using Films.Domain.Repositories;

namespace Films.Application.Services.EventHandlers;

/// <summary>
/// Обработчик доменного события создания фильма
/// </summary>
/// <param name="unitOfWork">Единица работы для взаимодействия с базой данных</param>
public class FilmCreatedEventHandler(IUnitOfWork unitOfWork) : BeforeSaveNotificationHandler<CreateEvent<Film>>
{
  /// <summary>
  /// Обрабатывает событие создания фильма и проверяет на дубликаты
  /// </summary>
  /// <param name="notification">Доменное событие создания фильма</param>
  /// <param name="cancellationToken">Токен отмены операции</param>
  /// <exception cref="FilmAlreadyExistsException">Если фильм с таким названием и годом уже существует</exception>
  protected override async Task Execute(CreateEvent<Film> notification, CancellationToken cancellationToken)
  {
    var filmSpec = new DuplicateFilmsSpecification(notification.Aggregate.Title, notification.Aggregate.Date);
    IReadOnlyList<Film> count = await unitOfWork.FilmRepository.Value.FindAsync(filmSpec, cancellationToken: cancellationToken);

    if (count.Count > 0)
      throw new FilmAlreadyExistsException(notification.Aggregate.Title, notification.Aggregate.Date);
  }
}
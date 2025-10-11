using Common.Application.Events;
using Common.Domain.Events;
using Common.Domain.Specifications;
using Films.Application.Abstractions.Exceptions;
using Films.Domain.Ratings;
using Films.Domain.Ratings.Specifications;
using Films.Domain.Repositories;

namespace Films.Application.Services.EventHandlers;

/// <summary>
/// Обработчик доменного события создания рейтинга.
/// Выполняет проверку на дубликаты оценок перед сохранением
/// </summary>
/// <param name="unitOfWork">Единица работы для взаимодействия с базой данных</param>
public class RatingCreatedEventHandler(IUnitOfWork unitOfWork) : BeforeSaveNotificationHandler<CreateEvent<Rating>>
{
    /// <summary>
    /// Обрабатывает событие создания рейтинга и проверяет на дубликаты
    /// </summary>
    /// <param name="notification">Доменное событие создания рейтинга</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <exception cref="RatingAlreadyExistsException">Если оценка данного пользователя для данного фильма уже существует</exception>
    protected override async Task Execute(CreateEvent<Rating> notification, CancellationToken cancellationToken)
    {
        // Создаем спецификацию для поиска существующей оценки (ищем оценку данного пользователя для данного фильма)
        var spec = new RatingByFilmSpecification(notification.Aggregate.FilmId)
            .And(new RatingByUserSpecification(notification.Aggregate.UserId));

        // Ищем фильмы, удовлетворяющие критериям дубликатов
        var count = await unitOfWork.RatingRepository.Value.FindAsync(spec, cancellationToken: cancellationToken);

        // Если найдены совпадения - бросаем исключение
        // Это предотвращает создание дубликатов фильмов в системе
        if (count.Count > 0) throw new RatingAlreadyExistsException(notification.Aggregate.FilmId, notification.Aggregate.UserId);
    }
}
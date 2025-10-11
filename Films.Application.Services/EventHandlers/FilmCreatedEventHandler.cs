using Common.Application.Events;
using Common.Domain.Events;
using Common.Domain.Specifications;
using Films.Application.Abstractions.Exceptions;
using Films.Domain.Films;
using Films.Domain.Films.Specifications;
using Films.Domain.Repositories;

namespace Films.Application.Services.EventHandlers;

/// <summary>
/// Обработчик доменного события создания фильма
/// Выполняет проверку на дубликаты фильмов перед сохранением
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
        // Создаем комбинированную спецификацию для проверки дубликатов фильма:
        // - совпадение названия (FilmsByTitleSpecification)
        // - совпадение года выпуска (FilmsByDateSpecification)
        var filmSpec = new FilmsByTitleSpecification(notification.Aggregate.Title)
            .And(new FilmsByDateSpecification(notification.Aggregate.Date));

        // Ищем фильмы, удовлетворяющие критериям дубликатов
        var count = await unitOfWork.FilmRepository.Value.FindAsync(filmSpec, cancellationToken: cancellationToken);

        // Если найдены совпадения - бросаем исключение
        // Это предотвращает создание дубликатов фильмов в системе
        if (count.Count > 0) throw new FilmAlreadyExistsException(notification.Aggregate.Title, notification.Aggregate.Date);
    }
}
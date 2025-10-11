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
public class UpdateUserGenresEventHandler(IUnitOfWork unitOfWork, MongoDbContext context) : BeforeSaveNotificationHandler<CreateEvent<Rating>>
{
    /// <summary>
    /// Обрабатывает событие создания рейтинга и обновляет жанровые предпочтения пользователя
    /// </summary>
    /// <param name="notification">Доменное событие создания рейтинга</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <exception cref="UserNotFoundException">Если пользователь с указанным ID не найден</exception>
    protected override async Task Execute(CreateEvent<Rating> notification, CancellationToken cancellationToken)
    {
        // Получаем пользователя по ID из запроса
        var user = await unitOfWork.UserRepository.Value.GetAsync(notification.Aggregate.UserId, cancellationToken);

        // Проверяем существование пользователя
        if (user == null) throw new UserNotFoundException(notification.Aggregate.UserId);
        
        // Получаем информацию о фильмах пользователя из MongoDB для обновления жанров
        var genres = await context.Ratings.AsQueryable()
            // Фильтруем только оценки текущего пользователя
            .Where(x => x.UserId == notification.Aggregate.UserId)
        
            // Сортируем по дате оценки (новые сначала)
            // Это позволяет учитывать последние предпочтения пользователя
            .OrderByDescending(x => x.CreatedAt)
        
            // Берем 100 последних оценок для анализа
            // Ограничение введено для оптимизации производительности
            .Take(100)
        
            // Соединяем с коллекцией фильмов (LEFT JOIN)
            .GroupJoin(
                // Коллекция фильмов для соединения
                context.Films.AsQueryable(),
            
                // Ключ соединения из рейтингов (ID фильма)
                r => r.FilmId,
            
                // Ключ соединения из фильмов (ID фильма)
                f => f.Id,
            
                // Проекция результатов:
                // - Берем первый найденный фильм (т.к. FilmId уникален)
                // - Создаем объект FilmToUpdate с жанрами фильма
                (r, films) => new User.FilmToUpdate(
                    films.First().Genres.ToArray()
                )
            )
            // Преобразуем в список
            .ToListAsync(cancellationToken: cancellationToken);

        // Обновляем предпочтения пользователя по жанрам
        // - Анализируем последние 100 оцененных фильмов
        // - Вычисляем новые предпочтения на основе их жанров
        user.UpdateGenres(genres);
    
        // Сохраняем обновленного пользователя в репозитории
        // - Используем Unit of Work для сохранения изменений
        await unitOfWork.UserRepository.Value.UpdateAsync(user, cancellationToken);
    }
}
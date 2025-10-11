using Films.Application.Abstractions.Commands.Playlists;
using Films.Application.Abstractions.Exceptions;
using Films.Domain.Playlists;
using Films.Domain.Repositories;
using Films.Infrastructure.Storage.Context;
using MediatR;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Films.Application.Services.CommandHandlers.Playlists;

/// <summary>
/// Обработчик команды изменения плейлиста
/// </summary>
/// <param name="unitOfWork">Единица работы для доступа к репозиториям</param>
/// <param name="context">Контекст MongoDB для работы с фильмами</param>
public class ChangePlaylistCommandHandler(IUnitOfWork unitOfWork, MongoDbContext context)
    : IRequestHandler<ChangePlaylistCommand>
{
    /// <summary>
    /// Обрабатывает команду изменения плейлиста
    /// </summary>
    /// <param name="request">Команда с данными для изменения</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <exception cref="PlaylistNotFoundException">Выбрасывается если плейлист не найден</exception>
    public async Task Handle(ChangePlaylistCommand request, CancellationToken cancellationToken)
    {
        // Получаем плейлист по идентификатору
        var playlist = await unitOfWork.PlaylistRepository.Value.GetAsync(request.Id, cancellationToken);

        // Если плейлист не найден - выбрасываем исключение
        if (playlist == null) throw new PlaylistNotFoundException(request.Id);

        // Обновляем описание плейлиста, если оно указано в запросе
        playlist.Description = request.Description;

        // Обработка обновления списка фильмов
        if (request.Films != null)
        {
            // Получаем информацию о фильмах из MongoDB
            var films = await context.Films.AsQueryable()

                // Фильтруем только запрошенные фильмы
                .Where(x => request.Films.Contains(x.Id))

                // Проецируем в упрощенную модель (ID и жанры)
                .Select(x => new Playlist.FilmToUpdate(x.Id, x.Genres.ToArray()))

                // Преобразуем в список
                .ToListAsync(cancellationToken: cancellationToken);

            // Проверяем, что все запрошенные фильмы найдены
            foreach (var film in request.Films)
            {
                if (films.All(f => f.Id != film))
                    throw new FilmNotFoundException(film);
            }

            // Обновляем список фильмов в плейлисте
            playlist.UpdateFilms(films);
        }

        // Сохраняем изменения плейлиста в репозитории
        await unitOfWork.PlaylistRepository.Value.UpdateAsync(playlist, cancellationToken);

        // Фиксируем изменения в базе данных
        await unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
    }
}
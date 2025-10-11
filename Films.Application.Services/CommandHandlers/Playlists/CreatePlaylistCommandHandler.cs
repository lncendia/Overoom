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
/// Обработчик команды создания нового плейлиста
/// </summary>
/// <param name="unitOfWork">Единица работы для взаимодействия с базой данных</param>
/// <param name="context">Контекст MongoDB для работы с фильмами</param>
public class CreatePlaylistCommandHandler(IUnitOfWork unitOfWork, MongoDbContext context)
    : IRequestHandler<CreatePlaylistCommand, Guid>
{
    /// <summary>
    /// Создает новый плейлист с указанными параметрами
    /// </summary>
    /// <param name="request">Данные для создания плейлиста (название, описание, обложка)</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Идентификатор созданного плейлиста</returns>
    /// <exception cref="PlaylistAlreadyExistsException">Если плейлист с таким именем уже существует</exception>
    public async Task<Guid> Handle(CreatePlaylistCommand request, CancellationToken cancellationToken)
    {
        // Создаем новый плейлист
        var playlist = new Playlist(Guid.NewGuid())
        {
            Name = request.Name,
            Description = request.Description,
            PosterKey = Constants.PlaylistPosterKeyDefault
        };

        // Обработка обновления списка фильмов
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

        // Добавляем плейлист в репозиторий
        await unitOfWork.PlaylistRepository.Value.AddAsync(playlist, cancellationToken);

        // Сохраняем изменения в базе данных
        await unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);

        // Возвращаем идентификатор созданного плейлиста
        return playlist.Id;
    }
}
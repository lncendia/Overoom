using Films.Application.Abstractions;
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
    var playlist = new Playlist(Guid.NewGuid())
    {
      Name = request.Name,
      Description = request.Description,
      PosterKey = Constants.Poster.PlaylistDefault
    };

    List<Playlist.FilmToUpdate>? films = await context.Films.AsQueryable()

      .Where(x => request.Films.Contains(x.Id))

      .Select(x => new Playlist.FilmToUpdate(x.Id, x.Genres.ToArray()))

      .ToListAsync(cancellationToken: cancellationToken);

    foreach (Guid film in request.Films)
    {
      if (films.All(f => f.Id != film))
        throw new FilmNotFoundException(film);
    }

    playlist.UpdateFilms(films);
    await unitOfWork.PlaylistRepository.Value.AddAsync(playlist, cancellationToken);
    await unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);

    return playlist.Id;
  }
}
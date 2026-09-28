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
    Playlist? playlist = await unitOfWork.PlaylistRepository.Value.GetAsync(request.Id, cancellationToken);
    if (playlist == null) throw new PlaylistNotFoundException(request.Id);

    playlist.Description = request.Description;

    if (request.Films != null)
    {
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
    }

    await unitOfWork.PlaylistRepository.Value.UpdateAsync(playlist, cancellationToken);
    await unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
  }
}

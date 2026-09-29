using Films.Application.Abstractions.Commands.Playlists;
using Films.Application.Abstractions.Exceptions;
using Films.Domain.Films;
using Films.Domain.Films.Specifications;
using Films.Domain.Playlists;
using Films.Domain.Repositories;
using MediatR;

namespace Films.Application.Services.CommandHandlers.Playlists;

/// <summary>
/// Обработчик команды изменения плейлиста
/// </summary>
/// <param name="unitOfWork">Единица работы для доступа к репозиториям</param>
public class ChangePlaylistCommandHandler(IUnitOfWork unitOfWork)
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
      IReadOnlyList<Film> found = await unitOfWork.FilmRepository.Value.FindAsync(
        new FilmsByIdsSpecification(request.Films), cancellationToken: cancellationToken);

      List<Playlist.FilmToUpdate> films = [.. found.Select(f => new Playlist.FilmToUpdate(f.Id, [.. f.Genres]))];

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

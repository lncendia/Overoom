using Common.Application.FileStorage;
using Films.Application.Abstractions.Commands.Playlists;
using Films.Application.Abstractions.Exceptions;
using Films.Domain.Playlists;
using Films.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Films.Application.Services.CommandHandlers.Playlists;

/// <summary>
/// Обработчик команды удаления плейлиста
/// </summary>
/// <param name="unitOfWork">Единица работы для взаимодействия с базой данных</param>
/// <param name="posterStore">Хранилище файлов для удаления обложки плейлиста</param>
public class DeletePlaylistCommandHandler(
  IUnitOfWork unitOfWork,
  IFileStorage posterStore,
  ILogger<DeletePlaylistCommandHandler> logger)
  : IRequestHandler<DeletePlaylistCommand>
{
  /// <summary>
  /// Обрабатывает запрос на удаление плейлиста
  /// </summary>
  /// <param name="request">Команда удаления (ID плейлиста)</param>
  /// <param name="cancellationToken">Токен отмены операции</param>
  /// <exception cref="PlaylistNotFoundException">Если плейлист с указанным ID не найден</exception>
  public async Task Handle(DeletePlaylistCommand request, CancellationToken cancellationToken)
  {
    Playlist? playlist = await unitOfWork.PlaylistRepository.Value.GetAsync(request.Id, cancellationToken);
    if (playlist == null) throw new PlaylistNotFoundException(request.Id);

    try
    {
      await posterStore.DeleteAsync(playlist.PosterKey, token: cancellationToken);
    }
    catch (FileNotFoundException)
    {
      logger.LogWarning("Файл обложки {poster} не найден в хранилище", playlist.PosterKey);
    }

    await unitOfWork.PlaylistRepository.Value.DeleteAsync(playlist, cancellationToken);
    await unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
  }
}
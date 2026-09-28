using Common.Application.FileStorage;
using Films.Application.Abstractions.Commands.Films;
using Films.Application.Abstractions.Exceptions;
using Films.Domain.Films;
using Films.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Films.Application.Services.CommandHandlers.Films;

/// <summary>
/// Обработчик команды удаления фильма
/// </summary>
/// <param name="unitOfWork">Единица работы для взаимодействия с базой данных</param>
/// <param name="posterStore">Хранилище файлов для удаления постера</param>
/// <param name="logger">Логгер для записи событий</param>
public class DeleteFilmCommandHandler(
  IUnitOfWork unitOfWork,
  IFileStorage posterStore,
  ILogger<DeleteFilmCommandHandler> logger)
  : IRequestHandler<DeleteFilmCommand>
{
  /// <summary>
  /// Обрабатывает запрос на удаление фильма
  /// </summary>
  /// <param name="request">Команда удаления (ID фильма)</param>
  /// <param name="cancellationToken">Токен отмены операции</param>
  /// <exception cref="FilmNotFoundException">Если фильм с указанным ID не найден</exception>
  public async Task Handle(DeleteFilmCommand request, CancellationToken cancellationToken)
  {
    Film? film = await unitOfWork.FilmRepository.Value.GetAsync(request.Id, cancellationToken);
    if (film == null) throw new FilmNotFoundException(request.Id);

    try
    {
      await posterStore.DeleteAsync(film.PosterKey, token: cancellationToken);
    }
    catch (FileNotFoundException)
    {
      logger.LogWarning("Файл обложки {poster} не найден в хранилище", film.PosterKey);
    }

    await unitOfWork.FilmRepository.Value.DeleteAsync(film, cancellationToken);
    await unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
  }
}
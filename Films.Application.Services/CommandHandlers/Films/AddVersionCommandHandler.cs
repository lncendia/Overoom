using Films.Application.Abstractions.Commands.Films;
using Films.Application.Abstractions.Exceptions;
using Films.Domain.Films;
using Films.Domain.Repositories;
using MediatR;

namespace Films.Application.Services.CommandHandlers.Films;

/// <summary>
/// Обработчик команды добавления новой версии фильма или серии
/// </summary>
/// <param name="unitOfWork">Единица работы для взаимодействия с базой данных</param>
public class AddVersionCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<AddVersionCommand>
{
  /// <summary>
  /// Обрабатывает команду добавления новой версии фильма или серии
  /// </summary>
  /// <param name="request">Команда с данными для добавления версии</param>
  /// <param name="cancellationToken">Токен отмены операции</param>
  /// <returns>Задача, представляющая асинхронную операцию</returns>
  /// <exception cref="FilmNotFoundException">Если фильм с указанным ID не найден</exception>
  public async Task Handle(AddVersionCommand request, CancellationToken cancellationToken)
  {
    Film? film = await unitOfWork.FilmRepository.Value.GetAsync(request.FilmId, cancellationToken);
    if (film == null) throw new FilmNotFoundException(request.FilmId);

    film.AddVersion(
      version: request.Version,
      seasonNumber: request.Season,
      episodeNumber: request.Episode
    );

    await unitOfWork.FilmRepository.Value.UpdateAsync(film, cancellationToken);
    await unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
  }
}
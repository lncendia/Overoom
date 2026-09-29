using Films.Application.Abstractions.Commands.Films;
using Films.Application.Abstractions.Exceptions;
using Films.Domain.Films;
using Films.Domain.Films.ValueObjects;
using Films.Domain.Repositories;
using MediatR;

namespace Films.Application.Services.CommandHandlers.Films;

/// <summary>
/// Обработчик команды изменения данных фильма
/// </summary>
/// <param name="unitOfWork">Единица работы для взаимодействия с базой данных</param>
public class ChangeFilmCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<ChangeFilmCommand>
{
  /// <summary>
  /// Обновляет информацию о фильме
  /// </summary>
  /// <param name="request">Команда с обновляемыми данными фильма</param>
  /// <param name="cancellationToken">Токен отмены операции</param>
  /// <exception cref="FilmNotFoundException">Если фильм с указанным ID не найден</exception>
  public async Task Handle(ChangeFilmCommand request, CancellationToken cancellationToken)
  {
    Film? film = await unitOfWork.FilmRepository.Value.GetAsync(request.Id, cancellationToken);
    if (film == null) throw new FilmNotFoundException(request.Id);

    film.Description = request.Description;

    if (!string.IsNullOrEmpty(request.ShortDescription))
      film.ShortDescription = request.ShortDescription;

    if (request.RatingKp.HasValue)
      film.RatingKp = new Rating(request.RatingKp.Value);

    if (request.RatingImdb.HasValue)
      film.RatingImdb = new Rating(request.RatingImdb.Value);

    await unitOfWork.FilmRepository.Value.UpdateAsync(film, cancellationToken);
    await unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
  }
}
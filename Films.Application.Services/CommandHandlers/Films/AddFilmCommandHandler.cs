using Films.Application.Abstractions;
using Films.Application.Abstractions.Commands.Films;
using Films.Domain.Films;
using Films.Domain.Films.ValueObjects;
using Films.Domain.Repositories;
using MediatR;

namespace Films.Application.Services.CommandHandlers.Films;

/// <summary>
/// Обработчик команды добавления нового фильма/сериала
/// </summary>
/// <param name="unitOfWork">Единица работы для взаимодействия с базой данных</param>
public class AddFilmCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<AddFilmCommand, Guid>
{
  /// <summary>
  /// Обрабатывает запрос на добавление нового фильма
  /// </summary>
  /// <param name="request">Данные о фильме (основная информация, постер)</param>
  /// <param name="cancellationToken">Токен отмены операции</param>
  /// <returns>ID созданного фильма</returns>
  public async Task<Guid> Handle(AddFilmCommand request, CancellationToken cancellationToken)
  {
    var id = Guid.NewGuid();

    var film = new Film(id)
    {
      Title = request.Title,
      Description = request.Description,
      Date = request.Date,
      Genres = request.Genres,
      Countries = request.Countries,
      Directors = request.Directors,
      Actors = request.Actors,
      Screenwriters = request.Screenwriters,
      RatingImdb = request.RatingImdb.HasValue ? new Rating(request.RatingImdb.Value) : null,
      RatingKp = request.RatingKp.HasValue ? new Rating(request.RatingKp.Value) : null,
      PosterKey = Constants.Poster.FilmDefault
    };

    if (!string.IsNullOrEmpty(request.ShortDescription))
      film.ShortDescription = request.ShortDescription;

    await unitOfWork.FilmRepository.Value.AddAsync(film, cancellationToken);
    await unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);

    return film.Id;
  }
}
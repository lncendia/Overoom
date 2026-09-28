using Films.Application.Abstractions.DTOs.Films;
using Films.Application.Abstractions.Queries.Films;
using Films.Infrastructure.Storage.Context;
using MediatR;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Films.Application.Services.QueryHandlers.Films;

/// <summary>
/// Обработчик запроса для получения популярных фильмов
/// </summary>
/// <param name="context">Контекст базы данных MongoDB</param>
public class GetPopularFilmsQueryHandler(MongoDbContext context)
  : IRequestHandler<GetPopularFilmsQuery, IReadOnlyList<FilmShortDto>>
{
  /// <summary>
  /// Обрабатывает запрос на получение списка популярных фильмов
  /// </summary>
  /// <param name="request">Запрос с параметрами (включая количество фильмов для возврата)</param>
  /// <param name="cancellationToken">Токен отмены операции</param>
  /// <returns>Список DTO популярных фильмов</returns>
  public async Task<IReadOnlyList<FilmShortDto>> Handle(GetPopularFilmsQuery request,
    CancellationToken cancellationToken)
  {
    return await context.Films.AsQueryable()
      .OrderByDescending(f => f.UserRatingsCount)
      .Take(request.Take)
      .Select(f => new FilmShortDto
      {
        Id = f.Id,
        Title = f.Title,
        PosterKey = f.PosterKey,
        Year = f.Date.Year,
        RatingKp = f.RatingKp,
        RatingImdb = f.RatingImdb,
        Description = f.ShortDescription,
        IsSerial = f.Seasons != null && f.Content == null,
        Genres = f.Genres
      })
      .ToListAsync(cancellationToken);
  }
}
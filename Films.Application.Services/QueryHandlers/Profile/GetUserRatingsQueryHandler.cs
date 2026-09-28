using Common.Application.DTOs;
using Films.Application.Abstractions.DTOs.Profile;
using Films.Application.Abstractions.Queries.Profile;
using Films.Infrastructure.Storage.Context;
using Films.Infrastructure.Storage.Models.Ratings;

using MediatR;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Films.Application.Services.QueryHandlers.Profile;

/// <summary>
/// Обработчик запроса для получения оценок пользователя
/// </summary>
/// <param name="context">Контекст базы данных MongoDB</param>
public class GetUserRatingsQueryHandler(MongoDbContext context)
  : IRequestHandler<GetUserRatingsQuery, CountResult<UserRatingDto>>
{
  /// <summary>
  /// Обрабатывает запрос на получение списка оценок пользователя
  /// </summary>
  /// <param name="request">Запрос, содержащий идентификатор пользователя</param>
  /// <param name="cancellationToken">Токен отмены операции</param>
  /// <returns>Результат с оценками пользователя и общим количеством</returns>
  public async Task<CountResult<UserRatingDto>> Handle(GetUserRatingsQuery request, CancellationToken cancellationToken)
  {
    IQueryable<RatingModel> baseQuery = context.Ratings.AsQueryable()
      .Where(r => r.UserId == request.Id);

    int count = await baseQuery.CountAsync(cancellationToken);
    if (count == 0) return CountResult<UserRatingDto>.NoValues();

    List<UserRatingDto>? films = await baseQuery
      .OrderByDescending(x => x.CreatedAt)
      .Skip(request.Skip)
      .Take(request.Take)
      .GroupJoin(
        context.Films.AsQueryable(),
        rating => rating.FilmId,
        film => film.Id,
        (rating, films) => new { Film = films.First(), rating.Score }
      )
      .Select(x => new UserRatingDto
      {
        Id = x.Film.Id,
        Title = x.Film.Title,
        PosterKey = x.Film.PosterKey,
        Year = x.Film.Date.Year,
        RatingKp = x.Film.RatingKp,
        RatingImdb = x.Film.RatingImdb,
        Score = x.Score,
        Description = x.Film.ShortDescription,
        IsSerial = x.Film.Seasons != null && x.Film.Content == null,
        Genres = x.Film.Genres
      })
      .ToListAsync(cancellationToken);

    return new CountResult<UserRatingDto>
    {
      List = films,
      TotalCount = count
    };
  }
}

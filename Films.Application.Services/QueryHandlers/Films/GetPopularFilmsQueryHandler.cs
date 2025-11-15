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
        // Выполняем запрос и получаем результаты в виде списка
        return await context.Films.AsQueryable()
            .GroupJoin(
                context.Ratings.AsQueryable(),
                film => film.Id,
                rating => rating.FilmId,
                (film, ratings) => new
                {
                    Film = film,
                    // ReSharper disable once PossibleMultipleEnumeration
                    UserRating = ratings.Average(r => r.Score),
                    // ReSharper disable once PossibleMultipleEnumeration
                    UserRatingCount = ratings.Count()
                }
            )
            .OrderByDescending(x => x.UserRatingCount)
            .Take(request.Take)
            .Select(x => new FilmShortDto
            {
                Id = x.Film.Id,
                Title = x.Film.Title,
                PosterKey = x.Film.PosterKey,
                Year = x.Film.Date.Year,
                RatingKp = x.Film.RatingKp,
                RatingImdb = x.Film.RatingImdb,
                Description = x.Film.ShortDescription,
                IsSerial = x.Film.Seasons != null && x.Film.Content == null,
                Genres = x.Film.Genres
            })
            .ToListAsync(cancellationToken);
    }
}
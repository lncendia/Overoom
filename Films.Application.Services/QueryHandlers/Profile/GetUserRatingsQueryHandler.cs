using Common.Application.DTOs;
using Films.Application.Abstractions.DTOs.Profile;
using Films.Application.Abstractions.Queries.Profile;
using Films.Infrastructure.Storage.Context;
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
        // Создаем базовый запрос к коллекции оценок
        var baseQuery = context.Ratings.AsQueryable()
            .Where(r => r.UserId == request.Id);

        // Получаем общее количество найденных оценок
        var count = await baseQuery.CountAsync(cancellationToken);

        // Если оценки не найдены, возвращаем пустой результат
        if (count == 0) return CountResult<UserRatingDto>.NoValues();

        // Выполняем запрос и получаем результаты в виде списка
        var films = await baseQuery
            .OrderByDescending(x => x.CreatedAt)
            .Skip(request.Skip)
            .Take(request.Take)
            .GroupJoin(
                context.Films.AsQueryable(),
                rating => rating.FilmId,
                film => film.Id,
                (rating, films) => new { Rating = rating, Film = films.First() }
            )
            .GroupJoin(
                context.Ratings.AsQueryable(),
                film => film.Film.Id,
                rating => rating.FilmId,
                (film, ratings) => new
                {
                    film.Film,
                    film.Rating.Score,
                    Date = film.Rating.CreatedAt,
                    // ReSharper disable once PossibleMultipleEnumeration
                    UserRating = ratings.Average(r => r.Score)
                }
            )
            .OrderByDescending(x => x.Date)
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

        // Возвращаем результат с данными и общим количеством
        return new CountResult<UserRatingDto>
        {
            List = films,
            TotalCount = count
        };
    }
}
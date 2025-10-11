using Common.Application.DTOs;
using Films.Application.Abstractions.DTOs.Rooms;
using Films.Application.Abstractions.Queries.Rooms;
using Films.Infrastructure.Storage.Context;
using MediatR;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Films.Application.Services.QueryHandlers.Rooms;

/// <summary>
/// Обработчик запроса для поиска комнат с фильтрацией
/// </summary>
/// <param name="context">Контекст базы данных MongoDB</param>
public class SearchRoomsQueryHandler(MongoDbContext context) 
    : IRequestHandler<SearchRoomsQuery, CountResult<RoomShortDto>>
{
    /// <summary>
    /// Обрабатывает запрос на поиск комнат с пагинацией
    /// </summary>
    /// <param name="request">Параметры поиска (фильтры, пагинация)</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Результат с найденными комнатами и общим количеством</returns>
    public async Task<CountResult<RoomShortDto>> Handle(
        SearchRoomsQuery request, 
        CancellationToken cancellationToken)
    {
        // Базовый запрос к коллекции комнат
        var baseQuery = context.Rooms.AsQueryable();

        // Фильтр по ID фильма (если указан)
        if (request.FilmId.HasValue)
            baseQuery = baseQuery.Where(x => x.FilmId == request.FilmId.Value);

        // Фильтр только публичных комнат (без кода доступа)
        if (request.OnlyPublic)
            baseQuery = baseQuery.Where(r => r.Code == null);
        
        // Получаем общее количество комнат (до пагинации)
        var count = await baseQuery.CountAsync(cancellationToken: cancellationToken);

        // Если комнат не найдено - возвращаем пустой результат
        if (count == 0) return CountResult<RoomShortDto>.NoValues();

        // Получаем список комнат с информацией о фильмах
        var list = await baseQuery
            .GroupJoin(
                context.Films.AsQueryable(),
                room => room.FilmId,
                film => film.Id,
                (room, films) => new { Room = room, Film = films.First() }
            )
            .Select(x => new RoomShortDto
            {
                // Информация о фильме
                Title = x.Film.Title,
                PosterKey = x.Film.PosterKey,
                Year = x.Film.Date.Year,
                RatingKp = x.Film.RatingKp,
                RatingImdb = x.Film.RatingImdb,
                Description = x.Film.Description,
                IsSerial = x.Film.Seasons != null && x.Film.Content == null,
                FilmId = x.Film.Id,
                Genres = x.Film.Genres,

                // Информация о комнате
                Id = x.Room.Id,
                ViewersCount = x.Room.Viewers.Count,
                IsPrivate = !string.IsNullOrEmpty(x.Room.Code),
            })
            .ToListAsync(cancellationToken: cancellationToken);
        
        // Возвращаем результат с данными и общим количеством
        return new CountResult<RoomShortDto>
        {
            List = list,
            TotalCount = count
        };
    }
}
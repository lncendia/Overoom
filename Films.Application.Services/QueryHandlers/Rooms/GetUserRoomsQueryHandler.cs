using Films.Application.Abstractions.DTOs.Rooms;
using Films.Application.Abstractions.Queries.Rooms;
using Films.Infrastructure.Storage.Context;
using MediatR;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Films.Application.Services.QueryHandlers.Rooms;

/// <summary>
/// Обработчик запроса для получения списка комнат пользователя
/// </summary>
/// <param name="context">Контекст базы данных MongoDB</param>
public class GetUserRoomsQueryHandler(MongoDbContext context)
    : IRequestHandler<GetUserRoomsQuery, IReadOnlyList<RoomShortDto>>
{
    /// <summary>
    /// Обрабатывает запрос на получение списка комнат пользователя
    /// </summary>
    /// <param name="request">Запрос, содержащий ID пользователя</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Список DTO с краткой информацией о комнатах</returns>
    public async Task<IReadOnlyList<RoomShortDto>> Handle(GetUserRoomsQuery request,
        CancellationToken cancellationToken)
    {
        // Получаем комнату из базы данных с агрегацией связанных данных
        return await context.Rooms.AsQueryable()
            .Where(r => r.Viewers.Contains(request.UserId))
            .GroupJoin(
                context.Films.AsQueryable(),
                room => room.FilmId,
                film => film.Id,
                (room, films) => new { Room = room, Film = films.First() }
            )
            .GroupJoin(
                context.Users.AsQueryable(),
                room => room.Room.OwnerId,
                user => user.Id,
                (room, users) => new { room.Room, room.Film, User = users.First() }
            )
            .Select(x => new RoomShortDto
            {
                Title = x.Film.Title,
                PosterKey = x.Film.PosterKey,
                Year = x.Film.Date.Year,
                RatingKp = x.Film.RatingKp,
                RatingImdb = x.Film.RatingImdb,
                Description = x.Film.Description,
                IsSerial = x.Film.Seasons != null && x.Film.Content == null,
                Genres = x.Film.Genres,
                FilmId = x.Film.Id,
                Id = x.Room.Id,
                ViewersCount = x.Room.Viewers.Count,
                IsPrivate = !string.IsNullOrEmpty(x.Room.Code),
                UserName = x.User.Username,
                PhotoKey = x.User.PhotoKey
            })
            .ToListAsync(cancellationToken: cancellationToken);
    }
}
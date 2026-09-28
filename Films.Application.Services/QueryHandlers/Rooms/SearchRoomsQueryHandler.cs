using Common.Application.DTOs;
using Films.Application.Abstractions.DTOs.Rooms;
using Films.Application.Abstractions.Queries.Rooms;
using Films.Infrastructure.Storage.Context;
using Films.Infrastructure.Storage.Models.Rooms;

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
    IQueryable<RoomModel>? baseQuery = context.Rooms.AsQueryable();

    if (request.FilmId.HasValue)
      baseQuery = baseQuery.Where(x => x.FilmId == request.FilmId.Value);

    if (request.OnlyPublic)
      baseQuery = baseQuery.Where(r => r.Code == null);

    int count = await baseQuery.CountAsync(cancellationToken: cancellationToken);
    if (count == 0) return CountResult<RoomShortDto>.NoValues();

    List<RoomShortDto>? list = await baseQuery
      .OrderByDescending(r => r.CreatedAt)
      .Skip(request.Skip)
      .Take(request.Take)
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
        FilmId = x.Film.Id,
        Genres = x.Film.Genres,

        Id = x.Room.Id,
        ViewersCount = x.Room.Viewers.Count,
        IsPrivate = !string.IsNullOrEmpty(x.Room.Code),

        UserName = x.User.Username,
        PhotoKey = x.User.PhotoKey
      })
      .ToListAsync(cancellationToken: cancellationToken);

    return new CountResult<RoomShortDto>
    {
      List = list,
      TotalCount = count
    };
  }
}
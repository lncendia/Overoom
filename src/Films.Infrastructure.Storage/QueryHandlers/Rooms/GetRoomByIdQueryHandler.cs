using Films.Application.Abstractions.DTOs.Rooms;
using Films.Application.Abstractions.Exceptions;
using Films.Application.Abstractions.Queries.Rooms;
using Films.Infrastructure.Storage.Context;
using MediatR;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Films.Infrastructure.Storage.QueryHandlers.Rooms;

/// <summary>
/// Обработчик запроса для получения информации о комнате по идентификатору
/// </summary>
/// <param name="context">Контекст базы данных MongoDB</param>
public class GetRoomByIdQueryHandler(MongoDbContext context) : IRequestHandler<GetRoomByIdQuery, RoomDto>
{
  /// <summary>
  /// Обрабатывает запрос на получение данных комнаты
  /// </summary>
  /// <param name="request">Запрос, содержащий ID комнаты и ID пользователя</param>
  /// <param name="cancellationToken">Токен отмены операции</param>
  /// <returns>DTO с полной информацией о комнате</returns>
  /// <exception cref="RoomNotFoundException">Выбрасывается если комната не найдена</exception>
  public async Task<RoomDto> Handle(GetRoomByIdQuery request, CancellationToken cancellationToken)
  {
    RoomDto? room = await context.Rooms.AsQueryable()
      .Where(r => r.Id == request.Id)
      .Select(x => new RoomDto
      {
        Id = x.Id,
        FilmId = x.FilmId,
        ViewersCount = x.Viewers.Count,
        IsPrivate = !string.IsNullOrEmpty(x.Code),
        IsUserIn = x.Viewers.Any(v => v == request.UserId)
      })
      .FirstOrDefaultAsync(cancellationToken: cancellationToken);

    return room ?? throw new RoomNotFoundException(request.Id);
  }
}
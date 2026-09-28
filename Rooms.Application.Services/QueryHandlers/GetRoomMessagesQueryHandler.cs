using Common.Application.DTOs;
using MediatR;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Rooms.Application.Abstractions.DTOs;
using Rooms.Application.Abstractions.Exceptions;
using Rooms.Application.Abstractions.Queries;
using Rooms.Domain.Rooms.Exceptions;
using Rooms.Infrastructure.Storage.Context;
using Rooms.Infrastructure.Storage.Models.Messages;

namespace Rooms.Application.Services.QueryHandlers;

/// <summary>
/// Обработчик запроса для получения сообщений комнаты с фильмом.
/// </summary>
/// <param name="context">Контекст базы данных MongoDB для работы с коллекцией сообщений</param>
public class GetRoomMessagesQueryHandler(MongoDbContext context)
  : IRequestHandler<GetRoomMessagesQuery, CountResult<MessageDto>>
{
  /// <summary>
  /// Обработка запроса на получение сообщений в комнате.
  /// Поддерживает пагинацию по курсору (FromMessageId), 
  /// возвращая сообщения, созданные позже указанного.
  /// </summary>
  /// <param name="request">Запрос с параметрами фильтрации и пагинации</param>
  /// <param name="cancellationToken">Токен отмены</param>
  /// <returns>Результат, содержащий список сообщений и общее количество</returns>
  public async Task<CountResult<MessageDto>> Handle(GetRoomMessagesQuery request, CancellationToken cancellationToken)
  {
    var room = await context.Rooms
      .AsQueryable()
      .Where(r => r.Id == request.RoomId)
      .Select(r => new { hasAccess = r.Viewers.Any(v => v.Id == request.ViewerId) })
      .FirstOrDefaultAsync(cancellationToken);

    if (room == null) throw new RoomNotFoundException(request.RoomId);
    if (!room.hasAccess) throw new ActionNotAllowedException("GetMessages");

    IQueryable<MessageModel> query = context.Messages.AsQueryable()
      .Where(m => m.RoomId == request.RoomId);

    int count = await query.CountAsync(cancellationToken);
    if (count == 0) return CountResult<MessageDto>.NoValues();

    if (request.FromMessageId.HasValue)
    {
      DateTime fromMessageCreatedAt = await context.Messages.AsQueryable()
        .Where(m => m.Id == request.FromMessageId.Value)
        .Where(m => m.RoomId == request.RoomId)
        .Select(m => m.SentAt)
        .FirstOrDefaultAsync(cancellationToken);

      if (fromMessageCreatedAt != default)
        query = query.Where(m => m.SentAt < fromMessageCreatedAt);
    }

    List<MessageDto>? list = await query
      .OrderByDescending(m => m.SentAt)
      .Take(request.Count)
      .Select(m => new MessageDto
      {
        Id = m.Id,
        UserId = m.UserId,
        Text = m.Text,
        SentAt = m.SentAt
      })
      .ToListAsync(cancellationToken);

    return new CountResult<MessageDto>
    {
      List = list,
      TotalCount = count
    };
  }
}
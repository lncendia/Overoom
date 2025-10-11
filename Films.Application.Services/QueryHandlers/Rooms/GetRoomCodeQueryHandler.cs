using Films.Application.Abstractions.Exceptions;
using Films.Application.Abstractions.Queries.Rooms;
using Films.Domain.Rooms.Exceptions;
using Films.Infrastructure.Storage.Context;
using MediatR;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Films.Application.Services.QueryHandlers.Rooms;

/// <summary>
/// Обработчик запроса для получения кода доступа комнаты по идентификатору
/// </summary>
/// <param name="context">Контекст базы данных MongoDB</param>
public class GetRoomCodeQueryHandler(MongoDbContext context) : IRequestHandler<GetRoomCodeQuery, string?>
{
    /// <summary>
    /// Обрабатывает запрос на получение кода доступа комнаты
    /// </summary>
    /// <param name="request">Запрос, содержащий ID комнаты и ID пользователя</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Код доступа комнаты или null, если код не установлен</returns>
    /// <exception cref="RoomNotFoundException">Выбрасывается если комната не найдена</exception>
    /// <exception cref="ActionNotAllowedException">Выбрасывается если пользователь не является участником комнаты</exception>
    public async Task<string?> Handle(GetRoomCodeQuery request, CancellationToken cancellationToken)
    {
        // Получаем комнату из базы данных с проверкой кода доступа и членства пользователя
        var room = await context.Rooms.AsQueryable()
            .Where(r => r.Id == request.RoomId)
            .Select(x => new
            {
                x.Code,
                IsUserIn = x.Viewers.Any(v => v == request.UserId)
            })
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        // Проверяем существование комнаты
        if (room is null) 
            throw new RoomNotFoundException(request.RoomId);

        // Проверяем, что пользователь является участником комнаты
        if (!room.IsUserIn) 
            throw new ActionNotAllowedException(request.RoomId, "GetCode");

        // Возвращаем код доступа комнаты (может быть null)
        return room.Code;
    }
}
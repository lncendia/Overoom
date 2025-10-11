using MediatR;
using Rooms.Application.Abstractions.DTOs;
using Rooms.Application.Abstractions.Exceptions;
using Rooms.Application.Abstractions.Queries;
using Rooms.Domain.Repositories;
using Rooms.Domain.Rooms.Exceptions;

namespace Rooms.Application.Services.QueryHandlers;

/// <summary>
/// Обработчик запроса для получения комнаты по идентификатору
/// </summary>
/// <param name="unitOfWork">Единица работы</param>
public class GetRoomByIdQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetRoomByIdQuery, RoomDto>
{
    /// <summary>
    /// Обрабатывает запрос на получение комнаты по ID
    /// </summary>
    /// <param name="request">Запрос, содержащий идентификатор комнаты</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>DTO комнаты с указанным идентификатором</returns>
    /// <exception cref="RoomNotFoundException">Выбрасывается, если плейлист с указанным ID не найден</exception>
    public async Task<RoomDto> Handle(GetRoomByIdQuery request, CancellationToken cancellationToken)
    {
        // Получаем комнату по ID из репозитория
        var room = await unitOfWork.RoomRepository.Value.GetAsync(request.RoomId, cancellationToken);

        // Проверяем существование фильма
        if (room == null) throw new RoomNotFoundException(request.RoomId);

        if (!room.Viewers.ContainsKey(request.ViewerId)) throw new ActionNotAllowedException("GetRoom");

        // Возвращаем комнату
        return new RoomDto
        {
            Id = room.Id,
            OwnerId = room.Owner.Id,
            FilmId = room.FilmId,
            IsSerial = room.IsSerial,
            Viewers = room.Viewers.Values.Select(ViewerDto.Create).ToArray()
        };
    }
}
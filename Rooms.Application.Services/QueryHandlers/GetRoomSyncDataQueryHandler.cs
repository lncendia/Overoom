using MediatR;
using Rooms.Application.Abstractions.DTOs;
using Rooms.Application.Abstractions.Events.Player;
using Rooms.Application.Abstractions.Exceptions;
using Rooms.Application.Abstractions.Queries;
using Rooms.Domain.Repositories;

namespace Rooms.Application.Services.QueryHandlers;

/// <summary>
/// Обработчик запроса для получения синхронизированных данных комнаты
/// </summary>
/// <param name="unitOfWork">Единица работы для взаимодействия с базой данных</param>
public class GetRoomSyncDataQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetRoomSyncDataQuery, RoomSyncDto>
{
    /// <summary>
    /// Обрабатывает запрос на получение синхронизированных данных комнаты
    /// </summary>
    /// <param name="request">Запрос, содержащий идентификатор комнаты и данные для синхронизации</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>DTO с синхронизированными данными комнаты</returns>
    /// <exception cref="RoomNotFoundException">Выбрасывается, если комната с указанным ID не найдена</exception>
    public async Task<RoomSyncDto> Handle(GetRoomSyncDataQuery request, CancellationToken cancellationToken)
    {
        // Получаем комнату по ID из репозитория
        var room = await unitOfWork.RoomRepository.Value.GetAsync(request.Id, cancellationToken);

        // Проверяем существование комнаты
        if (room == null) throw new RoomNotFoundException(request.Id);

        // Определяем пользователя, чье состояние будет использоваться для синхронизации
        // Если владелец онлайн - используем его состояние, иначе состояние текущего зрителя
        var viewerToSync = room.Owner.Online
            ? room.Owner
            : room.Viewers[request.ViewerId];

        // Формируем DTO с синхронизированными данными комнаты
        return new RoomSyncDto
        {
            PauseEvent = new PauseEvent
            {
                Pause = viewerToSync.OnPause
            },
            SpeedEvent = new SpeedEvent
            {
                Speed = viewerToSync.Speed
            },
            TimeLineEvent = new TimeLineEvent
            {
                TimeLine = viewerToSync.TimeLine.Ticks
            },
            EpisodeEvent = room.IsSerial && viewerToSync is { Season: not null, Episode: not null }
                ? new EpisodeEvent
                {
                    Season = viewerToSync.Season!.Value,
                    Episode = viewerToSync.Episode!.Value
                }
                : null 
        };
    }
}
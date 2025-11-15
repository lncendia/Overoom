using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Rooms.Application.Abstractions.Commands;
using Rooms.Application.Abstractions.Exceptions;
using Rooms.Application.Abstractions.Queries;
using Rooms.Application.Abstractions.RoomEvents.Messages;
using Rooms.Application.Abstractions.RoomEvents.Room;
using Rooms.Domain.Rooms.Exceptions;
using Rooms.Infrastructure.Web.Metrics;
using Rooms.Infrastructure.Web.Rooms.Exceptions;

namespace Rooms.Infrastructure.Web.Rooms.Hubs;

/// <summary>
/// SignalR хаб для управления комнатами и взаимодействия между пользователями
/// </summary>
/// <param name="mediator">Экземпляр медиатора для обработки CQRS запросов</param>
[Authorize]
public class RoomHub(ISender mediator) : Hub
{
    /// <summary>
    /// Подключение пользователя к комнате
    /// </summary>
    /// <param name="roomId">Идентификатор комнаты</param>
    public async Task Connect(Guid roomId)
    {
        // Получаем идентификатор текущего пользователя
        var userId = GetUserId();

        // Устанавливаем статус "онлайн" для пользователя в комнате
        await mediator.Send(new SetOnlineCommand
        {
            RoomId = roomId,
            Online = true,
            ViewerId = userId
        });

        // Сохраняем идентификатор комнаты в контексте подключения
        Context.Items.Add("roomId", roomId);

        // Добавляем подключение к группе SignalR для комнаты
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId.ToString());

        // Отправляем информацию о подключении вызывающему клиенту
        await Clients.Caller.SendAsync("Event", new ConnectEvent());
        
        // Увеличение метрики
        RoomsConnectionMetrics.Increment();
    }

    /// <summary>
    /// Получить данные комнаты
    /// </summary>
    public async Task GetRoom()
    {
        // Получаем идентификаторы пользователя и комнаты из контекста подключения
        var userId = GetUserId();
        var roomId = GetRoomId();
        
        // Получаем информацию о комнате
        var room = await mediator.Send(new GetRoomByIdQuery { RoomId = roomId, ViewerId = userId });

        // Создаем событие
        var @event = new RoomEvent { Room = room };

        // Отправляем информацию о комнате вызывающему клиенту
        await Clients.Caller.SendAsync("Event", @event);
    }
    
    /// <summary>
    /// Подключение пользователя к комнате и синхронизация текущего состояния медиа-контента
    /// </summary>
    public async Task Sync()
    {
        // Получаем идентификаторы пользователя и комнаты из контекста подключения
        var userId = GetUserId();
        var roomId = GetRoomId();

        // Запрашиваем текущее синхронизированное состояние комнаты
        var data = await mediator.Send(new GetRoomSyncDataQuery { Id = roomId, ViewerId = userId });

        // Последовательно отправляем все события состояния для синхронизации клиента

        // Отправляем событие текущего эпизода (только для сериалов)
        if (data.EpisodeEvent != null)
            await Clients.Caller.SendAsync("Event", data.EpisodeEvent);
        
        // Отправляем событие состояния паузы/воспроизведения
        await Clients.Caller.SendAsync("Event", data.PauseEvent);

        // Отправляем событие текущей позиции воспроизведения (таймкод)
        await Clients.Caller.SendAsync("Event", data.TimeLineEvent);

        // Отправляем событие скорости воспроизведения
        await Clients.Caller.SendAsync("Event", data.SpeedEvent);
    }

    /// <summary>
    /// Получение сообщений комнаты
    /// </summary>
    /// <param name="fromMessageId">Идентификатор сообщения, начиная с которого нужно получить сообщения (опционально)</param>
    /// <param name="count">Количество сообщений для получения (опционально, по умолчанию 20, максимум 50)</param>
    public async Task GetMessages(Guid? fromMessageId, int? count)
    {
        // Устанавливаем значения по умолчанию для количества сообщений
        if (count is null or < 0) count = 20;
        else if (count > 50) count = 50;

        // Получаем идентификаторы пользователя и комнаты
        var userId = GetUserId();
        var roomId = GetRoomId();

        // Получаем сообщения из базы данных
        var messages = await mediator.Send(new GetRoomMessagesQuery
        {
            RoomId = roomId,
            FromMessageId = fromMessageId,
            Count = count.Value,
            ViewerId = userId
        });

        // Создаем событие
        var @event = new MessagesEvent { Messages = messages };

        // Отправляем сообщения вызывающему клиенту
        await Clients.Caller.SendAsync("Event", @event);
    }

    /// <summary>
    /// Изменение текущей серии в комнате
    /// </summary>
    /// <param name="season">Номер сезона</param>
    /// <param name="episode">Номер серии</param>
    public async Task SetEpisode(int season, int episode)
    {
        // Получаем идентификаторы пользователя и комнаты
        var userId = GetUserId();
        var roomId = GetRoomId();

        // Отправляем команду на изменение серии
        await mediator.Send(new SetEpisodeCommand
        {
            Season = season,
            Episode = episode,
            ViewerId = userId,
            RoomId = roomId
        });
    }

    /// <summary>
    /// Уведомление о наборе сообщения пользователем
    /// </summary>
    public async Task Type()
    {
        // Получаем идентификаторы пользователя и комнаты
        var userId = GetUserId();
        var roomId = GetRoomId();

        // Отправляем команду о наборе сообщения
        await mediator.Send(new TypingCommand
        {
            ViewerId = userId,
            RoomId = roomId
        });
    }

    /// <summary>
    /// Отправка сообщения в комнату
    /// </summary>
    /// <param name="text">Текст сообщения</param>
    public async Task SendMessage(string text)
    {
        // Получаем идентификаторы пользователя и комнаты
        var userId = GetUserId();
        var roomId = GetRoomId();

        // Отправляем сообщение через медиатор
        await mediator.Send(new SendMessageCommand
        {
            Message = text,
            ViewerId = userId,
            RoomId = roomId,
            ConnectionId = Context.ConnectionId
        });
    }

    /// <summary>
    /// Установка текущей позиции воспроизведения
    /// </summary>
    /// <param name="ticks">Позиция</param>
    public async Task SetTimeLine(long ticks)
    {
        // Получаем идентификаторы пользователя и комнаты
        var userId = GetUserId();
        var roomId = GetRoomId();

        // Отправляем команду на установку позиции воспроизведения
        await mediator.Send(new SetTimeLineCommand
        {
            TimeLine = TimeSpan.FromTicks(ticks),
            ViewerId = userId,
            RoomId = roomId
        });
    }

    /// <summary>
    /// Установка состояния паузы
    /// </summary>
    /// <param name="pause">Флаг паузы (true - поставить на паузу)</param>
    /// <param name="ticks">Текущая позиция воспроизведения</param>
    /// <param name="buffering">Флаг, что пауза вызвана дозагрузкой контента</param>
    public async Task SetPause(bool pause, long ticks, bool buffering)
    {
        // Получаем идентификаторы пользователя и комнаты
        var userId = GetUserId();
        var roomId = GetRoomId();

        // Отправляем команду на установку паузы
        await mediator.Send(new SetPauseCommand
        {
            TimeLine = TimeSpan.FromTicks(ticks),
            Pause = pause,
            ViewerId = userId,
            RoomId = roomId,
            Buffering = buffering
        });
    }

    /// <summary>
    /// Установка скорости воспроизведения
    /// </summary>
    /// <param name="speed">Скорость воспроизведения (1.0 - нормальная скорость)</param>
    public async Task SetSpeed(double speed)
    {
        // Получаем идентификаторы пользователя и комнаты
        var userId = GetUserId();
        var roomId = GetRoomId();

        // Отправляем команду на установку скорости воспроизведения
        await mediator.Send(new SetSpeedCommand
        {
            ViewerId = userId,
            RoomId = roomId,
            Speed = speed
        });
    }

    /// <summary>
    /// Установка уровня громкости
    /// </summary>
    /// <param name="muted">Уровень громкости (0-100)</param>
    public async Task SetMuted(bool muted)
    {
        // Получаем идентификаторы пользователя и комнаты
        var userId = GetUserId();
        var roomId = GetRoomId();

        // Отправляем команду на установку громкости
        await mediator.Send(new SetVolumeCommand
        {
            ViewerId = userId,
            RoomId = roomId,
            Muted = muted
        });
    }

    /// <summary>
    /// Установка полноэкранного режима
    /// </summary>
    /// <param name="fullScreen">Флаг полноэкранного режима</param>
    public async Task SetFullScreen(bool fullScreen)
    {
        // Получаем идентификаторы пользователя и комнаты
        var userId = GetUserId();
        var roomId = GetRoomId();

        // Отправляем команду на установку полноэкранного режима
        await mediator.Send(new SetFullscreenCommand
        {
            Fullscreen = fullScreen,
            ViewerId = userId,
            RoomId = roomId
        });
    }

    /// <summary>
    /// Отправка звукового сигнала (бип) другому пользователю
    /// </summary>
    /// <param name="target">Идентификатор целевого пользователя</param>
    public async Task Beep(Guid target)
    {
        // Проверяем кулдаун на действия
        Action();

        // Получаем идентификаторы пользователя и комнаты
        var userId = GetUserId();
        var roomId = GetRoomId();

        // Отправляем команду на отправку бипа
        await mediator.Send(new BeepCommand
        {
            ViewerId = userId,
            RoomId = roomId,
            TargetId = target
        });
    }

    /// <summary>
    /// Отправка сигнала "крик" другому пользователю
    /// </summary>
    /// <param name="target">Идентификатор целевого пользователя</param>
    public async Task Scream(Guid target)
    {
        // Проверяем кулдаун на действия
        Action();

        // Получаем идентификаторы пользователя и комнаты
        var userId = GetUserId();
        var roomId = GetRoomId();

        // Отправляем команду на отправку крика
        await mediator.Send(new ScreamCommand
        {
            ViewerId = userId,
            RoomId = roomId,
            TargetId = target
        });
    }

    /// <summary>
    /// Обработчик отключения пользователя
    /// </summary>
    /// <param name="exception">Исключение, если отключение было вызвано ошибкой</param>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // Получаем идентификаторы пользователя и комнаты
        var userId = GetUserId();
        var roomId = GetRoomId();

        try
        {
            // Устанавливаем статус "оффлайн" для пользователя
            await mediator.Send(new SetOnlineCommand
            {
                Online = false,
                RoomId = roomId,
                ViewerId = userId
            });
        }
        catch (RoomNotFoundException)
        {
            // Игнорируем, так как комната может быть удалена
        }
        catch (ViewerNotFoundException)
        {
            // Игнорируем, так как пользователь может быть исключен / сам вышел
        }

        // Вызываем базовую реализацию метода
        await base.OnDisconnectedAsync(exception);
        
        // Уменьшение метрики
        RoomsConnectionMetrics.Decrement();
    }

    /// <summary>
    /// Получение идентификатора текущего пользователя из контекста
    /// </summary>
    /// <returns>Идентификатор пользователя</returns>
    /// <exception cref="InvalidOperationException">Если идентификатор не найден</exception>
    private Guid GetUserId()
    {
        var id = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new InvalidOperationException();
        return Guid.Parse(id);
    }

    /// <summary>
    /// Получение идентификатора комнаты из контекста подключения
    /// </summary>
    /// <returns>Идентификатор комнаты</returns>
    /// <exception cref="InvalidOperationException">Если идентификатор комнаты не найден</exception>
    private Guid GetRoomId()
    {
        return (Guid)(Context.Items["roomId"] ?? throw new InvalidOperationException());
    }

    /// <summary>
    /// Проверка кулдауна между действиями пользователя
    /// </summary>
    /// <exception cref="ActionCooldownException">Если действие выполняется слишком часто</exception>
    private void Action()
    {
        // Получаем время последнего действия
        var date = (DateTime?)Context.Items["LastActionTime"] ?? DateTime.MinValue;

        // Получаем текущее время
        var now = DateTime.Now;

        // Вычисляем разницу между текущим временем и временем последнего действия
        var difference = now - date;

        // Проверяем, что с последнего действия прошла хотя бы минута
        if (difference.TotalSeconds < 30) throw new ActionCooldownException(30 - difference.Seconds);

        // Обновляем время последнего действия
        Context.Items["LastActionTime"] = now;
    }
}
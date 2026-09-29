using System.Security.Claims;

using Common.Application.DTOs;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Rooms.Application.Abstractions.Commands;
using Rooms.Application.Abstractions.DTOs;
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
    Guid userId = GetUserId();

    await mediator.Send(new SetOnlineCommand
    {
      RoomId = roomId,
      Online = true,
      ViewerId = userId
    });

    bool alreadyConnected = Context.Items.ContainsKey("roomId");
    Context.Items["roomId"] = roomId;
    await Groups.AddToGroupAsync(Context.ConnectionId, roomId.ToString());
    await Clients.Caller.SendAsync("Event", new ConnectEvent());
    if (!alreadyConnected) RoomsConnectionMetrics.Increment();
  }

  /// <summary>
  /// Получить данные комнаты
  /// </summary>
  public async Task GetRoom()
  {
    Guid userId = GetUserId();
    Guid roomId = GetRoomId();
    RoomDto room = await mediator.Send(new GetRoomByIdQuery { RoomId = roomId, ViewerId = userId });
    var @event = new RoomEvent { Room = room };
    await Clients.Caller.SendAsync("Event", @event);
  }

  /// <summary>
  /// Подключение пользователя к комнате и синхронизация текущего состояния медиа-контента
  /// </summary>
  public async Task Sync()
  {
    Guid userId = GetUserId();
    Guid roomId = GetRoomId();
    RoomSyncDto data = await mediator.Send(new GetRoomSyncDataQuery { Id = roomId, ViewerId = userId });

    if (data.EpisodeEvent != null)
      await Clients.Caller.SendAsync("Event", data.EpisodeEvent);

    await Clients.Caller.SendAsync("Event", data.PauseEvent);
    await Clients.Caller.SendAsync("Event", data.TimeLineEvent);
    await Clients.Caller.SendAsync("Event", data.SpeedEvent);
  }

  /// <summary>
  /// Получение сообщений комнаты
  /// </summary>
  /// <param name="fromMessageId">Идентификатор сообщения, начиная с которого нужно получить сообщения (опционально)</param>
  /// <param name="count">Количество сообщений для получения (опционально, по умолчанию 20, максимум 50)</param>
  public async Task GetMessages(Guid? fromMessageId, int? count)
  {
    if (count is null or < 0) count = 20;
    else if (count > 50) count = 50;

    Guid userId = GetUserId();
    Guid roomId = GetRoomId();

    CountResult<MessageDto> messages = await mediator.Send(new GetRoomMessagesQuery
    {
      RoomId = roomId,
      FromMessageId = fromMessageId,
      Count = count.Value,
      ViewerId = userId
    });

    var @event = new MessagesEvent { Messages = messages };
    await Clients.Caller.SendAsync("Event", @event);
  }

  /// <summary>
  /// Изменение текущей серии в комнате
  /// </summary>
  /// <param name="season">Номер сезона</param>
  /// <param name="episode">Номер серии</param>
  public async Task SetEpisode(int season, int episode)
  {
    Guid userId = GetUserId();
    Guid roomId = GetRoomId();

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
    Guid userId = GetUserId();
    Guid roomId = GetRoomId();

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
    Guid userId = GetUserId();
    Guid roomId = GetRoomId();

    await mediator.Send(new SendMessageCommand
    {
      Message = text,
      ViewerId = userId,
      RoomId = roomId,
      ConnectionId = Context.ConnectionId
    });
  }

  /// <summary>
  /// Установка или снятие реакции на сообщение
  /// </summary>
  /// <param name="messageId">Идентификатор сообщения</param>
  /// <param name="reaction">Код реакции</param>
  public async Task ToggleReaction(Guid messageId, string reaction)
  {
    Guid userId = GetUserId();
    Guid roomId = GetRoomId();

    await mediator.Send(new ToggleReactionCommand
    {
      ViewerId = userId,
      RoomId = roomId,
      MessageId = messageId,
      Reaction = reaction
    });
  }

  /// <summary>
  /// Установка текущей позиции воспроизведения
  /// </summary>
  /// <param name="ticks">Позиция</param>
  public async Task SetTimeLine(long ticks)
  {
    Guid userId = GetUserId();
    Guid roomId = GetRoomId();

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
    Guid userId = GetUserId();
    Guid roomId = GetRoomId();

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
    Guid userId = GetUserId();
    Guid roomId = GetRoomId();

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
    Guid userId = GetUserId();
    Guid roomId = GetRoomId();

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
    Guid userId = GetUserId();
    Guid roomId = GetRoomId();

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
    Action();
    Guid userId = GetUserId();
    Guid roomId = GetRoomId();

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
    Action();
    Guid userId = GetUserId();
    Guid roomId = GetRoomId();

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
    if (!TryGetRoomId(out Guid roomId))
    {
      await base.OnDisconnectedAsync(exception);
      return;
    }

    Guid userId = GetUserId();

    try
    {
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

    await base.OnDisconnectedAsync(exception);
    RoomsConnectionMetrics.Decrement();
  }

  /// <summary>
  /// Получение идентификатора текущего пользователя из контекста
  /// </summary>
  /// <returns>Идентификатор пользователя</returns>
  /// <exception cref="InvalidOperationException">Если идентификатор не найден</exception>
  private Guid GetUserId()
  {
    string id = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new InvalidOperationException();
    return Guid.Parse(id);
  }

  /// <summary>
  /// Получение идентификатора комнаты из контекста подключения
  /// </summary>
  /// <returns>Идентификатор комнаты</returns>
  /// <exception cref="InvalidOperationException">Если идентификатор комнаты не найден</exception>
  private Guid GetRoomId()
  {
    return TryGetRoomId(out Guid roomId) ? roomId : throw new InvalidOperationException();
  }

  /// <summary>
  /// Получение идентификатора комнаты, если соединение уже прошло Connect
  /// </summary>
  private bool TryGetRoomId(out Guid roomId)
  {
    if (Context.Items.TryGetValue("roomId", out object? value) && value is Guid id)
    {
      roomId = id;
      return true;
    }

    roomId = Guid.Empty;
    return false;
  }

  /// <summary>
  /// Проверка кулдауна между действиями пользователя
  /// </summary>
  /// <exception cref="ActionCooldownException">Если действие выполняется слишком часто</exception>
  private void Action()
  {
    DateTime date = (DateTime?)Context.Items["LastActionTime"] ?? DateTime.MinValue;
    DateTime now = DateTime.Now;
    TimeSpan difference = now - date;
    if (difference.TotalSeconds < 30) throw new ActionCooldownException(30 - difference.Seconds);

    Context.Items["LastActionTime"] = now;
  }
}
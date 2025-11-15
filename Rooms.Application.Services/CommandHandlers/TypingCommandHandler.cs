using Common.Application.ScopedDictionary;
using MediatR;
using Rooms.Application.Abstractions;
using Rooms.Application.Abstractions.Commands;
using Rooms.Application.Abstractions.Exceptions;
using Rooms.Application.Abstractions.RoomEvents.Messages;
using Rooms.Application.Abstractions.Services;

namespace Rooms.Application.Services.CommandHandlers;

/// <summary>
/// Обработчик команды на отправку уведомления о наборе сообщения пользователем
/// </summary>
/// <param name="eventSender">Отправитель событий комнаты</param>
/// <param name="context">Контекст выполняемой области с данными текущего соединения</param>
public class TypingCommandHandler(IRoomEventSender eventSender, IScopedContext context) : IRequestHandler<TypingCommand>
{
    /// <summary>
    /// Обрабатывает команду уведомления о наборе текста пользователем
    /// </summary>
    /// <param name="request">Команда с данными о пользователе и комнате</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <exception cref="RoomNotFoundException">Если комната не найдена</exception>
    /// <exception cref="InvalidOperationException">Если данные контекста невалидны</exception>
    public async Task Handle(TypingCommand request, CancellationToken cancellationToken)
    {
        // Получаем идентификатор текущего соединения из контекста области
        var excludedConnectionId = context.Current.Get<string>(Constants.ScopedDictionary.CurrentConnectionIdKey);
        
        // Публикуем событие о наборе текста в комнату через MassTransit
        await eventSender.SendAsync(new TypingEvent { Initiator = request.ViewerId }, request.RoomId, excludedConnectionId, cancellationToken);
    }
}
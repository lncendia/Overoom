using Common.Application.ScopedDictionary;
using MassTransit;
using MediatR;
using Rooms.Application.Abstractions;
using Rooms.Application.Abstractions.Commands;
using Rooms.Application.Abstractions.Events;
using Rooms.Application.Abstractions.Events.Messages;
using Rooms.Application.Abstractions.Exceptions;

namespace Rooms.Application.Services.CommandHandlers;

/// <summary>
/// Обработчик команды на отправку уведомления о наборе сообщения пользователем
/// </summary>
/// <param name="publish">Сервис для публикации интеграционных событий через MassTransit</param>
/// <param name="context">Контекст выполняемой области с данными текущего соединения</param>
public class TypingCommandHandler(IPublishEndpoint publish, IScopedContext context) : IRequestHandler<TypingCommand>
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
        var connectionId = context.Current.Get<string>(Constants.ScopedDictionary.CurrentConnectionIdKey);
        
        // Создаем новую область контекста для изоляции данных
        using var _ = context.CreateScope();
        
        // Устанавливаем заголовки комнаты в контексте для последующей обработки
        context.Current.SetRoomHeaders(request.RoomId, connectionId);
        
        // Публикуем событие о наборе текста в комнату через MassTransit
        await publish.Publish<RoomBaseEvent>(new TypingEvent { Initiator = request.ViewerId }, cancellationToken);
    }
}
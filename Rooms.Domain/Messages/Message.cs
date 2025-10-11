using Common.Domain.Aggregates;
using Common.Domain.Extensions;
using Rooms.Domain.Messages.Events;
using Rooms.Domain.Rooms;
using Rooms.Domain.Rooms.Exceptions;

namespace Rooms.Domain.Messages;

/// <summary>
/// Представляет текстовое сообщение, отправленное пользователем в комнате.
/// Является агрегатом в доменной модели.
/// </summary>
public partial class Message : AggregateRoot
{
    private const int MaxTextLength = 1000;

    /// <summary>
    /// Создаёт новое сообщение от пользователя внутри заданной комнаты.
    /// Проверяет наличие пользователя в комнате и валидирует длину текста.
    /// </summary>
    /// <param name="room">Комната, в которую отправляется сообщение.</param>
    /// <param name="userId">Идентификатор пользователя, отправившего сообщение.</param>
    /// <param name="text">Текст сообщения.</param>
    /// <exception cref="ViewerNotFoundException">
    /// Бросается, если пользователь не найден среди участников комнаты.
    /// </exception>
    public Message(Room room, Guid userId, string text) : base(Guid.NewGuid())
    {
        if (!room.Viewers.ContainsKey(userId)) throw new ViewerNotFoundException();

        Text = text
            .Replace(Environment.NewLine, " ") // Удаление переводов строк
            .ValidateLength(nameof(MaxTextLength), MaxTextLength); // Ограничение по длине

        UserId = userId;
        RoomId = room.Id;
        
        AddDomainEvent(new NewMessageEvent
        {
            Room = room,
            Viewer = room.Viewers[userId],
            Message = this
        });
    }

    /// <summary>
    /// Идентификатор комнаты, к которой относится сообщение.
    /// </summary>
    public Guid RoomId { get; }

    /// <summary>
    /// Идентификатор пользователя, отправившего сообщение.
    /// </summary>
    public Guid UserId { get; }

    /// <summary>
    /// Время создания сообщения (UTC).
    /// </summary>
    public DateTime SentAt { get; } = DateTime.UtcNow;

    /// <summary>
    /// Текст сообщения (максимум 1000 символов, без переводов строк).
    /// </summary>
    public string Text { get; }
}

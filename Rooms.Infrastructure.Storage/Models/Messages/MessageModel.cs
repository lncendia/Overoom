using MongoTracker.Entities;
using Rooms.Domain.Messages.Snapshots;

namespace Rooms.Infrastructure.Storage.Models.Messages;

/// <summary>
/// Модель комментария для работы с базой данных.
/// Наследуется от абстрактного класса UpdatedEntity,
/// который предоставляет возможности отслеживания изменений и управления состоянием сущности.
/// </summary>
public class MessageModel : VersionedUpdatedEntity<MessageModel>
{
    private string _text = null!;
    private DateTime _sentAt;
    private Guid _userId;
    private Guid _roomId;

    /// <summary>
    /// Уникальный идентификатор комментария
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Текст комментария (максимальная длина - 1000 символов)
    /// </summary>
    public string Text
    {
        get => _text;
        set => _text = TrackChange(nameof(Text), _text, value)!;
    }

    /// <summary>
    /// Дата и время создания комментария
    /// </summary>
    public DateTime SentAt
    {
        get => _sentAt;
        set => _sentAt = TrackStructChange(nameof(SentAt), _sentAt, value);
    }

    /// <summary>
    /// Идентификатор пользователя, оставившего комментарий (может быть null)
    /// </summary>
    public Guid UserId
    {
        get => _userId;
        set => _userId = TrackStructChange(nameof(UserId), _userId, value);
    }

    /// <summary>
    /// Идентификатор комнаты, к которой относится сообщение
    /// </summary>
    public Guid RoomId
    {
        get => _roomId;
        set => _roomId = TrackStructChange(nameof(RoomId), _roomId, value);
    }
    
    /// <summary>
    /// Создаёт снапшот текущего состояния модели для хранения или передачи.
    /// </summary>
    public MessageSnapshot GetSnapshot() => new()
    {
        Id = Id,
        RoomId = RoomId,
        UserId = UserId,
        Text = Text,
        SentAt = SentAt
    };

    /// <summary>
    /// Обновляет модель на основе снапшота.
    /// Поля обновляются с отслеживанием изменений через TrackChange/TrackStructChange.
    /// </summary>
    public void UpdateFromSnapshot(MessageSnapshot snapshot)
    {
        RoomId = snapshot.RoomId;
        UserId = snapshot.UserId;
        Text = snapshot.Text;
        SentAt = snapshot.SentAt;
    }
}
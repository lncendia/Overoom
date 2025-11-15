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
    /// <summary>
    /// Уникальный идентификатор комментария
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Текст комментария (максимальная длина - 1000 символов)
    /// </summary>
    public string Text
    {
        get;
        set => field = TrackChange(nameof(Text), field, value)!;
    } = null!;

    /// <summary>
    /// Дата и время создания комментария
    /// </summary>
    public DateTime SentAt
    {
        get;
        set => field = TrackStructChange(nameof(SentAt), field, value);
    }

    /// <summary>
    /// Идентификатор пользователя, оставившего комментарий (может быть null)
    /// </summary>
    public Guid UserId
    {
        get;
        set => field = TrackStructChange(nameof(UserId), field, value);
    }

    /// <summary>
    /// Идентификатор комнаты, к которой относится сообщение
    /// </summary>
    public Guid RoomId
    {
        get;
        set => field = TrackStructChange(nameof(RoomId), field, value);
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
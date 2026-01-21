using Rooms.Domain.Messages.Snapshots;

namespace Rooms.Infrastructure.Storage.Models.Messages;

/// <summary>
/// Модель комментария для работы с базой данных.
/// </summary>
public class MessageModel
{
  /// <summary>
  /// Уникальный идентификатор комментария
  /// </summary>
  public required Guid Id { get; init; }

  /// <summary>
  /// Текст комментария
  /// </summary>
  public required string Text { get; set; }

  /// <summary>
  /// Дата и время создания комментария
  /// </summary>
  public DateTime SentAt { get; set; }

  /// <summary>
  /// Идентификатор пользователя, оставившего комментарий (может быть null)
  /// </summary>
  public Guid UserId { get; set; }

  /// <summary>
  /// Идентификатор комнаты, к которой относится сообщение
  /// </summary>
  public Guid RoomId { get; set; }

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
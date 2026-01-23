using Common.Infrastructure.Repositories.Models;

using Rooms.Domain.Messages.Snapshots;

namespace Rooms.Infrastructure.Storage.Models.Messages;

/// <summary>
/// Модель комментария для работы с базой данных.
/// </summary>
public class MessageModel : IModel<MessageSnapshot>
{
  #region Поля и свойства

  /// <summary>
  /// Текст комментария
  /// </summary>
  public string Text { get; set; } = null!;

  /// <summary>
  /// Дата и время создания комментария
  /// </summary>
  public DateTime SentAt { get; set; }

  /// <summary>
  /// Идентификатор пользователя, оставившего комментарий
  /// </summary>
  public Guid UserId { get; set; }

  /// <summary>
  /// Идентификатор комнаты, к которой относится сообщение
  /// </summary>
  public Guid RoomId { get; set; }

  #endregion

  #region IModel

  /// <summary>
  /// Уникальный идентификатор комментария
  /// </summary>
  public required Guid Id { get; init; }

  /// <summary>
  /// Создаёт снапшот текущего состояния модели для хранения или передачи.
  /// </summary>
  public MessageSnapshot GetSnapshot()
  {
    return new MessageSnapshot
    {
      Id = Id,
      RoomId = RoomId,
      UserId = UserId,
      Text = Text,
      SentAt = SentAt
    };
  }

  /// <summary>
  /// Обновляет модель на основе снапшота.
  /// </summary>
  public void UpdateFromSnapshot(MessageSnapshot snapshot)
  {
    RoomId = snapshot.RoomId;
    UserId = snapshot.UserId;
    Text = snapshot.Text;
    SentAt = snapshot.SentAt;
  }

  #endregion
}

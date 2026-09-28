using Common.Domain.Aggregates;
using Rooms.Domain.Messages.Snapshots;

namespace Rooms.Domain.Messages;

/// <summary>
/// Представляет текстовое сообщение в комнате.
/// </summary>
public partial class Message : ISnapshotable<Message, MessageSnapshot>
{
  /// <summary>
  /// Конструктор для восстановления из снапшота.
  /// </summary>
  private Message(MessageSnapshot snapshot) : base(snapshot.Id)
  {
    RoomId = snapshot.RoomId;
    UserId = snapshot.UserId;
    Text = snapshot.Text;
    SentAt = snapshot.SentAt;
  }

  /// <inheritdoc/>
  MessageSnapshot ISnapshotable<Message, MessageSnapshot>.ToSnapshot()
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

  /// <inheritdoc/>
  static Message ISnapshotable<Message, MessageSnapshot>.Restore(MessageSnapshot snapshot)
  {
    return new Message(snapshot);
  }
}

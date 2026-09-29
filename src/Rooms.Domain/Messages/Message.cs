using Common.Domain.Aggregates;
using Common.Domain.Extensions;

using Rooms.Domain.Messages.Events;
using Rooms.Domain.Messages.Exceptions;
using Rooms.Domain.Messages.Snapshots;
using Rooms.Domain.Messages.ValueObjects;
using Rooms.Domain.Rooms;
using Rooms.Domain.Rooms.Exceptions;

namespace Rooms.Domain.Messages;

/// <summary>
/// Представляет текстовое сообщение, отправленное пользователем в комнате.
/// </summary>
public partial class Message : AggregateRoot
{
  #region Константы

  private const int MaxTextLength = 1000;

  #endregion

  #region Поля и свойства

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

  /// <summary>
  /// Реакции зрителей на сообщение.
  /// </summary>
  private readonly HashSet<MessageReaction> _reactions = [];

  /// <summary>
  /// Реакции зрителей на сообщение.
  /// </summary>
  public IReadOnlySet<MessageReaction> Reactions => _reactions;

  #endregion

  #region Методы

  /// <summary>
  /// Ставит реакцию зрителя на сообщение или снимает её, если она уже стоит.
  /// </summary>
  /// <param name="room">Комната, в которой находится сообщение</param>
  /// <param name="viewerId">Идентификатор зрителя</param>
  /// <param name="reaction">Код реакции (см. <see cref="MessageReactions"/>)</param>
  /// <exception cref="ActionNotAllowedException">Если сообщение не принадлежит комнате</exception>
  /// <exception cref="ViewerNotFoundException">Если зритель не находится в комнате</exception>
  /// <exception cref="UnknownReactionException">Если реакция не входит в число допустимых</exception>
  public void ToggleReaction(Room room, Guid viewerId, string reaction)
  {
    if (room.Id != RoomId) throw new ActionNotAllowedException(nameof(ToggleReaction));
    if (!room.Viewers.ContainsKey(viewerId)) throw new ViewerNotFoundException();
    if (!MessageReactions.All.Contains(reaction)) throw new UnknownReactionException(reaction);

    var messageReaction = new MessageReaction { UserId = viewerId, Reaction = reaction };
    bool added = _reactions.Add(messageReaction);
    if (!added) _reactions.Remove(messageReaction);

    AddDomainEvent(new MessageReactionToggledEvent
    {
      Room = room,
      Message = this,
      ViewerId = viewerId,
      Reaction = reaction,
      Added = added
    });
  }

  #endregion

  #region Конструкторы

  /// <summary>
  /// Конструктор.
  /// </summary>
  /// <param name="room">Комната, в которую отправляется сообщение.</param>
  /// <param name="userId">Идентификатор пользователя, отправившего сообщение.</param>
  /// <param name="text">Текст сообщения.</param>
  /// <exception cref="ViewerNotFoundException"> Если пользователь не найден среди участников комнаты.</exception>
  public Message(Room room, Guid userId, string text) : base(Guid.NewGuid())
  {
    if (!room.Viewers.ContainsKey(userId)) throw new ViewerNotFoundException();

    Text = text
      .ReplaceLineEndings(" ")
      .ValidateLength(nameof(MaxTextLength), MaxTextLength);

    UserId = userId;
    RoomId = room.Id;

    AddDomainEvent(new NewMessageEvent
    {
      Room = room,
      Viewer = room.Viewers[userId],
      Message = this
    });
  }

  #endregion
}

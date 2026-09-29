using Common.Application.Events;
using Common.Infrastructure.Transactions;

using Films.Domain.CommentReactions.Events;
using Films.Infrastructure.Storage.Context;

namespace Films.Infrastructure.Storage.EventHandlers;

/// <summary>
/// Уменьшает денормализованный счётчик реакции в документе комментария при снятии реакции.
/// </summary>
/// <param name="context">MongoDB-контекст приложения</param>
/// <param name="transaction">Текущая транзакция области</param>
public class CommentReactionRemovedCountersEventHandler(MongoDbContext context, ITransactionContext transaction)
  : BeforeSaveNotificationHandler<CommentReactionRemovedEvent>
{
  /// <inheritdoc/>
  protected override Task Execute(CommentReactionRemovedEvent notification, CancellationToken cancellationToken)
  {
    return context.IncrementReactionCountAsync(transaction, notification.Reaction, -1, cancellationToken);
  }
}

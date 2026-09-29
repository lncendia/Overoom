using Common.Application.Events;
using Common.Domain.Events;
using Common.Infrastructure.Transactions;

using Films.Domain.CommentReactions;
using Films.Infrastructure.Storage.Context;
using Films.Infrastructure.Storage.Models.Comments;

using MongoDB.Driver;

namespace Films.Infrastructure.Storage.EventHandlers;

/// <summary>
/// Увеличивает денормализованный счётчик реакции в документе комментария при установке реакции.
/// </summary>
/// <remarks>
/// Выполняется до сохранения, поэтому внутри транзакции точки входа $inc фиксируется или откатывается
/// вместе с самой реакцией.
/// </remarks>
/// <param name="context">MongoDB-контекст приложения</param>
/// <param name="transaction">Текущая транзакция области</param>
public class CommentReactionCreatedCountersEventHandler(MongoDbContext context, ITransactionContext transaction)
  : BeforeSaveNotificationHandler<CreateEvent<CommentReaction>>
{
  /// <inheritdoc/>
  protected override Task Execute(CreateEvent<CommentReaction> notification, CancellationToken cancellationToken)
  {
    return context.IncrementReactionCountAsync(transaction, notification.Aggregate, 1, cancellationToken);
  }
}

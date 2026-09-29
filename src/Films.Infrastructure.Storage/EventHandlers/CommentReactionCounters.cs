using Common.Infrastructure.Transactions;

using Films.Domain.CommentReactions;
using Films.Infrastructure.Storage.Context;
using Films.Infrastructure.Storage.Models.Comments;

using MongoDB.Driver;

namespace Films.Infrastructure.Storage.EventHandlers;

/// <summary>
/// Операции над денормализованными счётчиками реакций комментариев.
/// </summary>
internal static class CommentReactionCounters
{
  /// <summary>
  /// Изменяет счётчик реакции в документе комментария через $inc в сессии текущей транзакции
  /// </summary>
  /// <param name="context">MongoDB-контекст приложения</param>
  /// <param name="transaction">Текущая транзакция области</param>
  /// <param name="reaction">Реакция</param>
  /// <param name="delta">Изменение счётчика</param>
  /// <param name="cancellationToken">Токен отмены операции</param>
  public static Task IncrementReactionCountAsync(this MongoDbContext context, ITransactionContext transaction,
    CommentReaction reaction, int delta, CancellationToken cancellationToken)
  {
    FilterDefinition<CommentModel> filter = Builders<CommentModel>.Filter.Eq(c => c.Id, reaction.CommentId);

    UpdateDefinition<CommentModel> update = Builders<CommentModel>.Update
      .Inc($"{nameof(CommentModel.ReactionCounts)}.{reaction.Reaction}", delta);

    return transaction.Session == null
      ? context.Comments.UpdateOneAsync(filter, update, cancellationToken: cancellationToken)
      : context.Comments.UpdateOneAsync(transaction.Session, filter, update, cancellationToken: cancellationToken);
  }
}

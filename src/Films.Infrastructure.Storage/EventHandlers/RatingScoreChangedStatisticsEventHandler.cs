using Common.Application.Events;
using Common.Infrastructure.Transactions;

using Films.Domain.Ratings.Events;
using Films.Infrastructure.Storage.Context;
using Films.Infrastructure.Storage.Models.Films;

using MongoDB.Driver;

namespace Films.Infrastructure.Storage.EventHandlers;

/// <summary>
/// Обновляет сумму оценок фильма при изменении пользователем своей оценки.
/// </summary>
/// <param name="context">MongoDB-контекст приложения</param>
/// <param name="transaction">Текущая транзакция области</param>
public class RatingScoreChangedStatisticsEventHandler(MongoDbContext context, ITransactionContext transaction)
  : BeforeSaveNotificationHandler<RatingScoreChangedEvent>
{
  /// <inheritdoc/>
  protected override Task Execute(RatingScoreChangedEvent notification, CancellationToken cancellationToken)
  {
    FilterDefinition<FilmModel> filter = Builders<FilmModel>.Filter.Eq(f => f.Id, notification.Rating.FilmId);

    UpdateDefinition<FilmModel> update = Builders<FilmModel>.Update
      .Inc(f => f.UserRatingsSum, notification.Rating.Score - notification.OldScore);

    return transaction.Session == null
      ? context.Films.UpdateOneAsync(filter, update, cancellationToken: cancellationToken)
      : context.Films.UpdateOneAsync(transaction.Session, filter, update, cancellationToken: cancellationToken);
  }
}

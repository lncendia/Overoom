using Rooms.Application.Abstractions.Services;
using Rooms.Infrastructure.Storage.Context;
using Rooms.Infrastructure.Storage.Models.Statistics;

using MongoDB.Driver;

namespace Rooms.Infrastructure.Storage.Services;

/// <summary>
/// Счётчики действий зрителей на атомарных $inc в отдельной коллекции.
/// </summary>
/// <param name="context">MongoDB-контекст приложения</param>
public class ViewerStatistics(MongoDbContext context) : IViewerStatistics
{
  private readonly IMongoCollection<ViewerStatisticModel> _statistics = context.ViewerStatistics;

  /// <inheritdoc/>
  public async Task<int> IncrementAsync(Guid roomId, Guid viewerId, string parameter,
    CancellationToken cancellationToken = default)
  {
    string field = $"{nameof(ViewerStatisticModel.Counters)}.{parameter}";

    var options = new FindOneAndUpdateOptions<ViewerStatisticModel>
    {
      IsUpsert = true,
      ReturnDocument = ReturnDocument.After
    };

    ViewerStatisticModel statistic;

    try
    {
      statistic = await _statistics.FindOneAndUpdateAsync(Filter(roomId, viewerId),
        Builders<ViewerStatisticModel>.Update.Inc(field, 1), options, cancellationToken);
    }
    catch (MongoCommandException ex) when (ex.Code == 11000)
    {
      // Параллельный upsert уже создал документ, повторный $inc попадёт в него
      statistic = await _statistics.FindOneAndUpdateAsync(Filter(roomId, viewerId),
        Builders<ViewerStatisticModel>.Update.Inc(field, 1), options, cancellationToken);
    }

    return statistic.Counters[parameter];
  }

  /// <inheritdoc/>
  public Task RemoveViewerAsync(Guid roomId, Guid viewerId, CancellationToken cancellationToken = default)
  {
    return _statistics.DeleteOneAsync(Filter(roomId, viewerId), cancellationToken);
  }

  /// <inheritdoc/>
  public Task RemoveRoomAsync(Guid roomId, CancellationToken cancellationToken = default)
  {
    return _statistics.DeleteManyAsync(Builders<ViewerStatisticModel>.Filter.Eq(s => s.RoomId, roomId),
      cancellationToken);
  }

  /// <summary>
  /// Фильтр документа счётчиков зрителя в комнате
  /// </summary>
  private static FilterDefinition<ViewerStatisticModel> Filter(Guid roomId, Guid viewerId)
  {
    return Builders<ViewerStatisticModel>.Filter.And(
      Builders<ViewerStatisticModel>.Filter.Eq(s => s.RoomId, roomId),
      Builders<ViewerStatisticModel>.Filter.Eq(s => s.ViewerId, viewerId));
  }
}

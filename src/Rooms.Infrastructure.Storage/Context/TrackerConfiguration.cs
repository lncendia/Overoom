using Rooms.Infrastructure.Storage.Models.Messages;
using Rooms.Infrastructure.Storage.Models.Rooms;

using Incendia.MongoTracker.Builders;

namespace Rooms.Infrastructure.Storage.Context;

/// <summary>
/// Конфигурация трекера изменений моделей хранения.
/// </summary>
public static class TrackerConfiguration
{
  /// <summary>
  /// Создаёт конфигурацию трекера: идентификаторы, отслеживаемые вложенные объекты и коллекции моделей.
  /// </summary>
  /// <returns>Конфигурация трекера</returns>
  public static ModelBuilder ConfigureModelBuilder()
  {
    var builder = new ModelBuilder();

    builder.Entity<RoomModel>(e =>
    {
      e.Property(r => r.Id).IsIdentifier();
      e.Property(r => r.Viewers).IsTrackedSet();
    });

    builder.Entity<ViewerModel>(e =>
    {
      // Зрители адресуются по идентификатору, а не по позиции в массиве: иначе параллельный уход другого
      // зрителя сдвигает индексы, и изменение попадает не тому зрителю
      e.Property(v => v.Id).IsIdentifier();
      e.Property(v => v.Tags).IsSet();
    });

    builder.Entity<MessageModel>(e =>
    {
      e.Property(m => m.Id).IsIdentifier();

      // Переключение реакции даёт одиночный $push или $pull, поэтому реакции разных зрителей не конфликтуют
      e.Property(m => m.Reactions).IsSet();
    });

    return builder;
  }
}

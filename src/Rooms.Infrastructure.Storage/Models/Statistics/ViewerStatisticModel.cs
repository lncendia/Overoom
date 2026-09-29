using MongoDB.Bson;

namespace Rooms.Infrastructure.Storage.Models.Statistics;

/// <summary>
/// Счётчики действий зрителя в комнате.
/// </summary>
public class ViewerStatisticModel
{
  /// <summary>
  /// Идентификатор документа
  /// </summary>
  public ObjectId Id { get; set; }

  /// <summary>
  /// Идентификатор комнаты
  /// </summary>
  public Guid RoomId { get; set; }

  /// <summary>
  /// Идентификатор зрителя
  /// </summary>
  public Guid ViewerId { get; set; }

  /// <summary>
  /// Значения счётчиков по названиям
  /// </summary>
  public Dictionary<string, int> Counters { get; set; } = [];
}

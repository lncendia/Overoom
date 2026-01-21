namespace Rooms.Infrastructure.Storage.Models.Rooms;

/// <summary>
/// Модель свойства статистики комнаты для хранения в MongoDB.
/// </summary>
public class StatisticProperty
{
  /// <summary>
  /// Название свойства статистики
  /// </summary>
  public required string Name { get; set; }

  /// <summary>
  /// Значение свойства статистики
  /// </summary>
  public int Value { get; set; }
}
using MongoTracker.Entities;

namespace Rooms.Infrastructure.Storage.Models.Rooms;

/// <summary>
/// Модель свойства статистики комнаты для хранения в MongoDB.
/// Наследует функциональность отслеживания изменений из UpdatedValueObject.
/// </summary>
public class StatisticProperty : UpdatedValueObject<RoomModel>
{
    /// <summary>
    /// Название свойства статистики
    /// </summary>
    public string Name
    {
        get;
        set => field = TrackChange(nameof(Name), field, value)!;
    } = string.Empty;

    /// <summary>
    /// Значение свойства статистики
    /// </summary>
    public int Value
    {
        get;
        set => field = TrackStructChange(nameof(Value), field, value);
    }
}

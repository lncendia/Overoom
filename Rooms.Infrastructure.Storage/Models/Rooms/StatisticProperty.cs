using MongoTracker.Entities;

namespace Rooms.Infrastructure.Storage.Models.Rooms;

/// <summary>
/// Модель свойства статистики комнаты для хранения в MongoDB.
/// Наследует функциональность отслеживания изменений из UpdatedValueObject.
/// </summary>
public class StatisticProperty : UpdatedValueObject<RoomModel>
{
    private string _name = string.Empty;
    private int _value;

    /// <summary>
    /// Название свойства статистики
    /// </summary>
    public string Name
    {
        get => _name;
        set => _name = TrackChange(nameof(Name), _name, value)!;
    }

    /// <summary>
    /// Значение свойства статистики
    /// </summary>
    public int Value
    {
        get => _value;
        set => _value = TrackStructChange(nameof(Value), _value, value);
    }
}

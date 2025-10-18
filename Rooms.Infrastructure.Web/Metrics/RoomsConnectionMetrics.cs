using System.Diagnostics.Metrics;

namespace Rooms.Infrastructure.Web.Metrics;

/// <summary>
/// Метрики для подключения пользователей к комнатам SignalR
/// </summary>
public static class RoomsConnectionMetrics
{
    /// <summary>
    /// Имя Meter для группировки метрик
    /// </summary>
    public const string MeterName = "Rooms.Web";
        
    /// <summary>
    /// Meter для создания метрик
    /// </summary>
    private static readonly Meter Meter = new(MeterName);

    /// <summary>
    /// Количество активных подключений в текущий момент
    /// </summary>
    private static int _activeConnections;

    /// <summary>
    /// Гистограмма времени выполнения методов хаба в миллисекундах
    /// </summary>
    internal static readonly Histogram<double> MethodDuration =
        Meter.CreateHistogram<double>(
            "signalr_method_duration_ms", 
            unit: "ms", 
            description: "Execution duration of SignalR hub methods in milliseconds");

    /// <summary>
    /// Статический конструктор для инициализации ObservableGauge
    /// </summary>
    static RoomsConnectionMetrics()
    {
        // Gauge — метрика, отражающая текущее состояние (в отличие от Counter)
        Meter.CreateObservableGauge(
            "rooms_active_connections",
            () => new Measurement<int>(_activeConnections),
            description: "Current number of active room connections");
    }

    /// <summary>
    /// Увеличивает количество активных подключений на 1
    /// </summary>
    internal static void Increment() => Interlocked.Increment(ref _activeConnections);

    /// <summary>
    /// Уменьшает количество активных подключений на 1
    /// </summary>
    internal static void Decrement() => Interlocked.Decrement(ref _activeConnections);
}
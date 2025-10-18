namespace Uploader.Application.Abstractions;

/// <summary>
/// Константы и настройки приложения
/// </summary>
public static class Constants
{
    /// <summary>
    /// Константы для OpenTelemetry
    /// </summary>
    public static class OpenTelemetry
    {
        /// <summary>
        /// Имя сервиса для трассировки
        /// </summary>
        public const string ServiceName = "uploader";
    }
    
    /// <summary>
    /// Константы для настройки Hangfire - системы фоновых заданий
    /// </summary>
    public static class Hangfire
    {
        /// <summary>
        /// Название очереди для обработки заданий загрузки
        /// </summary>
        public const string Queue = "uploader";
    }
}
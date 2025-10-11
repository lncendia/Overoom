namespace Uploader.Application.Abstractions;

/// <summary>
/// Константы и настройки приложения
/// </summary>
public static class Constants
{
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
namespace Films.Application.Abstractions;

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
        public const string ServiceName = "films";
    }

    /// <summary>
    /// Константы для работы с постерами
    /// </summary>
    public static class Poster
    {
        /// <summary>
        /// Формат ключа для хранения постера фильма
        /// {0} заменяется на ID фильма
        /// </summary>
        public const string FilmKeyFormat = "film/poster/{0}";

        /// <summary>
        /// Формат ключа для хранения постера подборки
        /// {0} заменяется на ID подборки
        /// </summary>
        public const string PlaylistKeyFormat = "playlist/poster/{0}";

        /// <summary>
        /// Постер фильма по умолчанию
        /// </summary>
        public const string FilmDefault = "film/poster/default";

        /// <summary>
        /// Постер подборки по умолчанию
        /// </summary>
        public const string PlaylistDefault = "playlist/poster/default";
    }

    /// <summary>
    /// MIME-типы
    /// </summary>
    public static class Mime
    {
        /// <summary>
        /// MIME-тип для JPEG фото
        /// </summary>
        public const string Photo = "image/jpeg";
    }

    /// <summary>
    /// Ограничения и лимиты
    /// </summary>
    public static class Limits
    {
        /// <summary>
        /// Максимальное число комнат у пользователя
        /// </summary>
        public const int MaxRoomsPerUser = 10;
    }
}
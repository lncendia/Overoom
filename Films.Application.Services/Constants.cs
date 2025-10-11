namespace Films.Application.Services;

/// <summary>
/// Класс с константами
/// </summary>
public static class Constants
{
    /// <summary>
    /// Формат ключа для хранения постера фильма
    /// {0} заменяется на ID фильма
    /// </summary>
    public const string FilmPosterKeyFormat = "film/poster/{0}";
    
    /// <summary>
    /// Формат ключа для хранения постера подборки
    /// {0} заменяется на ID подборки
    /// </summary>
    public const string PlaylistPosterKeyFormat = "playlist/poster/{0}";
    
    /// <summary>
    /// Постер фильма по умолчанию
    /// </summary>
    public const string FilmPosterKeyDefault = "film/poster/Default";
    
    /// <summary>
    /// Постер подборки по умолчанию
    /// </summary>
    public const string PlaylistPosterKeyDefault = "playlist/poster/Default";
    
    /// <summary>
    /// MIME-тип для JPEG фото
    /// </summary>
    public const string PhotoMimeType = "image/jpeg";
    
    /// <summary>
    /// Максимальное число комнат у пользователя
    /// </summary>
    public const int MaxRoomsCount = 5;
}
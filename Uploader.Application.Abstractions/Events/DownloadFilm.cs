namespace Uploader.Application.Abstractions.Events;

/// <summary>
/// Команда для непосредственного скачивания фильма
/// </summary>
public class DownloadFilm
{
    /// <summary>
    /// Magnet-ссылка для скачивания через торрент-клиент
    /// </summary>
    public required string MagnetUri { get; init; }
    
    /// <summary>
    /// Имя файла для обработки
    /// </summary>
    public string? FileName { get; init; }

    /// <summary>
    /// Метаданные фильма для обработки и идентификации
    /// </summary>
    public required FilmRecord FilmRecord { get; init; }
}
namespace Uploader.Application.Abstractions.Services;

/// <summary>
/// Сервис для скачивания видеофайлов из различных источников
/// </summary>
public interface IFilmDownloadService
{
    /// <summary>
    /// Асинхронно скачивает файл из указанного URI
    /// </summary>
    /// <param name="uri">URI для скачивания (magnet или http)</param>
    /// <param name="filename">Имя файла для сохранения (опционально)</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Путь к скачанному файлу</returns>
    Task<string> DownloadAsync(string uri, string? filename = null, CancellationToken cancellationToken = default);
}
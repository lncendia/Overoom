using Uploader.Application.Abstractions.DTOs;
using Uploader.Application.Abstractions.Services;
using Common.Application.FileStorage;

namespace Uploader.Infrastructure.Films;

/// <summary>
/// Реализация хранилища фильмов
/// </summary>
/// <param name="fileStorage">Хранилище файлов</param>
public sealed class HlsS3Storage(IFileStorage fileStorage) : IHlsStorage
{
    /// <summary>
    /// Хранилище файлов
    /// </summary>
    private readonly IFileStorage _fileStorage = fileStorage;

    /// <summary>
    /// Загружает HLS-файлы фильма в S3-совместимое хранилище
    /// </summary>
    /// <param name="film">Метаданные фильма для построения ключей S3</param>
    /// <param name="directory">Путь к локальной директории с HLS-файлами</param>
    /// <param name="token">Токен отмены для прерывания операции</param>
    public async Task UploadAsync(FilmRecord film, string directory, CancellationToken token = default)
    {
        // Рекурсивно получаем все файлы из директории и поддиректорий
        var files = Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories);

        // Обрабатываем каждый файл в директории
        foreach (var filePath in files)
        {
            // Проверяем не была ли запрошена отмена операции
            token.ThrowIfCancellationRequested();

            // Получаем относительный путь файла относительно корневой директории
            var relativePath = Path.GetRelativePath(directory, filePath).Replace("\\", "/");

            // Формируем ключ для S3 хранилища на основе метаданных фильма
            var key = $"{BuildKey(film)}/{relativePath}";

            // Определяем MIME-тип содержимого файла на основе расширения
            var contentType = GetContentType(filePath);

            // Открываем файловый поток для чтения в асинхронном режиме
            await using var stream = File.OpenRead(filePath);

            // Загружаем файл в S3-совместимое хранилище
            await _fileStorage.UploadAsync(key, stream, contentType, token: token);
        }
    }

    /// <summary>
    /// Удаляет файл из S3
    /// </summary>
    /// <param name="film">Метаданные фильма</param>
    /// <param name="token">Токен отмены</param>
    public Task DeleteAsync(FilmRecord film, CancellationToken token = default)
    {
        // Генерируем ключ в S3
        var key = BuildKey(film);

        // Выполняем удаление
        return _fileStorage.DeleteAsync(key, token: token);
    }

    /// <summary>
    /// Проверяет, загружен ли фильм
    /// </summary>
    /// <param name="film">Метаданные фильма</param>
    /// <param name="token">Токен отмены</param>
    public Task<bool> IsExistsAsync(FilmRecord film, CancellationToken token = default)
    {
        // Генерируем ключ в S3
        var key = BuildKey(film);

        // Выполняем удаление
        return _fileStorage.IsPathExistAsync(key, token: token);
    }

    /// <summary>
    /// Генерирует ключ объекта в S3 на основе метаданных фильма
    /// </summary>
    /// <param name="film">Метаданные фильма</param>
    /// <returns>Ключ объекта в S3</returns>
    private static string BuildKey(FilmRecord film)
    {
        // Формируем список частей ключа S3
        var parts = new List<string> { film.Id.ToString() };

        // Добавляем сезон в формате s01, s02 и т.д., если указан
        if (film.Season.HasValue) parts.Add($"s{film.Season.Value:D2}");

        // Добавляем эпизод в формате e01, e02 и т.д., если указан
        if (film.Episode.HasValue) parts.Add($"e{film.Episode.Value:D2}");

        // Добавляем версию фильма
        parts.Add(film.Version);

        // Объединяем все части через слеш для формирования итогового ключа
        return string.Join('/', parts);
    }

    /// <summary>
    /// Определяет MIME-тип по расширению файла
    /// </summary>
    private static string GetContentType(string filePath)
    {
        var ext = Path.GetExtension(filePath).ToLowerInvariant();

        return ext switch
        {
            ".m3u8" => "application/vnd.apple.mpegurl",
            ".ts" => "video/mp2t",
            _ => "application/octet-stream"
        };
    }
}
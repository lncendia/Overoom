using Uploader.Application.Abstractions.DTOs;

namespace Uploader.Application.Abstractions.Services;

/// <summary>
/// Интерфейс хранилища фильмов.
/// </summary>
public interface IHlsStorage
{
    /// <summary>
    /// Загружает файл в S3, используя массив байтов в качестве содержимого.
    /// </summary>
    /// <param name="film">Данные фильма.</param>
    /// <param name="directory">Путь к данным.</param>
    /// <param name="token">Токен отмены для отслеживания отмены операции.</param>
    Task UploadAsync(FilmRecord film, string directory, CancellationToken token = default);

    /// <summary>
    /// Удалить файл из S3.
    /// </summary>
    /// <param name="film">Данные фильма.</param>
    /// <param name="token">Токен отмены для отслеживания отмены операции.</param>
    Task DeleteAsync(FilmRecord film, CancellationToken token = default);

    /// <summary>
    /// Проверяет, загружен ли фильм
    /// </summary>
    /// <param name="film">Метаданные фильма</param>
    /// <param name="token">Токен отмены</param>
    Task<bool> IsExistsAsync(FilmRecord film, CancellationToken token = default);
}
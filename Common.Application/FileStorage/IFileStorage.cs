namespace Common.Application.FileStorage;

/// <summary>
/// Интерфейс хранилища файлов
/// </summary>
public interface IFileStorage
{
    /// <summary>
    /// Загружает файл в S3, используя поток в качестве содержимого.
    /// </summary>
    /// <param name="key">Ключ (имя) файла в S3.</param>
    /// <param name="stream">Содержимое файла.</param>
    /// <param name="contentType">MIME-тип содержимого файла.</param>
    /// <param name="bucket">Необязательный параметр. Название бакета. Если не указано, используется бакет по умолчанию.</param>
    /// <param name="token">Токен отмены для отслеживания отмены операции.</param>
    Task UploadAsync(string key, Stream stream, string contentType, string? bucket = null, CancellationToken token = default);

    /// <summary>
    /// Загружает файл в S3, используя URL в качестве источника данных.
    /// </summary>
    /// <param name="key">Ключ (путь) для сохранения файла в S3</param>
    /// <param name="url">URL исходного файла для загрузки</param>
    /// <param name="contentType">MIME-тип содержимого</param>
    /// <param name="bucket">Имя корзины S3 (если null, используется корзина по умолчанию)</param>
    /// <param name="token">Токен отмены операции</param>
    /// <returns>Задача, представляющая асинхронную операцию загрузки</returns>
    /// <exception cref="HttpRequestException">Выбрасывается при проблемах загрузки файла по URL</exception>
   Task UploadAsync(string key,
        Uri url,
        string contentType,
        string? bucket = null,
        CancellationToken token = default);

    /// <summary>
    /// Скачивает файл из S3.
    /// </summary>
    /// <param name="key">Ключ (имя) файла в S3.</param>
    /// <param name="bucket">Необязательный параметр. Название бакета. Если не указано, используется бакет по умолчанию.</param>
    /// <param name="token">Токен отмены для отслеживания отмены операции.</param>
    Task<(Stream Stream, string ContentType)> GetAsync(string key, string? bucket = null, CancellationToken token = default);

    /// <summary>
    /// Удалить файл из S3.
    /// </summary>
    /// <param name="key">Ключ (имя) файла в S3.</param>
    /// <param name="bucket">Необязательный параметр. Название бакета. Если не указано, используется бакет по умолчанию.</param>
    /// <param name="token">Токен отмены для отслеживания отмены операции.</param>
    Task DeleteAsync(string key, string? bucket = null, CancellationToken token = default);

    /// <summary>
    /// Проверить содержит ли папка файлы.
    /// </summary>
    /// <param name="prefix">Путь в S3.</param>
    /// <param name="bucket">Необязательный параметр. Название бакета. Если не указано, используется бакет по умолчанию.</param>
    /// <param name="token">Токен отмены для отслеживания отмены операции.</param>
    Task<bool> IsPathExistAsync(string prefix, string? bucket = null, CancellationToken token = default);
}
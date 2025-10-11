using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Common.Application.FileStorage;

namespace Common.Infrastructure.FileStorage;

/// <summary>
/// Реализация интерфейса IFileStorage для работы с AWS S3
/// Предоставляет методы для загрузки, скачивания и управления файлами в S3-совместимом хранилище
/// </summary>
public sealed class AwsS3ApiClient : IFileStorage
{
    private readonly IAmazonS3 _client;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _defaultBucket;
    public const string HttpClientName = "FileStorageHttpClient";

    /// <summary>
    /// Инициализирует новый экземпляр клиента для работы с AWS S3
    /// </summary>
    /// <param name="client">Клиент Amazon S3 для взаимодействия с API</param>
    /// <param name="httpClientFactory">Фабрика HTTP-клиентов для загрузки по URL</param>
    /// <param name="awsHttpClientFactory">Кастомная фабрика HTTP-клиентов для AWS</param>
    /// <param name="defaultBucket">Бакет по умолчанию для операций с файлами</param>
    public AwsS3ApiClient(IAmazonS3 client, IHttpClientFactory httpClientFactory,
        AwsS3HttpClientFactory awsHttpClientFactory, string defaultBucket)
    {
        _client = client;
        _defaultBucket = defaultBucket;
        _httpClientFactory = httpClientFactory;
        var amazonS3Config = (AmazonS3Config)_client.Config;
        amazonS3Config.HttpClientFactory = awsHttpClientFactory;
        amazonS3Config.ForcePathStyle = true;
    }

    /// <summary>
    /// Загружает файл в S3, используя поток данных в качестве содержимого
    /// </summary>
    /// <param name="key">Ключ (путь) файла в S3</param>
    /// <param name="stream">Поток данных для загрузки</param>
    /// <param name="contentType">MIME-тип содержимого файла</param>
    /// <param name="bucket">Имя бакета (если null, используется бакет по умолчанию)</param>
    /// <param name="token">Токен отмены операции</param>
    public async Task UploadAsync(string key,
        Stream stream,
        string contentType,
        string? bucket = null,
        CancellationToken token = default)
    {
        // Используем TransferUtility для эффективной загрузки файлов
        var fileTransferUtility = new TransferUtility(_client);
        var request = new TransferUtilityUploadRequest
        {
            InputStream = stream,
            ContentType = contentType,
            Key = key,
            BucketName = bucket ?? _defaultBucket,
        };
        await fileTransferUtility.UploadAsync(request, token);
    }

    /// <summary>
    /// Загружает файл в S3, используя URL в качестве источника данных
    /// Скачивает файл по URL и затем загружает его в S3
    /// </summary>
    /// <param name="key">Ключ (путь) файла в S3</param>
    /// <param name="url">URL источника данных для загрузки</param>
    /// <param name="contentType">MIME-тип содержимого файла</param>
    /// <param name="bucket">Имя бакета (если null, используется бакет по умолчанию)</param>
    /// <param name="token">Токен отмены операции</param>
    public async Task UploadAsync(string key,
        Uri url,
        string contentType,
        string? bucket = null,
        CancellationToken token = default)
    {
        // Создаем HTTP-клиент для скачивания файла
        var httpClient = _httpClientFactory.CreateClient(HttpClientName);

        // Скачиваем файл по URL
        var response = await httpClient.GetAsync(url, token);
        response.EnsureSuccessStatusCode();

        // Загружаем скачанный файл в S3
        await using var stream = await response.Content.ReadAsStreamAsync(token);
        await UploadAsync(key, stream, contentType, bucket, token);
    }

    /// <summary>
    /// Скачивает файл из S3
    /// </summary>
    /// <param name="key">Ключ (путь) файла в S3</param>
    /// <param name="bucket">Имя бакета (если null, используется бакет по умолчанию)</param>
    /// <param name="token">Токен отмены операции</param>
    /// <returns>Поток данных файла и его MIME-тип</returns>
    /// <exception cref="FileNotFoundException">Если файл не найден в S3</exception>
    public async Task<(Stream Stream, string ContentType)> GetAsync(string key, string? bucket = null, CancellationToken token = default)
    {
        var request = new GetObjectRequest
        {
            BucketName = bucket ?? _defaultBucket,
            Key = key,
        };

        try
        {
            // Получаем объект из S3
            var response = await _client.GetObjectAsync(request, token);
            return (response.ResponseStream, response.Headers.ContentType);
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            // Преобразуем S3 исключение в стандартное FileNotFoundException
            throw new FileNotFoundException($"The file with the key '{key}' was not found in the bucket '{request.BucketName}'.", ex);
        }
    }

    /// <summary>
    /// Удаляет файл из S3
    /// </summary>
    /// <param name="key">Ключ (путь) файла в S3</param>
    /// <param name="bucket">Имя бакета (если null, используется бакет по умолчанию)</param>
    /// <param name="token">Токен отмены операции</param>
    public async Task DeleteAsync(string key, string? bucket = null, CancellationToken token = default)
    {
        var request = new DeleteObjectRequest
        {
            BucketName = bucket ?? _defaultBucket,
            Key = key
        };

        await _client.DeleteObjectAsync(request, token);
    }
    
    /// <summary>
    /// Проверяет, существует ли путь (префикс) в хранилище S3
    /// </summary>
    /// <param name="prefix">Префикс пути для проверки</param>
    /// <param name="bucket">Имя бакета (если null, используется бакет по умолчанию)</param>
    /// <param name="token">Токен отмены операции</param>
    /// <returns>True если путь существует, иначе False</returns>
    public async Task<bool> IsPathExistAsync(string prefix, string? bucket = null, CancellationToken token = default)
    {
        var request = new ListObjectsV2Request
        {
            BucketName = bucket ?? _defaultBucket,
            Prefix = prefix,
            MaxKeys = 1
        };
        var response = await _client.ListObjectsV2Async(request, token);
        return response.KeyCount > 0;
    }
}
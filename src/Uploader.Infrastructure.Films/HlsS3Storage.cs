using Uploader.Application.Abstractions.Services;
using Common.Application.FileStorage;
using Uploader.Application.Abstractions.Events;

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
    IEnumerable<string> files = Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories);

    foreach (string filePath in files)
    {
      token.ThrowIfCancellationRequested();
      string relativePath = Path.GetRelativePath(directory, filePath).Replace("\\", "/");
      string key = $"{BuildKey(film)}/{relativePath}";
      string contentType = GetContentType(filePath);
      await using FileStream stream = File.OpenRead(filePath);
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
    string key = BuildKey(film);

    return _fileStorage.DeleteAsync(key, token: token);
  }

  /// <summary>
  /// Проверяет, загружен ли фильм
  /// </summary>
  /// <param name="film">Метаданные фильма</param>
  /// <param name="token">Токен отмены</param>
  public Task<bool> IsExistsAsync(FilmRecord film, CancellationToken token = default)
  {
    string key = BuildKey(film);

    return _fileStorage.IsPathExistAsync(key, token: token);
  }

  /// <summary>
  /// Генерирует ключ объекта в S3 на основе метаданных фильма
  /// </summary>
  /// <param name="film">Метаданные фильма</param>
  /// <returns>Ключ объекта в S3</returns>
  private static string BuildKey(FilmRecord film)
  {
    var parts = new List<string> { film.Id.ToString() };
    if (film.Season.HasValue) parts.Add($"s{film.Season.Value:D2}");
    if (film.Episode.HasValue) parts.Add($"e{film.Episode.Value:D2}");

    parts.Add(film.Version);

    return string.Join('/', parts);
  }

  /// <summary>
  /// Определяет MIME-тип по расширению файла
  /// </summary>
  private static string GetContentType(string filePath)
  {
    string ext = Path.GetExtension(filePath).ToLowerInvariant();

    return ext switch
    {
      ".m3u8" => "application/vnd.apple.mpegurl",
      ".ts" => "video/mp2t",
      _ => "application/octet-stream"
    };
  }
}
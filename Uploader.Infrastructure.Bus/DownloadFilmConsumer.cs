using Common.IntegrationEvents.Uploader;
using MassTransit;
using Microsoft.Extensions.Logging;
using Uploader.Application.Abstractions.Events;
using Uploader.Application.Abstractions.Services;

namespace Uploader.Infrastructure.Bus;

/// <summary>
/// Обработчик события DownloadFilmCommand
/// </summary>
/// <remarks>
/// Выполняет полный цикл обработки фильма:
/// 1. Загрузка по magnet-ссылке
/// 2. Транскодирование в различные разрешения
/// 3. Загрузка в файловое хранилище
/// 4. Публикация события о завершении обработки
/// </remarks>
/// <param name="download">Сервис загрузки фильмов</param>
/// <param name="transcoder">Сервис транскодирования видео</param>
/// <param name="filmStorage">Хранилище файлов фильмов</param>
/// <param name="logger">Логгер</param>
public class DownloadFilmConsumer(
  IFilmDownloadService download,
  IHlsTranscodingService transcoder,
  IHlsStorage filmStorage,
  ILogger<DownloadFilmConsumer> logger) : IJobConsumer<DownloadFilm>
{
  public async Task Run(JobContext<DownloadFilm> context)
  {
    DownloadFilm request = context.Job;
    logger.LogInformation("Downloading a movie by URI: {Uri}", request.MagnetUri);

    string originalFilePath =
      await download.DownloadAsync(request.MagnetUri, request.FileName, context.CancellationToken);

    logger.LogInformation("The movie is downloaded: {Path}", originalFilePath);
    await context.SetJobProgress(1, 4);

    string outputPath = Path.Join(Path.GetDirectoryName(originalFilePath),
      $"hls_{request.FilmRecord.Season ?? 0}_{request.FilmRecord.Episode ?? 0}");

    if (context.LastProgressValue < 2)
    {
      await transcoder.TranscodeAsync(originalFilePath, request.FilmRecord.Resolution, outputPath,
        context.CancellationToken);

      logger.LogInformation("Successful conversion: {Path}", outputPath);
      await context.SetJobProgress(2, 4);
    }

    if (context.LastProgressValue < 3)
    {
      if (await filmStorage.IsExistsAsync(request.FilmRecord, context.CancellationToken))
      {
        logger.LogInformation("Replacing existing film version: {Path}", outputPath);
        await filmStorage.DeleteAsync(request.FilmRecord, context.CancellationToken);
      }

      await filmStorage.UploadAsync(request.FilmRecord, outputPath, context.CancellationToken);
      await context.SetJobProgress(3, 4);
    }

    var @event = new VersionDownloadedIntegrationEvent
    {
      FilmId = request.FilmRecord.Id,
      Version = request.FilmRecord.Version,
      Season = request.FilmRecord.Season,
      Episode = request.FilmRecord.Episode
    };

    await context.Publish(@event, context.CancellationToken);
  }
}
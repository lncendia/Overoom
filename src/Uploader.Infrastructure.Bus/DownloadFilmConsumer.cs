using Common.IntegrationEvents.Uploader;
using MassTransit;
using Microsoft.Extensions.Logging;
using Uploader.Application.Abstractions.Events;
using Uploader.Application.Abstractions.Jobs;
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
/// Текущий этап и пути к файлам сохраняются в состоянии задачи (<see cref="DownloadFilmJobState"/>),
/// поэтому при повторном запуске задача продолжает с того этапа, на котором остановилась.
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
  /// <summary>
  /// Максимальное значение прогресса задачи
  /// </summary>
  private const long ProgressLimit = 100;

  /// <summary>
  /// Прогресс задачи в процентах в начале каждого этапа. Используется только для отображения
  /// </summary>
  private static readonly Dictionary<JobStage, long> StageProgress = new()
  {
    [JobStage.Downloading] = 0,
    [JobStage.Transcoding] = 60,
    [JobStage.Uploading] = 80,
    [JobStage.Publishing] = 95
  };

  public async Task Run(JobContext<DownloadFilm> context)
  {
    DownloadFilm request = context.Job;
    context.TryGetJobState(out DownloadFilmJobState? saved);

    DownloadFilmJobState state = ResolveStage(saved ?? new DownloadFilmJobState());
    if (state != saved) await context.SaveJobState(state);

    logger.LogInformation("Starting the job from the {Stage} stage", state.Stage);

    if (state.Stage == JobStage.Downloading)
    {
      logger.LogInformation("Downloading a movie by URI: {Uri}", request.MagnetUri);

      string originalFilePath = await download.DownloadAsync(request.MagnetUri, request.FileName,
        percent => ReportDownloadProgressAsync(context, percent), context.CancellationToken);

      logger.LogInformation("The movie is downloaded: {Path}", originalFilePath);

      state = await MoveToStageAsync(context, state with
      {
        Stage = JobStage.Transcoding,
        OriginalFilePath = originalFilePath,
        OutputPath = Path.Join(Path.GetDirectoryName(originalFilePath),
          $"hls_{request.FilmRecord.Season ?? 0}_{request.FilmRecord.Episode ?? 0}")
      });
    }

    if (state.Stage == JobStage.Transcoding)
    {
      await transcoder.TranscodeAsync(state.OriginalFilePath!, request.FilmRecord.Resolution, state.OutputPath!,
        context.CancellationToken);

      logger.LogInformation("Successful conversion: {Path}", state.OutputPath);
      state = await MoveToStageAsync(context, state with { Stage = JobStage.Uploading });
    }

    if (state.Stage == JobStage.Uploading)
    {
      if (await filmStorage.IsExistsAsync(request.FilmRecord, context.CancellationToken))
      {
        logger.LogInformation("Replacing existing film version: {Path}", state.OutputPath);
        await filmStorage.DeleteAsync(request.FilmRecord, context.CancellationToken);
      }

      await filmStorage.UploadAsync(request.FilmRecord, state.OutputPath!, context.CancellationToken);
      state = await MoveToStageAsync(context, state with { Stage = JobStage.Publishing });
    }

    var @event = new VersionDownloadedIntegrationEvent
    {
      FilmId = request.FilmRecord.Id,
      Version = request.FilmRecord.Version,
      Season = request.FilmRecord.Season,
      Episode = request.FilmRecord.Episode
    };

    await context.Publish(@event, context.CancellationToken);

    DeleteTemporaryFiles(state.OriginalFilePath, state.OutputPath);
  }

  /// <summary>
  /// Возвращается к более раннему этапу, если результата предыдущего этапа больше нет на диске
  /// </summary>
  /// <param name="state">Сохранённое состояние задачи</param>
  /// <returns>Состояние с этапом, с которого можно продолжить</returns>
  private DownloadFilmJobState ResolveStage(DownloadFilmJobState state)
  {
    if (state.Stage == JobStage.Uploading && !Directory.Exists(state.OutputPath))
    {
      logger.LogWarning("Transcoding result not found, transcoding again: {Path}", state.OutputPath);
      state = state with { Stage = JobStage.Transcoding };
    }

    if (state.Stage == JobStage.Transcoding && !File.Exists(state.OriginalFilePath))
    {
      logger.LogWarning("Downloaded file not found, downloading again: {Path}", state.OriginalFilePath);
      state = state with { Stage = JobStage.Downloading };
    }

    return state;
  }

  /// <summary>
  /// Сохраняет переход задачи на следующий этап и отмечает его в прогрессе
  /// </summary>
  /// <param name="context">Контекст задачи</param>
  /// <param name="state">Новое состояние задачи</param>
  /// <returns>Сохранённое состояние</returns>
  private static async Task<DownloadFilmJobState> MoveToStageAsync(JobContext context, DownloadFilmJobState state)
  {
    await context.SaveJobState(state);
    await context.SetJobProgress(StageProgress[state.Stage], ProgressLimit);
    return state;
  }

  /// <summary>
  /// Переводит процент скачивания в прогресс задачи в пределах этапа скачивания
  /// </summary>
  /// <param name="context">Контекст задачи</param>
  /// <param name="percent">Процент скачивания файла</param>
  private async Task ReportDownloadProgressAsync(JobContext context, double percent)
  {
    long value = (long)(Math.Clamp(percent, 0, 100) / 100 * StageProgress[JobStage.Transcoding]);

    try
    {
      await context.SetJobProgress(value, ProgressLimit);
    }
    catch (Exception e)
    {
      logger.LogWarning(e, "Failed to report download progress");
    }
  }

  /// <summary>
  /// Удаляет скачанный оригинал и результат транскодирования после успешной загрузки в хранилище
  /// </summary>
  /// <param name="originalFilePath">Путь к скачанному файлу, если известен</param>
  /// <param name="outputPath">Каталог с результатом транскодирования, если известен</param>
  private void DeleteTemporaryFiles(string? originalFilePath, string? outputPath)
  {
    try
    {
      if (Directory.Exists(outputPath)) Directory.Delete(outputPath, recursive: true);
      if (File.Exists(originalFilePath)) File.Delete(originalFilePath);
    }
    catch (Exception e)
    {
      logger.LogWarning(e, "Failed to delete temporary files: {Path}", originalFilePath);
    }
  }
}

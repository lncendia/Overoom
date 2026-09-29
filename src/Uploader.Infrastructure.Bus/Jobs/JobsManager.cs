using Common.Application.DTOs;
using Common.Domain.Enums;
using MassTransit;
using MassTransit.Contracts.JobService;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using Uploader.Application.Abstractions.Events;
using Uploader.Application.Abstractions.Jobs;
using JobStatus = Uploader.Application.Abstractions.Jobs.JobStatus;
using JobNotFoundException = Uploader.Application.Abstractions.Jobs.JobNotFoundException;

namespace Uploader.Infrastructure.Bus.Jobs;

/// <summary>
/// Управление задачами загрузки поверх job-сервиса MassTransit.
/// </summary>
/// <remarks>
/// Список задач MassTransit не предоставляет, поэтому он читается из коллекции саг задач,
/// а состояние каждой задачи запрашивается у самой саги через <see cref="GetJobState"/>.
/// Управление — стандартными командами CancelJob, RetryJob и FinalizeJob.
/// </remarks>
/// <param name="database">База данных MassTransit</param>
/// <param name="publishEndpoint">Точка публикации сообщений</param>
/// <param name="stateClient">Клиент запроса состояния задачи</param>
/// <param name="logger">Логгер</param>
public class JobsManager(
  IMongoDatabase database,
  IPublishEndpoint publishEndpoint,
  IRequestClient<GetJobState> stateClient,
  ILogger<JobsManager> logger) : IJobsManager
{
  /// <summary>
  /// Коллекция саг задач (имя по умолчанию для MongoDB-репозитория саг MassTransit)
  /// </summary>
  private const string JobSagasCollection = "job.sagas";

  /// <summary>
  /// Время ожидания ответа саги на запрос состояния
  /// </summary>
  private static readonly RequestTimeout StateTimeout = RequestTimeout.After(s: 5);

  /// <summary>
  /// Коллекция саг задач
  /// </summary>
  private IMongoCollection<BsonDocument> Sagas => database.GetCollection<BsonDocument>(JobSagasCollection);

  /// <inheritdoc/>
  public Task<Guid> SubmitAsync(DownloadFilm job, CancellationToken token = default)
  {
    return publishEndpoint.SubmitJob(job, token);
  }

  /// <inheritdoc/>
  public async Task<CountResult<JobDto>> GetAsync(int skip, int take, CancellationToken token = default)
  {
    FilterDefinition<BsonDocument> filter = FilterDefinition<BsonDocument>.Empty;
    long total = await Sagas.CountDocumentsAsync(filter, cancellationToken: token);

    List<BsonDocument> documents = await Sagas.Find(filter)
      .Sort(Builders<BsonDocument>.Sort.Descending("Submitted"))
      .Skip(skip)
      .Limit(take)
      .ToListAsync(token);

    JobDto[] jobs = await Task.WhenAll(documents.Select(d => ToDtoAsync(d, token)));

    return new CountResult<JobDto> { List = jobs, TotalCount = (int)total };
  }

  /// <inheritdoc/>
  public async Task CancelAsync(Guid id, CancellationToken token = default)
  {
    JobStatus status = await GetStatusAsync(id, token);
    if (status is not (JobStatus.Pending or JobStatus.Running))
      throw new JobStateConflictException(id, status, "cancel");

    await publishEndpoint.CancelJob(id, "Отменено администратором");
  }

  /// <inheritdoc/>
  public async Task RetryAsync(Guid id, CancellationToken token = default)
  {
    JobStatus status = await GetStatusAsync(id, token);
    if (status is not (JobStatus.Faulted or JobStatus.Canceled))
      throw new JobStateConflictException(id, status, "retry");

    await publishEndpoint.RetryJob(id);
  }

  /// <inheritdoc/>
  public async Task DeleteAsync(Guid id, CancellationToken token = default)
  {
    JobStatus status = await GetStatusAsync(id, token);
    if (status is not (JobStatus.Completed or JobStatus.Faulted or JobStatus.Canceled))
      throw new JobStateConflictException(id, status, "delete");

    await publishEndpoint.FinalizeJob(id);
  }

  /// <summary>
  /// Проверяет существование задачи и возвращает её состояние
  /// </summary>
  /// <param name="id">Идентификатор задачи</param>
  /// <param name="token">Токен отмены</param>
  /// <exception cref="JobNotFoundException">Если задача не найдена</exception>
  private async Task<JobStatus> GetStatusAsync(Guid id, CancellationToken token)
  {
    bool exists = await Sagas.Find(Builders<BsonDocument>.Filter.Eq("_id", id)).AnyAsync(token);
    if (!exists) throw new JobNotFoundException(id);

    JobState? state = await RequestStateAsync(id, token);
    return state == null ? JobStatus.Unknown : ToStatus(state.CurrentState);
  }

  /// <summary>
  /// Запрашивает состояние задачи у саги
  /// </summary>
  /// <param name="id">Идентификатор задачи</param>
  /// <param name="token">Токен отмены</param>
  /// <returns>Состояние задачи или null, если сага не ответила</returns>
  private async Task<JobState?> RequestStateAsync(Guid id, CancellationToken token)
  {
    try
    {
      Response<JobState> response = await stateClient.GetResponse<JobState>(new { JobId = id }, token, StateTimeout);
      return response.Message;
    }
    catch (RequestTimeoutException)
    {
      logger.LogWarning("Job {JobId} did not respond to the state request", id);
      return null;
    }
  }

  /// <summary>
  /// Собирает описание задачи из документа саги и ответа на запрос состояния
  /// </summary>
  /// <param name="document">Документ саги</param>
  /// <param name="token">Токен отмены</param>
  private async Task<JobDto> ToDtoAsync(BsonDocument document, CancellationToken token)
  {
    Guid id = ToGuid(document["_id"]) ?? Guid.Empty;
    JobState? state = await RequestStateAsync(id, token);
    BsonDocument? job = GetValue(document, "Job") as BsonDocument;

    JobStatus status = state == null ? JobStatus.Unknown : ToStatus(state.CurrentState);
    // При повторе MassTransit не сбрасывает время и причину ошибки прошлой попытки,
    // поэтому они показываются только для задач, которые сейчас упали или отменены (отмена тоже пишется в Faulted)
    bool isFinishedWithError = status is JobStatus.Faulted or JobStatus.Canceled;

    double? progress = state is { ProgressLimit: > 0, ProgressValue: not null }
      ? Math.Round(100.0 * state.ProgressValue.Value / state.ProgressLimit.Value, 1)
      : null;

    return new JobDto
    {
      Id = id,
      Status = status,
      State = state?.CurrentState ?? "Unknown",
      Submitted = state?.Submitted ?? ToDate(GetValue(document, "Submitted")),
      Started = state?.Started ?? ToDate(GetValue(document, "Started")),
      Completed = status == JobStatus.Completed
        ? state?.Completed ?? ToDate(GetValue(document, "Completed"))
        : null,
      Faulted = isFinishedWithError ? state?.Faulted ?? ToDate(GetValue(document, "Faulted")) : null,
      Reason = isFinishedWithError ? state?.Reason ?? ToText(GetValue(document, "Reason")) : null,
      RetryAttempt = state?.LastRetryAttempt ?? 0,
      Progress = progress,
      Stage = status is JobStatus.Completed or JobStatus.Unknown ? null : ToStage(document, status),
      Film = job == null ? null : ToFilmRecord(GetValue(job, "FilmRecord") as BsonDocument),
      FilmTitle = job == null ? null : ToText(GetValue(job, "FilmTitle")),
      MagnetUri = job == null ? null : ToText(GetValue(job, "MagnetUri")),
      FileName = job == null ? null : ToText(GetValue(job, "FileName"))
    };
  }

  /// <summary>
  /// Переводит состояние саги MassTransit в обобщённое состояние задачи
  /// </summary>
  /// <param name="state">Название состояния саги</param>
  private static JobStatus ToStatus(string? state)
  {
    return state switch
    {
      "Started" => JobStatus.Running,
      "Completed" => JobStatus.Completed,
      "Faulted" => JobStatus.Faulted,
      "Canceled" => JobStatus.Canceled,
      "Submitted" or "WaitingToStart" or "WaitingToRetry" or "WaitingForSlot"
        or "StartingJobAttempt" or "AllocatingJobSlot" => JobStatus.Pending,
      _ => JobStatus.Unknown
    };
  }

  /// <summary>
  /// Читает этап обработки из состояния, которое задача сохраняет через SaveJobState
  /// </summary>
  /// <param name="document">Документ саги</param>
  /// <param name="status">Состояние задачи</param>
  /// <returns>Этап, на котором задача выполняется, упала или с которого продолжит</returns>
  private static JobStage? ToStage(BsonDocument document, JobStatus status)
  {
    BsonValue? stage = GetValue(document, "JobState") is BsonDocument jobState ? GetValue(jobState, "Stage") : null;

    return stage switch
    {
      { IsString: true } when Enum.TryParse(stage.AsString, true, out JobStage parsed) => parsed,
      { IsNumeric: true } when Enum.IsDefined(typeof(JobStage), stage.ToInt32()) => (JobStage)stage.ToInt32(),
      // Задача ещё не сохраняла состояние: выполняющаяся задача в этом случае скачивает торрент
      _ => status == JobStatus.Running ? JobStage.Downloading : null
    };
  }

  /// <summary>
  /// Читает данные фильма из сохранённого сообщения задачи
  /// </summary>
  /// <param name="document">Документ FilmRecord</param>
  private static FilmRecord? ToFilmRecord(BsonDocument? document)
  {
    if (document == null) return null;

    Guid? id = ToGuid(GetValue(document, "Id"));
    if (id == null) return null;

    BsonValue? resolution = GetValue(document, "Resolution");
    FilmResolution parsedResolution = resolution switch
    {
      { IsString: true } when Enum.TryParse(resolution.AsString, true, out FilmResolution r) => r,
      { IsNumeric: true } => (FilmResolution)resolution.ToInt32(),
      _ => default
    };

    return new FilmRecord
    {
      Id = id.Value,
      Resolution = parsedResolution,
      Version = ToText(GetValue(document, "Version")) ?? string.Empty,
      Season = ToInt(GetValue(document, "Season")),
      Episode = ToInt(GetValue(document, "Episode"))
    };
  }

  /// <summary>
  /// Возвращает значение поля без учёта регистра имени: сообщения задач сохраняются в camelCase.
  /// </summary>
  /// <remarks>
  /// Вложенные объекты сообщения хранятся как словари, которые MongoDB-сериализатор оборачивает
  /// в документ с дискриминатором { _t: тип, _v: значение }, поэтому такая обёртка разворачивается.
  /// </remarks>
  /// <param name="document">Документ</param>
  /// <param name="name">Имя поля</param>
  private static BsonValue? GetValue(BsonDocument document, string name)
  {
    BsonElement element = document.Elements
      .FirstOrDefault(e => string.Equals(e.Name, name, StringComparison.OrdinalIgnoreCase));

    BsonValue? value = element.Value;
    if (value is BsonDocument wrapper && wrapper.Contains("_t") && wrapper.Contains("_v")) value = wrapper["_v"];

    return value is null or BsonNull ? null : value;
  }

  private static Guid? ToGuid(BsonValue? value)
  {
    return value switch
    {
      BsonBinaryData binary when binary.IsGuid => binary.ToGuid(),
      BsonString text when Guid.TryParse(text.Value, out Guid guid) => guid,
      _ => null
    };
  }

  private static DateTime? ToDate(BsonValue? value)
  {
    return value is { IsValidDateTime: true } ? value.ToUniversalTime() : null;
  }

  private static string? ToText(BsonValue? value)
  {
    return value is { IsString: true } ? value.AsString : null;
  }

  private static int? ToInt(BsonValue? value)
  {
    return value is { IsNumeric: true } ? value.ToInt32() : null;
  }
}

using Uploader.Application.Abstractions.Events;

namespace Uploader.Application.Abstractions.Jobs;

/// <summary>
/// Фоновая задача загрузки фильма
/// </summary>
public class JobDto
{
  /// <summary>
  /// Идентификатор задачи
  /// </summary>
  public required Guid Id { get; init; }

  /// <summary>
  /// Состояние задачи
  /// </summary>
  public required JobStatus Status { get; init; }

  /// <summary>
  /// Название состояния в терминах MassTransit (Submitted, Started, WaitingToRetry и т.д.)
  /// </summary>
  public required string State { get; init; }

  /// <summary>
  /// Время постановки в очередь
  /// </summary>
  public DateTime? Submitted { get; init; }

  /// <summary>
  /// Время запуска
  /// </summary>
  public DateTime? Started { get; init; }

  /// <summary>
  /// Время успешного завершения
  /// </summary>
  public DateTime? Completed { get; init; }

  /// <summary>
  /// Время ошибки
  /// </summary>
  public DateTime? Faulted { get; init; }

  /// <summary>
  /// Причина ошибки или отмены
  /// </summary>
  public string? Reason { get; init; }

  /// <summary>
  /// Номер последней попытки выполнения
  /// </summary>
  public int RetryAttempt { get; init; }

  /// <summary>
  /// Прогресс выполнения в процентах
  /// </summary>
  public double? Progress { get; init; }

  /// <summary>
  /// Этап выполнения
  /// </summary>
  public JobStage? Stage { get; init; }

  /// <summary>
  /// Фильм, для которого выполняется задача
  /// </summary>
  public FilmRecord? Film { get; init; }

  /// <summary>
  /// Название фильма
  /// </summary>
  public string? FilmTitle { get; init; }

  /// <summary>
  /// Magnet-ссылка
  /// </summary>
  public string? MagnetUri { get; init; }

  /// <summary>
  /// Имя файла внутри торрента
  /// </summary>
  public string? FileName { get; init; }
}

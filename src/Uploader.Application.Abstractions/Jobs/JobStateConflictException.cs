namespace Uploader.Application.Abstractions.Jobs;

/// <summary>
/// Исключение, возникающее, когда действие недопустимо в текущем состоянии задачи
/// </summary>
/// <param name="jobId">Идентификатор задачи</param>
/// <param name="status">Текущее состояние задачи</param>
/// <param name="action">Действие</param>
public class JobStateConflictException(Guid jobId, JobStatus status, string action)
  : Exception($"Action '{action}' is not allowed for job {jobId} in status {status}")
{
  /// <summary>
  /// Идентификатор задачи
  /// </summary>
  public Guid JobId { get; } = jobId;

  /// <summary>
  /// Текущее состояние задачи
  /// </summary>
  public JobStatus Status { get; } = status;

  /// <summary>
  /// Действие
  /// </summary>
  public string Action { get; } = action;
}

namespace Uploader.Application.Abstractions.Jobs;

/// <summary>
/// Исключение, возникающее, когда фоновая задача не найдена
/// </summary>
/// <param name="jobId">Идентификатор задачи</param>
public class JobNotFoundException(Guid jobId) : Exception($"Job {jobId} not found")
{
  /// <summary>
  /// Идентификатор задачи
  /// </summary>
  public Guid JobId { get; } = jobId;
}

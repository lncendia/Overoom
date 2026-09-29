namespace Uploader.Infrastructure.Web.Jobs.InputModels;

/// <summary>
/// Параметры пагинации списка задач
/// </summary>
public class GetJobsInputModel
{
  /// <summary>
  /// Количество пропускаемых задач
  /// </summary>
  public int Skip { get; init; }

  /// <summary>
  /// Количество возвращаемых задач
  /// </summary>
  public int Take { get; init; } = 20;
}

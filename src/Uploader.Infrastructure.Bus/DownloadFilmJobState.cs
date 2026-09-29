using Uploader.Application.Abstractions.Jobs;

namespace Uploader.Infrastructure.Bus;

/// <summary>
/// Состояние задачи загрузки фильма, сохраняемое между попытками выполнения
/// </summary>
public record DownloadFilmJobState
{
  /// <summary>
  /// Этап, с которого продолжается выполнение
  /// </summary>
  public JobStage Stage { get; init; } = JobStage.Downloading;

  /// <summary>
  /// Путь к скачанному исходному файлу
  /// </summary>
  public string? OriginalFilePath { get; init; }

  /// <summary>
  /// Каталог с результатом транскодирования
  /// </summary>
  public string? OutputPath { get; init; }
}

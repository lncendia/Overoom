using Common.Domain.Enums;

namespace Uploader.Application.Abstractions.Services;

/// <summary>
/// Сервис для транскодирования видео в HLS формат
/// Создает адаптивные стримы с несколькими вариантами качества
/// </summary>
public interface IHlsTranscodingService
{
    /// <summary>
    /// Транскодирует видео в формат HLS и сохраняет в указанный путь.
    /// </summary>
    /// <param name="inputPath">Путь к исходному видеофайлу</param>
    /// <param name="resolution">Исходное разрешение</param>
    /// <param name="outputPath">Путь, куда сохранить данные</param>
    /// <param name="cancellationToken">Токен отмены</param>
    Task TranscodeAsync(string inputPath, FilmResolution resolution, string outputPath, CancellationToken cancellationToken);
}

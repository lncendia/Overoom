using Common.Domain.Enums;

namespace Uploader.Infrastructure.Web.Queue.InputModels;

/// <summary>
/// Модель данных для постановки задачи транскодирования в очередь
/// </summary>
public class QueueInputModel
{
    /// <summary>
    /// Magnet-ссылка для скачивания торрента
    /// </summary>
    public string? MagnetUri { get; init; }
    
    /// <summary>
    /// Имя файла для обработки (опционально)
    /// </summary>
    public string? Filename { get; init; }
    
    /// <summary>
    /// Уникальный идентификатор фильма в системе
    /// </summary>
    public Guid FilmId { get; init; }
    
    /// <summary>
    /// Исходное разрешение для транскодирования фильма
    /// </summary>
    public FilmResolution Resolution { get; init; }
    
    /// <summary>
    /// Версия фильма
    /// </summary>
    public string? Version { get; init; }
    
    /// <summary>
    /// Номер сезона для сериалов (опционально)
    /// </summary>
    public int? Season { get; init; }
    
    /// <summary>
    /// Номер эпизода для сериалов (опционально)
    /// </summary>
    public int? Episode { get; init; }
}
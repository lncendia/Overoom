using Common.Domain.Enums;

namespace Uploader.Application.Abstractions.DTOs;

/// <summary>
/// Модель данных, представляющая метаинформацию о фильме
/// </summary>
public record FilmRecord
{
    /// <summary>Уникальный идентификатор фильма в системе</summary>
    public required Guid Id { get; init; }
    
    /// <summary>Разрешение видео (качество исходного файла)</summary>
    public required FilmResolution Resolution { get; init; }
    
    /// <summary>Версия видео (редакция, перевод, и т.д.)</summary>
    public required string Version { get; init; }
    
    /// <summary>Номер сезона для сериалов (опционально)</summary>
    public int? Season { get; init; }
    
    /// <summary>Номер эпизода для сериалов (опционально)</summary>
    public int? Episode { get; init; }
}
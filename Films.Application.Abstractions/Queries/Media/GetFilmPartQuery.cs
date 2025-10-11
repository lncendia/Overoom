using Common.Domain.Enums;
using MediatR;

namespace Films.Application.Abstractions.Queries.Media;

/// <summary>
/// Запрос для получения части медиафайла фильма
/// </summary>
public class GetFilmPartQuery : IRequest<FileResult>
{
    /// <summary>
    /// Идентификатор фильма
    /// </summary>
    public required Guid Id { get; init; }
    
    /// <summary>
    /// Разрешение видео (опционально)
    /// </summary>
    public FilmResolution? Resolution { get; set; }
    
    /// <summary>
    /// Версия медиафайла
    /// </summary>
    public required string Version { get; init; }
    
    /// <summary>
    /// Имя файла
    /// </summary>
    public required string FileName { get; init; }
    
    /// <summary>
    /// Номер сезона (для сериалов)
    /// </summary>
    public int? Season { get; init; }
    
    /// <summary>
    /// Номер эпизода (для сериалов)
    /// </summary>
    public int? Episode { get; init; }
}
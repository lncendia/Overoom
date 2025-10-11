namespace Films.Application.Abstractions.DTOs.Films;

/// <summary>
/// Базовый класс, представляющий медиаконтент с коллекцией версий.
/// </summary>
public class MediaContentDto
{
    /// <summary>
    /// Коллекция версий медиаконтента, доступная только для чтения.
    /// </summary>
    public required IReadOnlyList<string> Versions { get; init; }
}
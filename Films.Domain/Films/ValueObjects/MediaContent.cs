namespace Films.Domain.Films.ValueObjects;

/// <summary>
/// Базовый класс, представляющий медиаконтент с коллекцией версий.
/// </summary>
public class MediaContent
{
    /// <summary>
    /// Коллекция версий медиаконтента, доступная только для чтения.
    /// </summary>
    public required IReadOnlySet<string> Versions { get; init; }
}
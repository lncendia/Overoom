namespace Films.Application.Abstractions.DTOs.Films;

/// <summary>
/// Представляет сезон сериала, содержащий коллекцию эпизодов.
/// </summary>
public class SeasonDto
{
    /// <summary>
    /// Номер сезона. Используется для идентификации и сортировки.
    /// </summary>
    public required int Number { get; init; }

    /// <summary>
    /// Коллекция эпизодов в сезоне, доступная только для чтения.
    /// </summary>
    public required IReadOnlyList<EpisodeDto> Episodes { get; init; }
}
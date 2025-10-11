namespace Films.Application.Abstractions.DTOs.Films;

/// <summary>
/// Представляет эпизод сериала, содержащий информацию о версиях медиаконтента.
/// </summary>
public class EpisodeDto : MediaContentDto
{
    /// <summary>
    /// Номер эпизода в сезоне. Используется для идентификации и сортировки.
    /// </summary>
    public int Number { get; init; }
}
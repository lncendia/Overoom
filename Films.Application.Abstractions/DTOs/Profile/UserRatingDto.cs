using Films.Application.Abstractions.DTOs.Films;

namespace Films.Application.Abstractions.DTOs.Profile;

/// <summary>
/// DTO для передачи данных оценки пользователя с информацией о фильме
/// </summary>
public class UserRatingDto : FilmShortDto
{
    /// <summary>
    /// Оценка, поставленная пользователем
    /// </summary>
    public required double Score { get; init; }
}
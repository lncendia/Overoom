using Films.Application.Abstractions.DTOs.Profile;
using MediatR;

namespace Films.Application.Abstractions.Queries.Profile;

/// <summary>
/// Запрос для получения профиля пользователя
/// </summary>
public class GetUserProfileQuery : IRequest<UserProfileDto>
{
    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public required Guid Id { get; init; }
}
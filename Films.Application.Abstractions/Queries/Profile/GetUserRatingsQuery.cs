using Common.Application.DTOs;
using Films.Application.Abstractions.DTOs.Profile;
using MediatR;

namespace Films.Application.Abstractions.Queries.Profile;

/// <summary>
/// Запрос для получения оценок пользователя с пагинацией
/// </summary>
public class GetUserRatingsQuery : IRequest<CountResult<UserRatingDto>>
{
    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public required Guid Id { get; set; }
    
    /// <summary>
    /// Количество пропускаемых результатов
    /// </summary>
    public required int Skip { get; init; }
    
    /// <summary>
    /// Количество получаемых результатов
    /// </summary>
    public required int Take { get; init; }
}
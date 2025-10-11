using Common.Application.DTOs;
using Films.Application.Abstractions.DTOs.Rooms;
using MediatR;

namespace Films.Application.Abstractions.Queries.Rooms;

/// <summary>
/// Запрос для поиска комнат с фильтрами и пагинацией
/// </summary>
public class SearchRoomsQuery : IRequest<CountResult<RoomShortDto>>
{
    /// <summary>
    /// Фильтр по идентификатору фильма
    /// </summary>
    public Guid? FilmId { get; init; }
    
    /// <summary>
    /// Флаг поиска только публичных комнат
    /// </summary>
    public bool OnlyPublic { get; init; }
    
    /// <summary>
    /// Количество пропускаемых результатов
    /// </summary>
    public required int Skip { get; init; }
    
    /// <summary>
    /// Количество получаемых результатов
    /// </summary>
    public required int Take { get; init; }
}
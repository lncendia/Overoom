using Common.Application.DTOs;
using Films.Application.Abstractions.DTOs.Comments;
using MediatR;

namespace Films.Application.Abstractions.Queries.Comments;

/// <summary>
/// Запрос для получения комментариев к фильму с пагинацией
/// </summary>
public class GetFilmCommentsQuery : IRequest<CountResult<CommentDto>>
{
    /// <summary>
    /// Идентификатор фильма
    /// </summary>
    public required Guid FilmId { get; set; }
    
    /// <summary>
    /// Количество пропускаемых комментариев
    /// </summary>
    public required int Skip { get; init; }
    
    /// <summary>
    /// Количество получаемых комментариев
    /// </summary>
    public required int Take { get; init; }
}
using Films.Application.Abstractions.DTOs.Comments;
using MediatR;

namespace Films.Application.Abstractions.Queries.Comments;

/// <summary>
/// Запрос для получения последних комментариев
/// </summary>
public class GetLastCommentsQuery : IRequest<IReadOnlyList<CommentDto>>
{
    /// <summary>
    /// Количество получаемых комментариев
    /// </summary>
    public required int Take { get; init; }
}
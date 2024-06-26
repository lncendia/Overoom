using Films.Application.Abstractions.DTOs.Comments;
using MediatR;

namespace Films.Application.Abstractions.Queries.Comments;

public class FilmCommentsQuery : IRequest<(IReadOnlyCollection<CommentDto> comments, int count)>
{
    public required Guid FilmId { get; init; }
    public required int Skip { get; init; }
    public required int Take { get; init; }
}
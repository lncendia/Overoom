using Films.Application.Abstractions.DTOs.Comments;
using Films.Application.Abstractions.Queries.Comments;
using Films.Infrastructure.Storage.Context;
using MediatR;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Films.Application.Services.QueryHandlers.Comments;

/// <summary>
/// Обработчик запроса для получения комментариев к фильму.
/// </summary>
/// <param name="context">Контекст MongoDB</param>
public class GetLastCommentsQueryHandler(MongoDbContext context)
    : IRequestHandler<GetLastCommentsQuery, IReadOnlyList<CommentDto>>
{
    /// <summary>
    /// Обрабатывает запрос на последние комментарии и возвращает соответствующие комментарии.
    /// </summary>
    /// <param name="request">Запрос на комментарии.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Коллекция DTO комментариев.</returns>
    public async Task<IReadOnlyList<CommentDto>> Handle(GetLastCommentsQuery request, CancellationToken cancellationToken)
    {
        // Выполняем запрос и получаем список
        return await context.Comments.AsQueryable()
            .OrderByDescending(c => c.CreatedAt)
            .Take(request.Take)
            .GroupJoin(
                context.Users.AsQueryable(),
                comment => comment.UserId,
                user => user.Id,
                (comment, users) => new CommentDto
                {
                    Id = comment.Id,
                    UserId = comment.UserId,
                    Text = comment.Text,
                    CreatedAt = comment.CreatedAt,
                    UserName = users.First().Username
                }
            )
            .ToListAsync(cancellationToken);
    }
}
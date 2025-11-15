using Common.Application.DTOs;
using Films.Application.Abstractions.DTOs.Comments;
using Films.Application.Abstractions.Queries.Comments;
using Films.Infrastructure.Storage.Context;
using MediatR;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Films.Application.Services.QueryHandlers.Comments;


/// <summary>
/// Обработчик запроса для получения комментариев к фильму
/// </summary>
/// <param name="context">Контекст MongoDB</param>
public class GetFilmCommentsQueryHandler(MongoDbContext context)
    : IRequestHandler<GetFilmCommentsQuery, CountResult<CommentDto>>
{
    /// <summary>
    /// Обработка запроса на получение комментариев к фильму
    /// </summary>
    /// <param name="request">Запрос с параметрами</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Результат с коллекцией комментариев и общим количеством</returns>
    public async Task<CountResult<CommentDto>> Handle(GetFilmCommentsQuery request, CancellationToken cancellationToken)
    {
        // Создаем базовый запрос для комментариев
        var baseQuery = context.Comments.AsQueryable()
            .Where(x => x.FilmId == request.FilmId);

        // Получаем общее количество комментариев для фильма
        var count = await baseQuery.CountAsync(cancellationToken: cancellationToken);

        // Если комментариев нет, возвращаем пустой результат
        if (count == 0) return CountResult<CommentDto>.NoValues();

        // Выполняем запрос и получаем список
        var list = await baseQuery
            .OrderByDescending(c => c.CreatedAt)
            .Skip(request.Skip)
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
                    // ReSharper disable once PossibleMultipleEnumeration
                    UserName = users.First().Username,
                    // ReSharper disable once PossibleMultipleEnumeration
                    PhotoKey = users.First().PhotoKey
                }
            )
            .ToListAsync(cancellationToken);

        // Возвращаем результат с данными и общим количеством
        return new CountResult<CommentDto>
        {
            List = list,
            TotalCount = count
        };
    }
}
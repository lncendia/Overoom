using Common.Application.DTOs;
using Films.Application.Abstractions.DTOs.Comments;
using Films.Application.Abstractions.Queries.Comments;
using Films.Infrastructure.Storage.Context;
using Films.Infrastructure.Storage.Models.Comments;

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
    IQueryable<CommentModel> baseQuery = context.Comments.AsQueryable()
      .Where(x => x.FilmId == request.FilmId);

    int count = await baseQuery.CountAsync(cancellationToken: cancellationToken);
    if (count == 0) return CountResult<CommentDto>.NoValues();

    List<CommentDto>? list = await baseQuery
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

    return new CountResult<CommentDto>
    {
      List = list,
      TotalCount = count
    };
  }
}
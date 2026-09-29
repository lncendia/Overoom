using Common.Application.DTOs;
using Films.Application.Abstractions.DTOs.Comments;
using Films.Application.Abstractions.Queries.Comments;
using Films.Infrastructure.Storage.Context;
using Films.Infrastructure.Storage.Models.Comments;

using MediatR;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Films.Infrastructure.Storage.QueryHandlers.Comments;

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

    List<CommentRow> rows = await baseQuery
      .OrderByDescending(c => c.CreatedAt)
      .Skip(request.Skip)
      .Take(request.Take)
      .GroupJoin(
        context.Users.AsQueryable(),
        comment => comment.UserId,
        user => user.Id,
        (comment, users) => new CommentRow
        {
          Id = comment.Id,
          UserId = comment.UserId,
          Text = comment.Text,
          CreatedAt = comment.CreatedAt,
          ReactionCounts = comment.ReactionCounts,
          // ReSharper disable once PossibleMultipleEnumeration
          UserName = users.First().Username,
          // ReSharper disable once PossibleMultipleEnumeration
          PhotoKey = users.First().PhotoKey
        }
      )
      .ToListAsync(cancellationToken);

    HashSet<(Guid CommentId, string Reaction)> userReactions = await GetUserReactionsAsync(
      request.UserId, rows.Select(r => r.Id).ToList(), cancellationToken);

    List<CommentDto> list = rows.Select(r => new CommentDto
    {
      Id = r.Id,
      UserId = r.UserId,
      Text = r.Text,
      CreatedAt = r.CreatedAt,
      UserName = r.UserName,
      PhotoKey = r.PhotoKey,
      Reactions = (r.ReactionCounts ?? [])
        .Where(c => c.Value > 0)
        .Select(c => new CommentReactionDto
        {
          Reaction = c.Key,
          Count = c.Value,
          Reacted = userReactions.Contains((r.Id, c.Key))
        })
        .ToList()
    }).ToList();

    return new CountResult<CommentDto>
    {
      List = list,
      TotalCount = count
    };
  }

  /// <summary>
  /// Получает реакции, поставленные пользователем на указанные комментарии
  /// </summary>
  /// <param name="userId">Идентификатор пользователя или null для анонимного запроса</param>
  /// <param name="commentIds">Идентификаторы комментариев</param>
  /// <param name="cancellationToken">Токен отмены</param>
  /// <returns>Пары (комментарий, реакция)</returns>
  private async Task<HashSet<(Guid CommentId, string Reaction)>> GetUserReactionsAsync(
    Guid? userId, List<Guid> commentIds, CancellationToken cancellationToken)
  {
    if (userId == null || commentIds.Count == 0) return [];

    var reactions = await context.CommentReactions.AsQueryable()
      .Where(r => r.UserId == userId && commentIds.Contains(r.CommentId))
      .Select(r => new { r.CommentId, r.Reaction })
      .ToListAsync(cancellationToken);

    return reactions.Select(r => (r.CommentId, r.Reaction)).ToHashSet();
  }

  /// <summary>
  /// Проекция комментария с автором и счётчиками реакций
  /// </summary>
  private class CommentRow
  {
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string Text { get; init; } = null!;
    public DateTime CreatedAt { get; init; }
    public Dictionary<string, int>? ReactionCounts { get; init; }
    public string UserName { get; init; } = null!;
    public string? PhotoKey { get; init; }
  }
}
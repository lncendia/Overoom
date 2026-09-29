using Common.Domain.Specifications.Abstractions;
using Films.Domain.CommentReactions.Specifications.Visitor;

namespace Films.Domain.CommentReactions.Specifications;

/// <summary>
/// Конкретная реакция пользователя на комментарий
/// </summary>
public class UserCommentReactionSpecification(Guid commentId, Guid userId, string reaction)
  : ISpecification<CommentReaction, ICommentReactionSpecificationVisitor>
{
  public Guid CommentId { get; } = commentId;

  public Guid UserId { get; } = userId;

  public string Reaction { get; } = reaction;

  public void Accept(ICommentReactionSpecificationVisitor visitor)
  {
    visitor.Visit(this);
  }

  public bool IsSatisfiedBy(CommentReaction item)
  {
    return item.CommentId == CommentId && item.UserId == UserId && item.Reaction == Reaction;
  }
}

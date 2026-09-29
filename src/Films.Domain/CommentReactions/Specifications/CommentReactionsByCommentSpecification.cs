using Common.Domain.Specifications.Abstractions;
using Films.Domain.CommentReactions.Specifications.Visitor;

namespace Films.Domain.CommentReactions.Specifications;

/// <summary>
/// Все реакции на комментарий
/// </summary>
public class CommentReactionsByCommentSpecification(Guid commentId)
  : ISpecification<CommentReaction, ICommentReactionSpecificationVisitor>
{
  public Guid CommentId { get; } = commentId;

  public void Accept(ICommentReactionSpecificationVisitor visitor)
  {
    visitor.Visit(this);
  }

  public bool IsSatisfiedBy(CommentReaction item)
  {
    return item.CommentId == CommentId;
  }
}

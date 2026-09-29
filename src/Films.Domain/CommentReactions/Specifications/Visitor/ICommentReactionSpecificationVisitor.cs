using Common.Domain.Specifications.Abstractions;

namespace Films.Domain.CommentReactions.Specifications.Visitor;

public interface ICommentReactionSpecificationVisitor
  : ISpecificationVisitor<ICommentReactionSpecificationVisitor, CommentReaction>
{
  void Visit(UserCommentReactionSpecification specification);

  void Visit(CommentReactionsByCommentSpecification specification);
}

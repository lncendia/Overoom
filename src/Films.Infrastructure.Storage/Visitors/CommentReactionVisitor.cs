using System.Linq.Expressions;
using Common.Domain.Specifications.Abstractions;
using Common.Infrastructure.Repositories.Visitors;
using Films.Domain.CommentReactions;
using Films.Domain.CommentReactions.Specifications;
using Films.Domain.CommentReactions.Specifications.Visitor;
using Films.Infrastructure.Storage.Models.CommentReactions;

namespace Films.Infrastructure.Storage.Visitors;

public class CommentReactionVisitor
  : BaseSpecificationVisitor<CommentReactionModel, ICommentReactionSpecificationVisitor, CommentReaction>,
    ICommentReactionSpecificationVisitor
{
  protected override Expression<Func<CommentReactionModel, bool>> ConvertSpecToExpression(
    ISpecification<CommentReaction, ICommentReactionSpecificationVisitor> spec)
  {
    var visitor = new CommentReactionVisitor();
    spec.Accept(visitor);
    return visitor.Expr!;
  }

  public void Visit(UserCommentReactionSpecification specification)
  {
    Expr = x => x.CommentId == specification.CommentId
                && x.UserId == specification.UserId
                && x.Reaction == specification.Reaction;
  }

  public void Visit(CommentReactionsByCommentSpecification specification)
  {
    Expr = x => x.CommentId == specification.CommentId;
  }
}

using System.Linq.Expressions;
using Common.Domain.Specifications.Abstractions;
using Common.Infrastructure.Repositories.Visitors;
using Films.Domain.Comments;
using Films.Domain.Comments.Specifications.Visitor;
using Films.Infrastructure.Storage.Models.Comments;

namespace Films.Infrastructure.Storage.Visitors;

public class CommentVisitor : BaseSpecificationVisitor<CommentModel, ICommentSpecificationVisitor, Comment>, ICommentSpecificationVisitor
{
    protected override Expression<Func<CommentModel, bool>> ConvertSpecToExpression(ISpecification<Comment, ICommentSpecificationVisitor> spec)
    {
        var visitor = new CommentVisitor();
        spec.Accept(visitor);
        return visitor.Expr!;
    }
}
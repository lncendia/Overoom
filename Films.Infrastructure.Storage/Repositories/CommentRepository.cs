using System.Linq.Expressions;

using Common.Domain.Specifications.Abstractions;
using Common.Infrastructure.Repositories;
using Films.Domain.Comments;
using Films.Domain.Comments.Snapshots;
using Films.Domain.Comments.Specifications.Visitor;
using Films.Domain.Repositories;
using Films.Infrastructure.Storage.Context;
using Films.Infrastructure.Storage.Models.Comments;
using Films.Infrastructure.Storage.Visitors;

using Incendia.MongoTracker.Builders;

namespace Films.Infrastructure.Storage.Repositories;

/// <summary>
/// Реализация репозитория для хранения комментариев.
/// </summary>
public class CommentRepository : RepositoryBase<CommentModel, Comment, CommentSnapshot, ICommentSpecificationVisitor>, ICommentRepository
{
  /// <summary>
  /// Конструктор
  /// </summary>
  /// <param name="context">MongoDB-контекст приложения</param>
  /// <param name="config">Конфигурация трекера моделей</param>
  public CommentRepository(MongoDbContext context, ModelBuilder config) : base(config, context.Comments)
  {
  }

  /// <inheritdoc/>
  protected override Expression<Func<CommentModel, bool>>? SpecificationVisitor(
    ISpecification<Comment, ICommentSpecificationVisitor> spec)
  {
    var visitor = new CommentVisitor();
    spec.Accept(visitor);

    return visitor.Expr;
  }

  /// <inheritdoc/>
  protected override CommentModel FactoryMethod(Comment aggregate)
  {
    return new CommentModel
    {
      Id = aggregate.Id
    };
  }
}

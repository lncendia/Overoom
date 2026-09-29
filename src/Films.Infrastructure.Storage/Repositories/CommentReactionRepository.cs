using System.Linq.Expressions;

using Common.Domain.Specifications.Abstractions;
using Common.Infrastructure.Repositories;
using Common.Infrastructure.Transactions;

using Films.Domain.CommentReactions;
using Films.Domain.CommentReactions.Snapshots;
using Films.Domain.CommentReactions.Specifications.Visitor;
using Films.Domain.Repositories;
using Films.Infrastructure.Storage.Context;
using Films.Infrastructure.Storage.Models.CommentReactions;
using Films.Infrastructure.Storage.Visitors;

using Incendia.MongoTracker.Builders;

namespace Films.Infrastructure.Storage.Repositories;

/// <summary>
/// Реализация репозитория для хранения реакций на комментарии.
/// </summary>
public class CommentReactionRepository
  : RepositoryBase<CommentReactionModel, CommentReaction, CommentReactionSnapshot, ICommentReactionSpecificationVisitor>,
    ICommentReactionRepository
{
  /// <summary>
  /// Конструктор
  /// </summary>
  /// <param name="context">MongoDB-контекст приложения</param>
  /// <param name="config">Конфигурация трекера моделей</param>
  /// <param name="transaction">Текущая транзакция области</param>
  public CommentReactionRepository(MongoDbContext context, ModelBuilder config, ITransactionContext transaction)
    : base(config, context.CommentReactions, transaction)
  {
  }

  /// <inheritdoc/>
  protected override Expression<Func<CommentReactionModel, bool>>? SpecificationVisitor(
    ISpecification<CommentReaction, ICommentReactionSpecificationVisitor> spec)
  {
    var visitor = new CommentReactionVisitor();
    spec.Accept(visitor);

    return visitor.Expr;
  }

  /// <inheritdoc/>
  protected override CommentReactionModel FactoryMethod(CommentReaction aggregate)
  {
    return new CommentReactionModel
    {
      Id = aggregate.Id
    };
  }
}

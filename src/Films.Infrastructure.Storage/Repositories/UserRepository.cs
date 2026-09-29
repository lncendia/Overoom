using System.Linq.Expressions;

using Common.Domain.Specifications.Abstractions;
using Common.Infrastructure.Repositories;
using Common.Infrastructure.Transactions;

using Films.Domain.Repositories;
using Films.Domain.Users;
using Films.Domain.Users.Snapshots;
using Films.Domain.Users.Specifications.Visitor;
using Films.Infrastructure.Storage.Context;
using Films.Infrastructure.Storage.Models.Users;
using Films.Infrastructure.Storage.Visitors;

using Incendia.MongoTracker.Builders;

namespace Films.Infrastructure.Storage.Repositories;

/// <summary>
/// Реализация репозитория для хранения пользователей.
/// </summary>
public class UserRepository : RepositoryBase<UserModel, User, UserSnapshot, IUserSpecificationVisitor>, IUserRepository
{
  /// <summary>
  /// Конструктор
  /// </summary>
  /// <param name="context">MongoDB-контекст приложения</param>
  /// <param name="config">Конфигурация трекера моделей</param>
  /// <param name="transaction">Текущая транзакция области</param>
  public UserRepository(MongoDbContext context, ModelBuilder config, ITransactionContext transaction)
    : base(config, context.Users, transaction)
  {
  }

  /// <inheritdoc/>
  protected override Expression<Func<UserModel, bool>>? SpecificationVisitor(ISpecification<User, IUserSpecificationVisitor> spec)
  {
    var visitor = new UserVisitor();
    spec.Accept(visitor);

    return visitor.Expr;
  }

  /// <inheritdoc/>
  protected override UserModel FactoryMethod(User aggregate)
  {
    return new UserModel
    {
      Id = aggregate.Id
    };
  }
}

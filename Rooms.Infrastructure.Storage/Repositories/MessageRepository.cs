using System.Linq.Expressions;

using Common.Domain.Specifications.Abstractions;
using Common.Infrastructure.Repositories;

using Incendia.MongoTracker.Builders;

using Rooms.Domain.Messages;
using Rooms.Domain.Messages.Snapshots;
using Rooms.Domain.Messages.Specifications.Visitor;
using Rooms.Domain.Repositories;
using Rooms.Infrastructure.Storage.Context;
using Rooms.Infrastructure.Storage.Models.Messages;
using Rooms.Infrastructure.Storage.Visitors;

namespace Rooms.Infrastructure.Storage.Repositories;

/// <summary>
/// Реализация репозитория для хранения сообщений.
/// </summary>
public class MessageRepository : RepositoryBase<MessageModel, Message, MessageSnapshot, IMessageSpecificationVisitor>, IMessageRepository
{
  /// <summary>
  /// Конструктор
  /// </summary>
  /// <param name="context">MongoDB-контекст приложения</param>
  /// <param name="config">Конфигурация трекера моделей</param>
  public MessageRepository(MongoDbContext context, ModelBuilder config) : base(config, context.Messages)
  {
  }

  /// <inheritdoc/>
  protected override Expression<Func<MessageModel, bool>>? SpecificationVisitor(ISpecification<Message, IMessageSpecificationVisitor> spec)
  {
    var visitor = new MessageVisitor();
    spec.Accept(visitor);

    return visitor.Expr;
  }

  /// <inheritdoc/>
  protected override MessageModel FactoryMethod(Message aggregate)
  {
    return new MessageModel
    {
      Id = aggregate.Id
    };
  }

  /// <inheritdoc/>
  protected override Message FromSnapshot(MessageSnapshot snapshot)
  {
    return Message.FromSnapshot(snapshot);
  }
}

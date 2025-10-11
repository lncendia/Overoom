using System.Linq.Expressions;
using Common.Domain.Specifications.Abstractions;
using Common.Infrastructure.Repositories.Visitors;
using Rooms.Domain.Messages;
using Rooms.Domain.Messages.Specifications;
using Rooms.Domain.Messages.Specifications.Visitor;
using Rooms.Infrastructure.Storage.Models.Messages;

namespace Rooms.Infrastructure.Storage.Visitors;

public class MessageVisitor :
    BaseSpecificationVisitor<MessageModel, IMessageSpecificationVisitor, Message>,
    IMessageSpecificationVisitor
{
    protected override Expression<Func<MessageModel, bool>> ConvertSpecToExpression(
        ISpecification<Message, IMessageSpecificationVisitor> spec)
    {
        var visitor = new MessageVisitor();
        spec.Accept(visitor);
        return visitor.Expr!;
    }

    public void Visit(RoomMessagesSpecification spec) => Expr = m => m.RoomId == spec.RoomId;
}
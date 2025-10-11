using Common.Domain.Specifications.Abstractions;

namespace Rooms.Domain.Messages.Specifications.Visitor;

public interface IMessageSpecificationVisitor : ISpecificationVisitor<IMessageSpecificationVisitor, Message>
{
    void Visit(RoomMessagesSpecification spec);
}
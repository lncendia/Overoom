using Common.Domain.Specifications.Abstractions;
using Rooms.Domain.Messages.Specifications.Visitor;

namespace Rooms.Domain.Messages.Specifications;

public class RoomMessagesSpecification(Guid roomId) : ISpecification<Message, IMessageSpecificationVisitor>
{
    public Guid RoomId { get; } = roomId;

    public void Accept(IMessageSpecificationVisitor visitor) => visitor.Visit(this);
    public bool IsSatisfiedBy(Message item) => item.RoomId == RoomId;
}
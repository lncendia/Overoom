using Common.Domain.Specifications.Abstractions;
using Rooms.Domain.Rooms.Specifications.Visitor;

namespace Rooms.Domain.Rooms.Specifications;

public class RoomsByViewerSpecification(Guid userId) : ISpecification<Room, IRoomSpecificationVisitor>
{
    public Guid UserId { get; } = userId;

    public void Accept(IRoomSpecificationVisitor visitor) => visitor.Visit(this);
    public bool IsSatisfiedBy(Room item) => item.Viewers.Any(u => u.Key == UserId);
}
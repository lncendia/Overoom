using Common.Domain.Specifications.Abstractions;
using Films.Domain.Rooms.Specifications.Visitor;

namespace Films.Domain.Rooms.Specifications;

public class RoomByUserSpecification(Guid userId) : ISpecification<Room, IRoomSpecificationVisitor>
{
    public Guid UserId { get; } = userId;

    public void Accept(IRoomSpecificationVisitor visitor) => visitor.Visit(this);
    public bool IsSatisfiedBy(Room item) => item.Viewers.Any(u => u == UserId);
}
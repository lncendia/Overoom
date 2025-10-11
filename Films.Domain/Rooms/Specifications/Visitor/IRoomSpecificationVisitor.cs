using Common.Domain.Specifications.Abstractions;

namespace Films.Domain.Rooms.Specifications.Visitor;

public interface IRoomSpecificationVisitor : ISpecificationVisitor<IRoomSpecificationVisitor, Room>
{
    void Visit(RoomByUserSpecification spec);
}
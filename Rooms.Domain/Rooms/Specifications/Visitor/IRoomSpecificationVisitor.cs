using Common.Domain.Specifications.Abstractions;

namespace Rooms.Domain.Rooms.Specifications.Visitor;

public interface IRoomSpecificationVisitor : ISpecificationVisitor<IRoomSpecificationVisitor, Room>
{
    void Visit(RoomsByViewerSpecification spec);
}
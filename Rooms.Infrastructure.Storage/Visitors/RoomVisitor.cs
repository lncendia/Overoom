using System.Linq.Expressions;
using Common.Domain.Specifications.Abstractions;
using Common.Infrastructure.Repositories.Visitors;
using Rooms.Domain.Rooms;
using Rooms.Domain.Rooms.Specifications;
using Rooms.Domain.Rooms.Specifications.Visitor;
using Rooms.Infrastructure.Storage.Models.Rooms;

namespace Rooms.Infrastructure.Storage.Visitors;

public class RoomVisitor : BaseSpecificationVisitor<RoomModel, IRoomSpecificationVisitor, Room>,
    IRoomSpecificationVisitor
{
    protected override Expression<Func<RoomModel, bool>> ConvertSpecToExpression(
        ISpecification<Room, IRoomSpecificationVisitor> spec)
    {
        var visitor = new RoomVisitor();
        spec.Accept(visitor);
        return visitor.Expr!;
    }

    public void Visit(RoomsByViewerSpecification spec) => Expr = x => x.Viewers.Any(v => v.Id == spec.UserId);
}
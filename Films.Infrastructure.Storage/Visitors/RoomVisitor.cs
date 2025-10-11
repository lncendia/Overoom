using System.Linq.Expressions;
using Common.Domain.Specifications.Abstractions;
using Common.Infrastructure.Repositories.Visitors;
using Films.Domain.Rooms;
using Films.Domain.Rooms.Specifications;
using Films.Domain.Rooms.Specifications.Visitor;
using Films.Infrastructure.Storage.Models.Rooms;

namespace Films.Infrastructure.Storage.Visitors;

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

    public void Visit(RoomByUserSpecification spec) => Expr = x => x.Viewers.Contains(spec.UserId);
}
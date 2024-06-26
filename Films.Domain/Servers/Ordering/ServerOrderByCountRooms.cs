﻿using Films.Domain.Ordering.Abstractions;
using Films.Domain.Servers.Ordering.Visitor;

namespace Films.Domain.Servers.Ordering;

public class ServerOrderByCountRooms : IOrderBy<Server, IServerSortingVisitor>
{
    public IEnumerable<Server> Order(IEnumerable<Server> items) => items.OrderBy(x => x.RoomsCount);

    public IReadOnlyCollection<IEnumerable<Server>> Divide(IEnumerable<Server> items) =>
        Order(items).GroupBy(x => x.RoomsCount).Select(x => x.AsEnumerable()).ToArray();

    public void Accept(IServerSortingVisitor visitor) => visitor.Visit(this);
}
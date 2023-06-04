﻿using Overoom.Domain.Ordering.Abstractions;
using Overoom.Domain.Room.YoutubeRoom.Ordering.Visitor;

namespace Overoom.Domain.Room.YoutubeRoom.Ordering;

public class OrderByLastActivityDate : IOrderBy<Entities.YoutubeRoom, IYoutubeRoomSortingVisitor>
{
    public IEnumerable<Entities.YoutubeRoom> Order(IEnumerable<Entities.YoutubeRoom> items) => items.OrderBy(x => x.LastActivity);

    public IList<IEnumerable<Entities.YoutubeRoom>> Divide(IEnumerable<Entities.YoutubeRoom> items) =>
        Order(items).GroupBy(x => x.LastActivity).Select(x => x.AsEnumerable()).ToList();

    public void Accept(IYoutubeRoomSortingVisitor visitor) => visitor.Visit(this);
}
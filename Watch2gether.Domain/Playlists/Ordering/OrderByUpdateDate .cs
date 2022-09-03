﻿using Watch2gether.Domain.Ordering.Abstractions;
using Watch2gether.Domain.Playlists.Ordering.Visitor;

namespace Watch2gether.Domain.Playlists.Ordering;

public class OrderByUpdateDate : IOrderBy<Playlist, IPlaylistSortingVisitor>
{
    public IEnumerable<Playlist> Order(IEnumerable<Playlist> items) => items.OrderBy(x => x.Updated);

    public IList<IEnumerable<Playlist>> Divide(IEnumerable<Playlist> items) =>
        Order(items).GroupBy(x => x.Updated).Select(x => x.AsEnumerable()).ToList();

    public void Accept(IPlaylistSortingVisitor visitor) => visitor.Visit(this);
}
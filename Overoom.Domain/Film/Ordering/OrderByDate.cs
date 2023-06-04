﻿using Overoom.Domain.Film.Ordering.Visitor;
using Overoom.Domain.Ordering.Abstractions;

namespace Overoom.Domain.Film.Ordering;

public class OrderByDate : IOrderBy<Film.Entities.Film, IFilmSortingVisitor>
{
    public IEnumerable<Film.Entities.Film> Order(IEnumerable<Film.Entities.Film> items) => items.OrderBy(x => x.Date);

    public IList<IEnumerable<Film.Entities.Film>> Divide(IEnumerable<Film.Entities.Film> items) =>
        Order(items).GroupBy(x => x.Date).Select(x => x.AsEnumerable()).ToList();

    public void Accept(IFilmSortingVisitor visitor) => visitor.Visit(this);
}
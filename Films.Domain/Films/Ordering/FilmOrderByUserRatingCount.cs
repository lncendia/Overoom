﻿using Films.Domain.Films.Entities;
using Films.Domain.Films.Ordering.Visitor;
using Films.Domain.Ordering.Abstractions;

namespace Films.Domain.Films.Ordering;

public class FilmOrderByUserRatingCount : IOrderBy<Film, IFilmSortingVisitor>
{
    public IEnumerable<Film> Order(IEnumerable<Film> items) => items.OrderBy(x => x.UserRatingsCount);
    public IList<IEnumerable<Film>> Divide(IEnumerable<Film> items) =>
        Order(items).GroupBy(x => x.UserRatingsCount).Select(x => x.AsEnumerable()).ToList();

    public void Accept(IFilmSortingVisitor visitor) => visitor.Visit(this);
}
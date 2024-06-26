﻿using Films.Domain.Comments.Ordering.Visitor;
using Films.Domain.Ordering.Abstractions;

namespace Films.Domain.Comments.Ordering;

public class CommentOrderByDate : IOrderBy<Comment, ICommentSortingVisitor>
{
    public IEnumerable<Comment> Order(IEnumerable<Comment> items) => items.OrderBy(x => x.CreatedAt);

    public IReadOnlyCollection<IEnumerable<Comment>> Divide(IEnumerable<Comment> items) =>
        Order(items).GroupBy(x => x.CreatedAt).Select(x => x.AsEnumerable()).ToArray();

    public void Accept(ICommentSortingVisitor visitor) => visitor.Visit(this);
}